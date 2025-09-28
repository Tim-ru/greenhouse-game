using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Система для создания эффекта разлетающихся капель воды при стирании плесени.
/// </summary>
public class WaterDropletsEffect : MonoBehaviour
{
    [Header("Droplet Settings")]
    [SerializeField] private GameObject dropletPrefab;
    [SerializeField] private int dropletCount = 8;
    [SerializeField] private float dropletLifetime = 1.5f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float bounceDamping = 0.6f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color dropletColor = new Color(0.7f, 0.9f, 1f, 0.8f);
    [SerializeField] private float minSize = 0.05f;
    [SerializeField] private float maxSize = 0.15f;
    [SerializeField] private AnimationCurve sizeOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f);
    [SerializeField] private AnimationCurve alphaOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    private List<WaterDroplet> activeDroplets = new List<WaterDroplet>();
    
    [System.Serializable]
    private class WaterDroplet
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D rigidbody;
        public float lifetime;
        public float maxLifetime;
        public Vector3 velocity;
        public float size;
        
        public WaterDroplet(GameObject obj, float maxLife, Vector3 vel, float dropletSize)
        {
            gameObject = obj;
            spriteRenderer = obj.GetComponent<SpriteRenderer>();
            rigidbody = obj.GetComponent<Rigidbody2D>();
            maxLifetime = maxLife;
            lifetime = maxLife;
            velocity = vel;
            size = dropletSize;
        }
    }
    
    private void Start()
    {
        // Создаем префаб капли, если он не назначен
        if (dropletPrefab == null)
        {
            CreateDefaultDropletPrefab();
        }
    }
    
    private void CreateDefaultDropletPrefab()
    {
        // Создаем префаб капли
        dropletPrefab = new GameObject("WaterDropletPrefab");
        
        // Создаем текстуру для капли
        Texture2D dropletTexture = CreateDropletTexture();
        
        // Создаем спрайт
        Sprite dropletSprite = Sprite.Create(
            dropletTexture,
            new Rect(0, 0, dropletTexture.width, dropletTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        // Добавляем компоненты
        SpriteRenderer renderer = dropletPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = dropletSprite;
        renderer.color = dropletColor;
        renderer.sortingOrder = 15; // Поверх всего
        
        Rigidbody2D rb = dropletPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        
        CircleCollider2D collider = dropletPrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // Делаем префаб неактивным
        dropletPrefab.SetActive(false);
    }
    
    private Texture2D CreateDropletTexture()
    {
        int size = 32;
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
                alpha = Mathf.Pow(alpha, 1.5f); // Делаем каплю более округлой
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Создает эффект разлетающихся капель в указанной позиции
    /// </summary>
    public void CreateDropletsAt(Vector3 worldPosition, Vector3 impactDirection)
    {
        for (int i = 0; i < dropletCount; i++)
        {
            CreateSingleDroplet(worldPosition, impactDirection);
        }
    }
    
    private void CreateSingleDroplet(Vector3 position, Vector3 impactDirection)
    {
        // Создаем каплю
        GameObject dropletObj = Instantiate(dropletPrefab, position, Quaternion.identity);
        dropletObj.SetActive(true);
        
        // Случайный размер
        float dropletSize = Random.Range(minSize, maxSize);
        dropletObj.transform.localScale = Vector3.one * dropletSize;
        
        // Случайная скорость в направлении от удара
        float speed = Random.Range(minSpeed, maxSpeed);
        Vector3 randomDirection = (impactDirection + Random.insideUnitSphere * 0.5f).normalized;
        Vector3 velocity = randomDirection * speed;
        
        // Добавляем случайное вращение
        float randomTorque = Random.Range(-10f, 10f);
        
        WaterDroplet droplet = new WaterDroplet(dropletObj, dropletLifetime, velocity, dropletSize);
        activeDroplets.Add(droplet);
        
        // Применяем физику
        Rigidbody2D rb = dropletObj.GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
        rb.AddTorque(randomTorque);
    }
    
    private void Update()
    {
        // Обновляем все активные капли
        for (int i = activeDroplets.Count - 1; i >= 0; i--)
        {
            WaterDroplet droplet = activeDroplets[i];
            
            if (droplet.gameObject == null)
            {
                activeDroplets.RemoveAt(i);
                continue;
            }
            
            // Уменьшаем время жизни
            droplet.lifetime -= Time.deltaTime;
            
            if (droplet.lifetime <= 0f)
            {
                // Удаляем каплю
                DestroyImmediate(droplet.gameObject);
                activeDroplets.RemoveAt(i);
            }
            else
            {
                // Обновляем визуальные свойства
                float normalizedTime = 1f - (droplet.lifetime / droplet.maxLifetime);
                
                if (droplet.spriteRenderer != null)
                {
                    Color currentColor = dropletColor;
                    currentColor.a *= alphaOverTime.Evaluate(normalizedTime);
                    droplet.spriteRenderer.color = currentColor;
                    
                    float currentSize = droplet.size * sizeOverTime.Evaluate(normalizedTime);
                    droplet.gameObject.transform.localScale = Vector3.one * currentSize;
                }
                
                // Проверяем, не упала ли капля слишком низко
                if (droplet.gameObject.transform.position.y < -10f)
                {
                    DestroyImmediate(droplet.gameObject);
                    activeDroplets.RemoveAt(i);
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Очищаем все капли
        foreach (WaterDroplet droplet in activeDroplets)
        {
            if (droplet.gameObject != null)
            {
                DestroyImmediate(droplet.gameObject);
            }
        }
        activeDroplets.Clear();
    }
}
