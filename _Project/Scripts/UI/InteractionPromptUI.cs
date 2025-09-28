using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text label;

    public void Show(string text)
    {
        Debug.Log($"[InteractionPromptUI] Показываем подсказку: '{text}'");
        label.text = text;
        group.alpha = 1;
        group.blocksRaycasts = false;
    }

    public void Hide()
    {
        Debug.Log("[InteractionPromptUI] Скрываем подсказку");
        group.alpha = 0;
        group.blocksRaycasts = false;
    }
}
