using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public GreenhouseState greenhouseState;
    public InventorySystem inventorySystem;

    void Awake()
    {
        if (!greenhouseState) greenhouseState = FindObjectOfType<GreenhouseState>();
        if (!inventorySystem) inventorySystem = FindObjectOfType<InventorySystem>();
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
}