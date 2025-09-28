using UnityEngine;

public class ExampleInteractable : InteractableBase
{
    [SerializeField] string interactionText = "Нажмите E для взаимодействия";
    
    void Start()
    {
        // Можно переопределить текст подсказки в инспекторе
        prompt = interactionText;
    }

    protected override void OnInteract(GameObject interactor)
    {
        Debug.Log($"Игрок {interactor.name} взаимодействует с {gameObject.name}!");
        
        // Здесь добавьте логику взаимодействия
        // Например: открыть диалог, поднять предмет, активировать механизм и т.д.
        
        // Простой пример - изменить цвет объекта
        GetComponent<SpriteRenderer>().color = Random.ColorHSV();
    }

    public override bool CanInteract(GameObject interactor)
    {
        // Можно добавить условия для взаимодействия
        // Например: только если у игрока есть ключ, или если объект не заблокирован
        
        bool canInteract = true; // Пока всегда можно взаимодействовать
        Debug.Log($"[ExampleInteractable] Проверка взаимодействия с {gameObject.name}: {(canInteract ? "разрешено" : "запрещено")}");
        return canInteract;
    }
}
