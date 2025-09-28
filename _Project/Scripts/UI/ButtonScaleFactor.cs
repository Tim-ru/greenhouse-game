using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleFactor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scale;

    private RectTransform rect;
    private Vector3 scaleDefault;

    public void OnPointerEnter(PointerEventData eventData)
    {
        rect.localScale = scale * scaleDefault;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.localScale = scaleDefault;
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        scaleDefault = rect.localScale;
    }


}
