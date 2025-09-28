using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Гизмос для отладки MoldSurface.
/// Отображает круг кисти в позиции курсора для визуализации области стирания.
/// </summary>
[CustomEditor(typeof(MoldSurface))]
public class MoldSurfaceGizmo : Editor
{
    private MoldSurface moldSurface;
    
    private void OnEnable()
    {
        moldSurface = target as MoldSurface;
    }
    
    private void OnSceneGUI()
    {
        if (moldSurface == null) return;
        
        // Получаем позицию курсора в мировых координатах
        Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        mousePosition.z = 0f; // Для 2D
        
        // Рисуем круг кисти
        DrawBrushGizmo(mousePosition);
        
        // Обновляем сцену для плавного отображения
        if (Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
    }
    
    private void DrawBrushGizmo(Vector3 worldPosition)
    {
        if (moldSurface == null) return;
        
        // Получаем радиус кисти в мировых единицах
        float brushRadius = GetBrushRadiusInWorldUnits();
        
        // Настройки отображения
        Handles.color = new Color(0f, 1f, 0f, 0.3f); // Полупрозрачный зеленый
        Handles.DrawSolidDisc(worldPosition, Vector3.forward, brushRadius);
        
        // Контур круга
        Handles.color = Color.green;
        Handles.DrawWireDisc(worldPosition, Vector3.forward, brushRadius);
        
        // Центральная точка
        Handles.color = Color.red;
        Handles.DrawWireDisc(worldPosition, Vector3.forward, 0.1f);
    }
    
    private float GetBrushRadiusInWorldUnits()
    {
        if (moldSurface == null) return 1f;
        
        // Получаем SpriteRenderer для определения pixelsPerUnit
        SpriteRenderer spriteRenderer = moldSurface.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return 1f;
        
        // Конвертируем пиксели в мировые единицы
        float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        
        // Получаем радиус кисти через рефлексию (так как поле приватное)
        var brushRadiusField = typeof(MoldSurface).GetField("brushRadiusPixels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (brushRadiusField != null)
        {
            int brushRadiusPixels = (int)brushRadiusField.GetValue(moldSurface);
            return brushRadiusPixels / pixelsPerUnit;
        }
        
        return 1f;
    }
    
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    private static void DrawMoldSurfaceGizmo(MoldSurface moldSurface, GizmoType gizmoType)
    {
        if (moldSurface == null) return;
        
        // Рисуем границы объекта
        Gizmos.color = Color.blue;
        Bounds bounds = moldSurface.GetComponent<SpriteRenderer>().bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // Показываем процент очистки
        float cleanPercent = moldSurface.GetCleanPercent();
        Vector3 labelPosition = bounds.center + Vector3.up * bounds.size.y * 0.6f;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPosition, $"Clean: {cleanPercent:P1}");
        #endif
    }
}
#endif
