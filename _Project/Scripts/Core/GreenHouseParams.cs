using UnityEngine;

[CreateAssetMenu(fileName = "GreenhouseParams", menuName = "Greenhouse/Params")]
public class GreenhouseParams : ScriptableObject
{
    [Header("Target ranges")]
    public Vector2 temperatureComfort = new Vector2(20, 24); // °C
    public Vector2 humidityComfort = new Vector2(0.45f, 0.65f); // 0..1

    [Header("Drift per second")]
    public float temperatureDrift = -0.05f; // утекает вниз
    public float humidityDrift = -0.01f;
    public float dirtAccumulation = 0.01f; // растет грязь 0..1

    [Header("Light")]
    public float baseLight = 1.0f; // 0..1 до грязи
    
    [Header("Oxygen")]
    public float oxygenProductionRate = 0.02f; // производство кислорода в секунду
    public float oxygenConsumptionRate = 0.01f; // потребление кислорода в секунду
}
