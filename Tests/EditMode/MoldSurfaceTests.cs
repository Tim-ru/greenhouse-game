using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Тесты для MoldSurface в режиме редактирования.
/// Проверяет функциональность стирания плесени и расчета процента очистки.
/// </summary>
public class MoldSurfaceTests
{
    private GameObject testObject;
    private MoldSurface moldSurface;
    private SpriteRenderer spriteRenderer;
    
    [SetUp]
    public void Setup()
    {
        // Создаем тестовый объект
        testObject = new GameObject("TestMoldSurface");
        spriteRenderer = testObject.AddComponent<SpriteRenderer>();
        moldSurface = testObject.AddComponent<MoldSurface>();
        
        // Создаем тестовую текстуру и спрайт
        CreateTestSprite();
    }
    
    [TearDown]
    public void TearDown()
    {
        if (testObject != null)
        {
            Object.DestroyImmediate(testObject);
        }
    }
    
    private void CreateTestSprite()
    {
        // Создаем текстуру 64x64 с полностью непрозрачной альфой
        Texture2D testTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(1f, 0f, 0f, 1f); // Красный с полной альфой
        }
        
        testTexture.SetPixels(pixels);
        testTexture.Apply();
        
        // Создаем спрайт
        Sprite testSprite = Sprite.Create(
            testTexture,
            new Rect(0, 0, 64, 64),
            new Vector2(0.5f, 0.5f),
            64f // pixelsPerUnit
        );
        
        // Устанавливаем спрайт через рефлексию (так как поле приватное)
        var sourceMoldSpriteField = typeof(MoldSurface).GetField("sourceMoldSprite", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        sourceMoldSpriteField?.SetValue(moldSurface, testSprite);
        
        spriteRenderer.sprite = testSprite;
    }
    
    [Test]
    public void MoldSurface_Initialization_ShouldCreateRuntimeTexture()
    {
        // Вызываем Awake для инициализации
        moldSurface.SendMessage("Awake");
        
        // Проверяем, что объект активен
        Assert.IsTrue(moldSurface.enabled);
        
        // Проверяем, что спрайт установлен
        Assert.IsNotNull(spriteRenderer.sprite);
    }
    
    [Test]
    public void MoldSurface_EraseAtWorldPoint_ShouldReduceAlpha()
    {
        // Инициализируем
        moldSurface.SendMessage("Awake");
        
        // Получаем начальный процент очистки
        float initialCleanPercent = moldSurface.GetCleanPercent();
        
        // Стираем в центре объекта
        Vector3 centerPoint = testObject.transform.position;
        moldSurface.EraseAtWorldPoint(centerPoint);
        
        // Проверяем, что процент очистки увеличился
        float finalCleanPercent = moldSurface.GetCleanPercent();
        Assert.Greater(finalCleanPercent, initialCleanPercent);
    }
    
    [Test]
    public void MoldSurface_GetCleanPercent_ShouldReturnValidPercentage()
    {
        // Инициализируем
        moldSurface.SendMessage("Awake");
        
        float cleanPercent = moldSurface.GetCleanPercent();
        
        // Проверяем, что процент в допустимом диапазоне
        Assert.GreaterOrEqual(cleanPercent, 0f);
        Assert.LessOrEqual(cleanPercent, 1f);
    }
    
    [Test]
    public void MoldSurface_MultipleErase_ShouldIncreaseCleanPercent()
    {
        // Инициализируем
        moldSurface.SendMessage("Awake");
        
        float initialCleanPercent = moldSurface.GetCleanPercent();
        
        // Выполняем несколько стираний в разных точках
        Vector3 centerPoint = testObject.transform.position;
        moldSurface.EraseAtWorldPoint(centerPoint);
        moldSurface.EraseAtWorldPoint(centerPoint + Vector3.right * 0.1f);
        moldSurface.EraseAtWorldPoint(centerPoint + Vector3.left * 0.1f);
        
        float finalCleanPercent = moldSurface.GetCleanPercent();
        
        // Проверяем, что процент очистки значительно увеличился
        Assert.Greater(finalCleanPercent, initialCleanPercent + 0.1f);
    }
    
    [Test]
    public void MoldSurface_EraseOutsideBounds_ShouldNotCrash()
    {
        // Инициализируем
        moldSurface.SendMessage("Awake");
        
        // Пытаемся стереть за пределами объекта
        Vector3 farPoint = testObject.transform.position + Vector3.right * 100f;
        
        // Это не должно вызывать исключений
        Assert.DoesNotThrow(() => moldSurface.EraseAtWorldPoint(farPoint));
    }
    
    [Test]
    public void MoldSurface_WithNullSprite_ShouldDisableComponent()
    {
        // Создаем новый объект без спрайта
        GameObject nullSpriteObject = new GameObject("NullSpriteObject");
        MoldSurface nullMoldSurface = nullSpriteObject.AddComponent<MoldSurface>();
        SpriteRenderer nullSpriteRenderer = nullSpriteObject.AddComponent<SpriteRenderer>();
        
        // Не устанавливаем спрайт
        nullSpriteRenderer.sprite = null;
        
        // Вызываем Awake
        nullMoldSurface.SendMessage("Awake");
        
        // Проверяем, что компонент отключен
        Assert.IsFalse(nullMoldSurface.enabled);
        
        // Очищаем
        Object.DestroyImmediate(nullSpriteObject);
    }
}
