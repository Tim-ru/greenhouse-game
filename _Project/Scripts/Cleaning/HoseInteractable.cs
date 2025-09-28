using UnityEngine;

/// <summary>
/// Компонент для взаимодействия со шлангом.
/// Позволяет игроку взять шланг для очистки плесени в труднодоступных местах.
/// </summary>
public class HoseInteractable : MonoBehaviour, IInteractable
{
    [Header("Hose Settings")]
    [SerializeField] private string promptText = "Взять шланг";
    [SerializeField] private string promptTextInUse = "Шланг уже используется";
    [SerializeField] private bool requiresWateringCan = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite hoseSprite;
    [SerializeField] private GameObject hoseVisual;
    
    private bool isHoseInUse = false;
    private GameObject currentUser;
    
    public string Prompt => isHoseInUse ? promptTextInUse : promptText;
    
    /// <summary>
    /// Проверяет, можно ли взять шланг
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        if (isHoseInUse && currentUser != interactor) return false;
        
        // Если требуется лейка, проверяем её наличие
        if (requiresWateringCan)
        {
            if (InventorySystem.Instance == null) return false;
            if (!InventorySystem.Instance.HasWateringCan) return false;
            
            var wateringCan = InventorySystem.Instance.GetWateringCan();
            if (wateringCan == null || !wateringCan.HasWater) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Взаимодействие со шлангом - взятие или возврат
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        
        // Получаем компонент HoseEquipment у игрока
        HoseEquipment hoseEquipment = interactor.GetComponent<HoseEquipment>();
        
        if (hoseEquipment != null && hoseEquipment.IsHoseEquipped)
        {
            // Игрок уже держит шланг - возвращаем его
            ReturnHose(interactor, hoseEquipment);
        }
        else
        {
            // Игрок берет шланг
            TakeHose(interactor);
        }
    }
    
    private void TakeHose(GameObject interactor)
    {
        // Получаем или создаем компонент HoseEquipment
        HoseEquipment hoseEquipment = interactor.GetComponent<HoseEquipment>();
        if (hoseEquipment == null)
        {
            hoseEquipment = interactor.AddComponent<HoseEquipment>();
        }
        
        // Передаем шланг игроку
        hoseEquipment.EquipHose(this);
        
        // Помечаем шланг как используемый
        isHoseInUse = true;
        currentUser = interactor;
        
        // Скрываем визуальный объект шланга
        if (hoseVisual != null)
        {
            hoseVisual.SetActive(false);
        }
        
        Debug.Log($"[HoseInteractable] Игрок {interactor.name} взял шланг");
    }
    
    private void ReturnHose(GameObject interactor, HoseEquipment hoseEquipment)
    {
        // Возвращаем шланг
        hoseEquipment.UnequipHose();
        
        // Освобождаем шланг
        isHoseInUse = false;
        currentUser = null;
        
        // Показываем визуальный объект шланга
        if (hoseVisual != null)
        {
            hoseVisual.SetActive(true);
        }
        
        Debug.Log($"[HoseInteractable] Игрок {interactor.name} вернул шланг");
    }
    
    /// <summary>
    /// Принудительно освобождает шланг (например, при уничтожении игрока)
    /// </summary>
    public void ForceReturnHose()
    {
        if (currentUser != null)
        {
            HoseEquipment hoseEquipment = currentUser.GetComponent<HoseEquipment>();
            if (hoseEquipment != null)
            {
                hoseEquipment.UnequipHose();
            }
        }
        
        isHoseInUse = false;
        currentUser = null;
        
        if (hoseVisual != null)
        {
            hoseVisual.SetActive(true);
        }
        
        Debug.Log("[HoseInteractable] Шланг принудительно возвращен");
    }
    
    private void OnDestroy()
    {
        ForceReturnHose();
    }
}
