using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Компонент для управления шлангом в руках игрока.
/// Обеспечивает возможность очистки плесени с помощью шланга на большом расстоянии.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class HoseEquipment : MonoBehaviour
{
    [Header("Hose Settings")]
    [SerializeField] private float hoseRange = 8f; // Дальность шланга
    [SerializeField] private LayerMask moldMask = 1 << 6; // Слой объектов с плесенью (слой 6)
    [SerializeField] private float paintInterval = 0.01f; // Интервал между стираниями
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite hoseSprite; // Спрайт шланга в руках
    [SerializeField] private Vector3 hoseOffset = new Vector3(0.3f, 0.2f, 0f); // Смещение относительно игрока
    
    private HoseInteractable originalHose;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private SimpleWaterController waterController;
    private JetVFX jetVFX;
    private Camera mainCamera;
    private float lastPaintTime;
    private bool isEquipped = false;
    
    public bool IsHoseEquipped => isEquipped;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    private void Update()
    {
        if (!isEquipped) return;
        
        HandleHoseInput();
        UpdateHoseVisual();
    }
    
    /// <summary>
    /// Экипирует шланг
    /// </summary>
    public void EquipHose(HoseInteractable hose)
    {
        originalHose = hose;
        isEquipped = true;
        
        // Меняем спрайт игрока на спрайт с шлангом
        if (hoseSprite != null)
        {
            spriteRenderer.sprite = hoseSprite;
        }
        
        // Создаем компоненты для струи воды
        CreateWaterJetComponents();
        
        Debug.Log($"[HoseEquipment] Шланг экипирован игроком {gameObject.name}");
    }
    
    /// <summary>
    /// Снимает шланг
    /// </summary>
    public void UnequipHose()
    {
        if (!isEquipped) return;
        
        // Возвращаем оригинальный спрайт
        spriteRenderer.sprite = originalSprite;
        
        // Уничтожаем компоненты струи воды
        DestroyWaterJetComponents();
        
        // Сохраняем ссылку на шланг перед сбросом
        HoseInteractable hoseToNotify = originalHose;
        
        isEquipped = false;
        originalHose = null;
        
        // Уведомляем шланг о возврате (ForceReturnHose имеет защиту от рекурсии)
        if (hoseToNotify != null)
        {
            hoseToNotify.ForceReturnHose();
        }
        
        Debug.Log($"[HoseEquipment] Шланг снят игроком {gameObject.name}");
    }
    
    private void CreateWaterJetComponents()
    {
        // Создаем точку выхода воды (nozzle)
        GameObject nozzle = new GameObject("HoseNozzle");
        nozzle.transform.SetParent(transform);
        nozzle.transform.localPosition = hoseOffset;
        
        // Добавляем SimpleWaterController
        waterController = gameObject.AddComponent<SimpleWaterController>();
        SetupWaterController();
        
        // Создаем визуальные эффекты
        CreateVFXComponents(nozzle);
    }
    
    private void SetupWaterController()
    {
        if (waterController == null) return;
        
        // Настраиваем радиус полива равный дальности шланга
        var wateringRangeField = typeof(SimpleWaterController).GetField("wateringRange", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        wateringRangeField?.SetValue(waterController, hoseRange);
        
        Debug.Log("[HoseEquipment] SimpleWaterController настроен с радиусом полива: " + hoseRange);
    }
    
    private void CreateVFXComponents(GameObject nozzle)
    {
        // Создаем объект для визуальных эффектов
        GameObject vfxObject = new GameObject("HoseVFX");
        vfxObject.transform.SetParent(nozzle.transform);
        
        // Добавляем LineRenderer и JetVFX
        LineRenderer lineRenderer = vfxObject.AddComponent<LineRenderer>();
        jetVFX = vfxObject.AddComponent<JetVFX>();
        
        // Настраиваем LineRenderer для шланга
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.sortingOrder = 15; // Поверх других объектов
        
        // Настраиваем nozzle в JetVFX
        var nozzleField = typeof(JetVFX).GetField("nozzle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nozzleField?.SetValue(jetVFX, nozzle.transform);
        
        // SimpleWaterController автоматически создает свои эффекты
        
        Debug.Log("[HoseEquipment] VFX компоненты созданы");
    }
    
    
    private void DestroyWaterJetComponents()
    {
        // Уничтожаем SimpleWaterController
        if (waterController != null)
        {
            DestroyImmediate(waterController);
            waterController = null;
        }
        
        if (jetVFX != null)
        {
            DestroyImmediate(jetVFX.gameObject);
            jetVFX = null;
        }
        
        // Уничтожаем nozzle
        Transform nozzle = transform.Find("HoseNozzle");
        if (nozzle != null)
        {
            DestroyImmediate(nozzle.gameObject);
        }
    }
    
    private void HandleHoseInput()
    {
        // SimpleWaterController автоматически обрабатывает ввод
        // Здесь можно добавить дополнительную логику, если нужно
    }
    
    
    private bool GetFireInput()
    {
        // Используем Input System через Mouse
        var mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.leftButton.isPressed;
        }
        
        // Fallback - возвращаем false
        return false;
    }
    
    private void UpdateHoseVisual()
    {
        // Обновляем визуальное отображение шланга
        if (jetVFX != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            mouseWorldPos.z = 0f;
            
            Vector3 hosePos = transform.position + hoseOffset;
            Vector3 direction = (mouseWorldPos - hosePos).normalized;
            float distance = Mathf.Min(Vector3.Distance(hosePos, mouseWorldPos), hoseRange);
            
            Vector3 endPoint = hosePos + direction * distance;
            
            // Показываем струю только при нажатой ЛКМ и наличии воды
            if (GetFireInput() && HasWaterInInventory())
            {
                jetVFX.DrawTo(endPoint);
            }
            else
            {
                jetVFX.Hide();
            }
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        // Используем Input System для получения позиции мыши
        var mouse = Mouse.current;
        if (mouse != null)
        {
            return Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        }
        
        // Fallback - возвращаем позицию игрока
        return transform.position;
    }
    
    private bool HasWaterInInventory()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
            return false;
        
        var wateringCan = InventorySystem.Instance.GetWateringCan();
        return wateringCan != null && wateringCan.HasWater;
    }
    
    private void OnDestroy()
    {
        if (isEquipped)
        {
            UnequipHose();
        }
    }
}
