using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Простая система партиклов для эффекта разлетающихся капель воды.
/// Создает эффект как от сварки или брызг воды без физики.
/// </summary>
public class WaterParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 12;
    [SerializeField] private float particleLifetime = 1.5f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float gravity = 5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color particleColor = new Color(0.7f, 0.9f, 1f, 0.8f);
    [SerializeField] private float minSize = 0.25f;
    [SerializeField] private float maxSize = 0.55f;
    [SerializeField] private AnimationCurve sizeOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f);
    [SerializeField] private AnimationCurve alphaOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    private List<WaterParticle> activeParticles = new List<WaterParticle>();
    
    [System.Serializable]
    private class WaterParticle
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public Vector3 velocity;
        public float lifetime;
        public float maxLifetime;
        public float size;
        public Vector3 startPosition;
        
        public WaterParticle(GameObject obj, Vector3 vel, float maxLife, float particleSize, Vector3 startPos)
        {
            gameObject = obj;
            spriteRenderer = obj.GetComponent<SpriteRenderer>();
            velocity = vel;
            maxLifetime = maxLife;
            lifetime = maxLife;
            size = particleSize;
            startPosition = startPos;
        }
    }
    
    private void Start()
    {
        // Создаем префаб партикла
        CreateParticlePrefab();
    }
    
    private GameObject particlePrefab;
    
    private void CreateParticlePrefab()
    {
        // Создаем префаб партикла
        particlePrefab = new GameObject("WaterParticlePrefab");
        
        // Создаем текстуру для партикла
        Texture2D particleTexture = CreateParticleTexture();
        
        // Создаем спрайт
        Sprite particleSprite = Sprite.Create(
            particleTexture,
            new Rect(0, 0, particleTexture.width, particleTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        // Добавляем SpriteRenderer
        SpriteRenderer renderer = particlePrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = particleSprite;
        renderer.color = particleColor;
        renderer.sortingOrder = 15; // Поверх всего
        
        // Делаем префаб неактивным
        particlePrefab.SetActive(false);
    }
    
    private Texture2D CreateParticleTexture()
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
                alpha = Mathf.Pow(alpha, 2f); // Более резкие края для партиклов
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Создает эффект разлетающихся партиклов в указанной позиции
    /// </summary>
    public void CreateParticlesAt(Vector3 worldPosition, Vector3 direction)
    {
        for (int i = 0; i < particleCount; i++)
        {
            CreateSingleParticle(worldPosition, direction);
        }
    }
    
    private void CreateSingleParticle(Vector3 position, Vector3 direction)
    {
        // Создаем партикл
        GameObject particleObj = Instantiate(particlePrefab, position, Quaternion.identity);
        particleObj.SetActive(true);
        
        // Случайный размер
        float particleSize = Random.Range(minSize, maxSize);
        particleObj.transform.localScale = Vector3.one * particleSize;
        
        // Случайная скорость в направлении от удара
        float speed = Random.Range(minSpeed, maxSpeed);
        Vector3 randomDirection = (direction + Random.insideUnitSphere * 0.8f).normalized;
        Vector3 velocity = randomDirection * speed;
        
        WaterParticle particle = new WaterParticle(particleObj, velocity, particleLifetime, particleSize, position);
        activeParticles.Add(particle);
    }
    
    private void Update()
    {
        // Обновляем все активные партиклы
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            WaterParticle particle = activeParticles[i];
            
            if (particle.gameObject == null)
            {
                activeParticles.RemoveAt(i);
                continue;
            }
            
            // Уменьшаем время жизни
            particle.lifetime -= Time.deltaTime;
            
            if (particle.lifetime <= 0f)
            {
                // Удаляем партикл
                DestroyImmediate(particle.gameObject);
                activeParticles.RemoveAt(i);
            }
            else
            {
                // Простая физика без Rigidbody
                particle.velocity.y -= gravity * Time.deltaTime;
                particle.gameObject.transform.position += particle.velocity * Time.deltaTime;
                
                // Обновляем визуальные свойства
                float normalizedTime = 1f - (particle.lifetime / particle.maxLifetime);
                
                if (particle.spriteRenderer != null)
                {
                    Color currentColor = particleColor;
                    currentColor.a *= alphaOverTime.Evaluate(normalizedTime);
                    particle.spriteRenderer.color = currentColor;
                    
                    float currentSize = particle.size * sizeOverTime.Evaluate(normalizedTime);
                    particle.gameObject.transform.localScale = Vector3.one * currentSize;
                }
                
                // Проверяем, не упал ли партикл слишком низко
                if (particle.gameObject.transform.position.y < -10f)
                {
                    DestroyImmediate(particle.gameObject);
                    activeParticles.RemoveAt(i);
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Очищаем все партиклы
        foreach (WaterParticle particle in activeParticles)
        {
            if (particle.gameObject != null)
            {
                DestroyImmediate(particle.gameObject);
            }
        }
        activeParticles.Clear();
    }
}
