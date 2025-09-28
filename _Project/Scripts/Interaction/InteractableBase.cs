using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string prompt = "Взаимодействовать [E]";
    public string Prompt 
    { 
        get 
        {
            Debug.Log($"[InteractableBase] Получение подсказки для {GetType().Name}: '{prompt}'");
            return prompt;
        }
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        bool canInteract = true;
        Debug.Log($"[InteractableBase] Проверка возможности взаимодействия с {GetType().Name}: {(canInteract ? "да" : "нет")}");
        return canInteract;
    }
    public void Interact(GameObject interactor)
    {
        Debug.Log($"[InteractableBase] Начало взаимодействия с {GetType().Name} от {interactor.name}");
        OnInteract(interactor);
        Debug.Log($"[InteractableBase] Завершение взаимодействия с {GetType().Name}");
    }
    
    protected abstract void OnInteract(GameObject interactor);
}
