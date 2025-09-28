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
    [SerializeField] private float waterConsumption = 0.5f; // Расход воды за стирание
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite hoseSprite; // Спрайт шланга в руках
    [SerializeField] private Vector3 hoseOffset = new Vector3(0.3f, 0.2f, 0f); // Смещение относительно игрока
    
    private HoseInteractable originalHose;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private WaterJetController waterJetController;
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
        
        // Возвращаем шланг обратно
        if (originalHose != null)
        {
            originalHose.ForceReturnHose();
        }
        
        isEquipped = false;
        originalHose = null;
        
        Debug.Log($"[HoseEquipment] Шланг снят игроком {gameObject.name}");
    }
    
    private void CreateWaterJetComponents()
    {
        // Создаем точку выхода воды (nozzle)
        GameObject nozzle = new GameObject("HoseNozzle");
        nozzle.transform.SetParent(transform);
        nozzle.transform.localPosition = hoseOffset;
        
        // Создаем визуальные эффекты
        CreateVFXComponents(nozzle);
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
        // lineRenderer.color = Color.blue;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.sortingOrder = 15; // Поверх других объектов
        
        // Сохраняем ссылку на VFX
        // jetVFX уже сохранен в переменной класса
        
        // Настраиваем nozzle в JetVFX
        var nozzleField = typeof(JetVFX).GetField("nozzle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nozzleField?.SetValue(jetVFX, nozzle.transform);
    }
    
    private void DestroyWaterJetComponents()
    {
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
        // Проверяем наличие воды в лейке
        if (InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan)
        {
            var wateringCan = InventorySystem.Instance.GetWateringCan();
            if (wateringCan != null && wateringCan.HasWater)
            {
                // Вода есть - активируем шланг при зажатой ЛКМ
                bool fireInput = GetFireInput();
                if (fireInput)
                {
                    Debug.Log($"[HoseEquipment] Fire input detected, time since last paint: {Time.time - lastPaintTime}");
                    
                    if (Time.time - lastPaintTime >= paintInterval)
                    {
                        Debug.Log("[HoseEquipment] Paint interval passed, performing raycast");
                        
                        // Расходуем воду
                        wateringCan.UseWater(waterConsumption);
                        lastPaintTime = Time.time;
                        
                        // Выполняем raycast для очистки плесени
                        PerformHoseRaycast();
                    }
                }
            }
            else
            {
                Debug.Log("[HoseEquipment] Watering can has no water");
            }
        }
        else
        {
            Debug.Log("[HoseEquipment] No watering can in inventory");
        }
    }
    
    private void PerformHoseRaycast()
    {
        if (mainCamera == null) 
        {
            Debug.Log("[HoseEquipment] mainCamera == null");
            return;
        }
        
        // Получаем позицию курсора в мировых координатах
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = 0f;
        
        Debug.Log($"[HoseEquipment] Mouse world position: {mouseWorldPos}");
        
        // Ищем все объекты плесени в сцене
        MoldSurface[] allMoldSurfaces = FindObjectsOfType<MoldSurface>();
        
        foreach (MoldSurface moldSurface in allMoldSurfaces)
        {
            // Проверяем, находится ли курсор в пределах плесени
            if (IsPointInsideMold(moldSurface, mouseWorldPos))
            {
                Debug.Log($"[HoseEquipment] Mouse inside mold {moldSurface.name}, erasing at {mouseWorldPos}");
                moldSurface.EraseAtWorldPoint(mouseWorldPos);
                
                // Проверяем результат
                float cleanPercent = moldSurface.GetCleanPercent();
                Debug.Log($"[HoseEquipment] Clean percent after erase: {cleanPercent:P1}");
                break; // Обрабатываем только одну плесень за раз
            }
        }
    }
    
    /// <summary>
    /// Проверяет, находится ли точка внутри плесени
    /// </summary>
    private bool IsPointInsideMold(MoldSurface moldSurface, Vector3 worldPoint)
    {
        SpriteRenderer spriteRenderer = moldSurface.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return false;
        
        // Получаем границы спрайта
        Bounds spriteBounds = spriteRenderer.bounds;
        
        // Проверяем, находится ли точка внутри границ
        bool isInside = spriteBounds.Contains(worldPoint);
        
        Debug.Log($"[HoseEquipment] Point {worldPoint} inside {moldSurface.name} bounds {spriteBounds}: {isInside}");
        
        return isInside;
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
