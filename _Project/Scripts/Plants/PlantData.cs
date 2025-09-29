using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Greenhouse/Plant Data")]
public class PlantData : ScriptableObject
{
    [Header("Growth Settings")]
    public AnimationCurve growthRateByLight = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Environment Requirements")]
    public Vector2 tempComfort = new Vector2(20, 24);
    public Vector2 humidityComfort = new Vector2(0.45f, 0.65f);
    
    [Header("Health Settings")]
    public float damagePerSecondOutside = 0.033f; // Урон для смерти за 30 секунд (100% / 30 сек = 0.033)
    
    [Header("Visual Settings")]
    public Sprite[] stageSprites; // 0..N
    public float[] stageThresholds = { 0.2f, 0.5f, 0.8f, 1.0f }; // суммарный прогресс 0..1
    public float plantScale = 1.5f; // Размер растения (можно настроить для каждого типа)
    
    [Header("Watering Settings")]
    public float waterConsumptionRate = 0.02f; // Снижаем потребление воды с 0.1 до 0.02
    public float maxWaterNeed = 1f; // максимальная потребность в воде
    public float wateringEffectiveness = 1f; // эффективность полива
}
