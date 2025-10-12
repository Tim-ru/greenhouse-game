using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Система для создания голубого полупрозрачного шлейфа от струи воды.
/// Оставляет следы в местах, где была "разбрызгана вода".
/// </summary>
public class WaterTrailSystem : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private float trailLifetime = 2f;
    [SerializeField] private float trailFadeSpeed = 1f;
    [SerializeField] private float minTrailDistance = 0.1f;
    [SerializeField] private int maxTrails = 50;
    
    [Header("Visual Settings")]
    [SerializeField] private Color trailColor = new Color(0.5f, 0.8f, 1f, 0.3f); // Голубой полупрозрачный
    [SerializeField] private float trailSize = 0.2f;
    [SerializeField] private AnimationCurve sizeOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve alphaOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    private List<WaterTrail> activeTrails = new List<WaterTrail>();
    private Vector3 lastTrailPosition;
    private bool isFirstTrail = true;
    
    [System.Serializable]
    private class WaterTrail
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public float lifetime;
        public float maxLifetime;
        public Vector3 position;
        
        public WaterTrail(GameObject obj, float maxLife, Vector3 pos)
        {
            gameObject = obj;
            spriteRenderer = obj.GetComponent<SpriteRenderer>();
            maxLifetime = maxLife;
            lifetime = maxLife;
            position = pos;
        }
    }
    
    private void Start()
    {
        // Создаем префаб следа, если он не назначен
        if (trailPrefab == null)
        {
            CreateDefaultTrailPrefab();
        }
    }
    
    private void CreateDefaultTrailPrefab()
    {
        // Создаем простой круглый спрайт для следа
        trailPrefab = new GameObject("WaterTrailPrefab");
        
        // Создаем текстуру для следа
        Texture2D trailTexture = CreateTrailTexture();
        
        // Создаем спрайт
        Sprite trailSprite = Sprite.Create(
            trailTexture,
            new Rect(0, 0, trailTexture.width, trailTexture.height),
            new Vector2(0.5f, 0.5f),
            100f // pixelsPerUnit
        );
        
        // Добавляем SpriteRenderer
        SpriteRenderer renderer = trailPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = trailSprite;
        renderer.color = trailColor;
        renderer.sortingOrder = 5; // Поверх плесени, но под струей
        
        // Делаем префаб неактивным
        trailPrefab.SetActive(false);
    }
    
    private Texture2D CreateTrailTexture()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.4f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                alpha = Mathf.Pow(alpha, 2f); // Делаем более мягкие края
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Добавляет след в указанную позицию
    /// </summary>
    public void AddTrailAt(Vector3 worldPosition)
    {
        // Проверяем расстояние от последнего следа
        if (!isFirstTrail)
        {
            float distance = Vector3.Distance(worldPosition, lastTrailPosition);
            if (distance < minTrailDistance)
            {
                return; // Слишком близко к предыдущему следу
            }
        }
        
        // Ограничиваем количество активных следов
        if (activeTrails.Count >= maxTrails)
        {
            RemoveOldestTrail();
        }
        
        // Создаем новый след
        GameObject trailObj = Instantiate(trailPrefab, worldPosition, Quaternion.identity);
        trailObj.SetActive(true);
        
        WaterTrail trail = new WaterTrail(trailObj, trailLifetime, worldPosition);
        activeTrails.Add(trail);
        
        lastTrailPosition = worldPosition;
        isFirstTrail = false;
    }
    
    /// <summary>
    /// Очищает все следы (вызывается при остановке струи)
    /// </summary>
    public void ClearTrails()
    {
        foreach (WaterTrail trail in activeTrails)
        {
            if (trail.gameObject != null)
            {
                DestroyImmediate(trail.gameObject);
            }
        }
        activeTrails.Clear();
        isFirstTrail = true;
    }
    
    private void RemoveOldestTrail()
    {
        if (activeTrails.Count > 0)
        {
            WaterTrail oldestTrail = activeTrails[0];
            if (oldestTrail.gameObject != null)
            {
                DestroyImmediate(oldestTrail.gameObject);
            }
            activeTrails.RemoveAt(0);
        }
    }
    
    private void Update()
    {
        // Обновляем все активные следы
        for (int i = activeTrails.Count - 1; i >= 0; i--)
        {
            WaterTrail trail = activeTrails[i];
            
            if (trail.gameObject == null)
            {
                activeTrails.RemoveAt(i);
                continue;
            }
            
            // Уменьшаем время жизни
            trail.lifetime -= Time.deltaTime * trailFadeSpeed;
            
            if (trail.lifetime <= 0f)
            {
                // Удаляем след
                DestroyImmediate(trail.gameObject);
                activeTrails.RemoveAt(i);
            }
            else
            {
                // Обновляем визуальные свойства
                float normalizedTime = 1f - (trail.lifetime / trail.maxLifetime);
                
                if (trail.spriteRenderer != null)
                {
                    Color currentColor = trailColor;
                    currentColor.a *= alphaOverTime.Evaluate(normalizedTime);
                    trail.spriteRenderer.color = currentColor;
                    
                    float currentSize = trailSize * sizeOverTime.Evaluate(normalizedTime);
                    trail.gameObject.transform.localScale = Vector3.one * currentSize;
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        ClearTrails();
    }
}


