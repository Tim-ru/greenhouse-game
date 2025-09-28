using System.Collections.Generic;
using UnityEngine;

public class GreenhousePotManager : MonoBehaviour
{
    [Header("Pot Management")]
    public List<PotEntity> allPots = new List<PotEntity>();
    
    [Header("Settings")]
    public bool autoFindPots = true;
    public float updateInterval = 1f; // Обновление состояния каждую секунду
    
    private float lastUpdateTime;
    
    void Start()
    {
        if (autoFindPots)
        {
            FindAllPots();
        }
    }
    
    void Update()
    {
        // Периодическое обновление состояния горшков
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdatePotStates();
            lastUpdateTime = Time.time;
        }
    }
    
    private void FindAllPots()
    {
        allPots.Clear();
        PotEntity[] pots = FindObjectsOfType<PotEntity>();
        allPots.AddRange(pots);
        
        Debug.Log($"Found {allPots.Count} pots in greenhouse");
    }
    
    private void UpdatePotStates()
    {
        foreach (var pot in allPots)
        {
            if (pot == null) continue;
            
            // Можно добавить логику для автоматического полива
            // или других системных действий
        }
    }
    
    public void RegisterPot(PotEntity pot)
    {
        if (!allPots.Contains(pot))
        {
            allPots.Add(pot);
        }
    }
    
    public void UnregisterPot(PotEntity pot)
    {
        allPots.Remove(pot);
    }
    
    public List<PotEntity> GetPotsNeedingWater()
    {
        List<PotEntity> needyPots = new List<PotEntity>();
        foreach (var pot in allPots)
        {
            if (pot != null && pot.NeedsWater)
            {
                needyPots.Add(pot);
            }
        }
        return needyPots;
    }
    
    public List<PotEntity> GetBloomedPlants()
    {
        List<PotEntity> bloomedPots = new List<PotEntity>();
        foreach (var pot in allPots)
        {
            if (pot != null && pot.HasPlant && pot.Plant != null && pot.Plant.IsBloomed)
            {
                bloomedPots.Add(pot);
            }
        }
        return bloomedPots;
    }
    
    public int GetTotalPots() => allPots.Count;
    public int GetPotsWithPlants() => allPots.FindAll(p => p != null && p.HasPlant).Count;
    public int GetEmptyPots() => GetTotalPots() - GetPotsWithPlants();
    
    // Метод для получения статистики теплицы
    public GreenhouseStats GetStats()
    {
        return new GreenhouseStats
        {
            totalPots = GetTotalPots(),
            potsWithPlants = GetPotsWithPlants(),
            emptyPots = GetEmptyPots(),
            potsNeedingWater = GetPotsNeedingWater().Count,
            bloomedPlants = GetBloomedPlants().Count
        };
    }
}

[System.Serializable]
public class GreenhouseStats
{
    public int totalPots;
    public int potsWithPlants;
    public int emptyPots;
    public int potsNeedingWater;
    public int bloomedPlants;
}
