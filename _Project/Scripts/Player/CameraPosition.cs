using UnityEngine;

/// <summary>
/// Данные о позиции камеры для настройки через Inspector
/// </summary>
[System.Serializable]
public class CameraPosition
{
    [Header("Position Settings")]
    public string positionName = "New Position";
    public Vector3 position = Vector3.zero;
    public bool useCustomSize = false;
    public float orthographicSize = 5f;
    
    [Header("Transition Settings")]
    public float transitionTime = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    public CameraPosition(string name, Vector3 pos)
    {
        positionName = name;
        position = pos;
        transitionTime = 1f;
        transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }
}

