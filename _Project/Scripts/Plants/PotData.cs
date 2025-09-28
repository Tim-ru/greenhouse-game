using UnityEngine;

[CreateAssetMenu(fileName = "PotData", menuName = "Greenhouse/Pot Data")]
public class PotData : ScriptableObject
{
    [Header("Visual Settings")]
    public Sprite emptyPotSprite;
    public Sprite potWithPlantSprite;
    
    [Header("Watering Settings")]
    public float waterCapacity = 1f; // максимальное количество воды
    public float waterRefillAmount = 0.5f; // количество воды за один полив
    public float waterRetentionTime = 10f; // время удержания воды в горшке
    
    [Header("Plant Settings")]
    public bool canHoldPlant = true;
    public PlantData defaultPlantData; // растение по умолчанию (опционально)
    public bool autoPlantOnStart = false; // автоматически сажать растение при старте
}
