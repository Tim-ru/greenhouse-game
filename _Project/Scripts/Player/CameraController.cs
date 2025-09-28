using UnityEngine;
using System.Collections;

/// <summary>
/// Контроллер камеры с плавным перемещением между позициями
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float moveSpeed = 5f; // Скорость перемещения камеры
    [SerializeField] private float smoothTime = 0.3f; // Время плавного перемещения
    
    [Header("Camera Positions")]
    [SerializeField] private CameraPosition leftDoorPosition = new CameraPosition("Left Door", new Vector3(-10f, 0f, -10f));
    [SerializeField] private CameraPosition rightDoorPosition = new CameraPosition("Right Door", new Vector3(10f, 0f, -10f));
    [SerializeField] private CameraPosition centerPosition = new CameraPosition("Center", new Vector3(0f, 0f, -10f));
    
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private Camera cameraComponent;
    
    private void Start()
    {
        // Получаем компонент камеры
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            cameraComponent = Camera.main;
        }
        
        // Устанавливаем начальную позицию камеры
        targetPosition = centerPosition.position;
        transform.position = centerPosition.position;
        
        // Устанавливаем начальный размер камеры
        if (cameraComponent != null && centerPosition.useCustomSize)
        {
            cameraComponent.orthographicSize = centerPosition.orthographicSize;
        }
    }
    
    private void Update()
    {
        if (isMoving)
        {
            // Плавно перемещаем камеру к целевой позиции
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            
            // Проверяем, достигли ли мы целевой позиции
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
    
    /// <summary>
    /// Перемещает камеру к левой двери
    /// </summary>
    public void MoveToLeftDoor()
    {
        MoveToPosition(leftDoorPosition);
        Debug.Log("[CameraController] Перемещение к левой двери");
    }
    
    /// <summary>
    /// Перемещает камеру к правой двери
    /// </summary>
    public void MoveToRightDoor()
    {
        MoveToPosition(rightDoorPosition);
        Debug.Log("[CameraController] Перемещение к правой двери");
    }
    
    /// <summary>
    /// Возвращает камеру в центр
    /// </summary>
    public void MoveToCenter()
    {
        MoveToPosition(centerPosition);
        Debug.Log("[CameraController] Возврат в центр");
    }
    
    /// <summary>
    /// Перемещает камеру к указанной позиции
    /// </summary>
    private void MoveToPosition(CameraPosition position)
    {
        if (!isMoving)
        {
            targetPosition = position.position;
            isMoving = true;
            
            // Настраиваем размер камеры если нужно
            if (cameraComponent != null && position.useCustomSize)
            {
                StartCoroutine(ChangeCameraSize(position.orthographicSize, position.transitionTime));
            }
        }
    }
    
    /// <summary>
    /// Плавно изменяет размер камеры
    /// </summary>
    private IEnumerator ChangeCameraSize(float targetSize, float duration)
    {
        float startSize = cameraComponent.orthographicSize;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float curveValue = leftDoorPosition.transitionCurve.Evaluate(progress);
            
            cameraComponent.orthographicSize = Mathf.Lerp(startSize, targetSize, curveValue);
            yield return null;
        }
        
        cameraComponent.orthographicSize = targetSize;
    }
    
    /// <summary>
    /// Мгновенно перемещает камеру в указанную позицию
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        targetPosition = position;
        transform.position = position;
        isMoving = false;
    }
    
    /// <summary>
    /// Проверяет, движется ли камера
    /// </summary>
    public bool IsMoving => isMoving;
    
    /// <summary>
    /// Получает текущую целевую позицию
    /// </summary>
    public Vector3 TargetPosition => targetPosition;
}
