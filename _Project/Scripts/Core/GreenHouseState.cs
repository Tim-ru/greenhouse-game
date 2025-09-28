using System;
using UnityEngine;

public class GreenhouseState : MonoBehaviour
{
    public GreenhouseParams parameters;

    [Range(0, 1)] public float Dirt { get; private set; } = 0f; // 0..1
    public float Temperature { get; private set; } = 24f;      // °C
    [Range(0, 1)] public float Humidity { get; private set; } = 0.8f;
    [Range(0, 1)] public float Oxygen { get; private set; } = 0.8f; // 0..1
    [Range(0, 1)] public float Light => Mathf.Clamp01(parameters.baseLight * (1f - Dirt));

    public event Action OnChanged;

    public void AddDirt(float v) { Dirt = Mathf.Clamp01(Dirt + v); OnChanged?.Invoke(); }
    public void AddTemperature(float v) { Temperature += v; OnChanged?.Invoke(); }
    public void AddHumidity(float v) { Humidity = Mathf.Clamp01(Humidity + v); OnChanged?.Invoke(); }
    public void AddOxygen(float v) { Oxygen = Mathf.Clamp01(Oxygen + v); OnChanged?.Invoke(); }

    void Update()
    {
        float dt = Time.deltaTime;

        Temperature += parameters.temperatureDrift * dt;
        // Humidity = Mathf.Clamp01(Humidity + parameters.humidityDrift * dt);
        Dirt = Mathf.Clamp01(Dirt + parameters.dirtAccumulation * dt);
        
        // Кислород: растения производят кислород, но он также потребляется
        float oxygenChange = (parameters.oxygenProductionRate - parameters.oxygenConsumptionRate) * dt;
        Oxygen = Mathf.Clamp01(Oxygen + oxygenChange);

        OnChanged?.Invoke();
    }
}
