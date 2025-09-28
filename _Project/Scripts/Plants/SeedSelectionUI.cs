using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SeedSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject seedSelectionPanel;
    public Transform seedButtonParent;
    public Button seedButtonPrefab;
    public Button closeButton;
    
    [Header("Inventory")]
    // Теперь используем InventorySystem вместо SeedInventory
    // public SeedInventory inventory;
    
    private System.Action<PlantData> onSeedSelected;
    private PotEntity targetPot;
    
    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSelection);
            
        if (seedSelectionPanel != null)
            seedSelectionPanel.SetActive(false);
    }
    
    public void ShowSeedSelection(PotEntity pot, System.Action<PlantData> onSelected = null)
    {
        targetPot = pot;
        onSeedSelected = onSelected;
        
        if (seedSelectionPanel != null)
            seedSelectionPanel.SetActive(true);
            
        PopulateSeedButtons();
    }
    
    private void PopulateSeedButtons()
    {
        // Очищаем существующие кнопки
        if (seedButtonParent != null)
        {
            foreach (Transform child in seedButtonParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (InventorySystem.Instance == null) return;
        
        var availableSeeds = InventorySystem.Instance.GetAvailableSeeds();
        
        foreach (var seedData in availableSeeds)
        {
            if (seedButtonPrefab != null && seedButtonParent != null)
            {
                Button seedButton = Instantiate(seedButtonPrefab, seedButtonParent);
                
                // Настраиваем кнопку
                var buttonText = seedButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = seedData.name;
                
                var buttonImage = seedButton.GetComponent<Image>();
                if (buttonImage != null && seedData.stageSprites != null && seedData.stageSprites.Length > 0)
                    buttonImage.sprite = seedData.stageSprites[0];
                
                // Добавляем обработчик клика
                seedButton.onClick.AddListener(() => SelectSeed(seedData));
            }
        }
    }
    
    private void SelectSeed(PlantData seedData)
    {
        if (InventorySystem.Instance != null && InventorySystem.Instance.UseSeed(seedData))
        {
            if (targetPot != null)
                targetPot.PlantSeed(seedData);
                
            onSeedSelected?.Invoke(seedData);
        }
        
        CloseSelection();
    }
    
    public void CloseSelection()
    {
        if (seedSelectionPanel != null)
            seedSelectionPanel.SetActive(false);
            
        onSeedSelected = null;
        targetPot = null;
    }
}
