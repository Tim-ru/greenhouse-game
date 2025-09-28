using System;
using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    public Canvas miniGameCanvas;
    public GreenhouseState state;
    public InteractionPromptUI promptUI;

    bool running;
    Action<bool> onComplete;

    void Start()
    {
        // Автоматически найти компоненты если не назначены
        if (!state)
            state = FindObjectOfType<GreenhouseState>();
        
        if (!miniGameCanvas)
            miniGameCanvas = FindObjectOfType<Canvas>();
            
        if (!promptUI)
            promptUI = FindObjectOfType<InteractionPromptUI>();
    }

    public void Run(IMiniGame game, Action<bool> callback)
    {
        if (running) return;
        
        // Проверки на null
        if (!miniGameCanvas)
        {
            callback?.Invoke(false);
            return;
        }
        
        if (!state)
        {
            callback?.Invoke(false);
            return;
        }
        
        if (game == null)
        {
            callback?.Invoke(false);
            return;
        }
        
        running = true;
        onComplete = (s) => { 
            running = false; 
            
            // Восстанавливаем время СРАЗУ
            Time.timeScale = 1f;
            
            miniGameCanvas.enabled = false; 
            // Также отключаем конкретную игру
            if (game is MonoBehaviour gameMB)
            {
                gameMB.gameObject.SetActive(false);
            }
            
            callback?.Invoke(s); 
        };
        
        // Скрываем подсказку взаимодействия
        if (promptUI != null)
        {
            promptUI.Hide();
        }
        
        // Включаем Canvas и конкретную игру
        miniGameCanvas.enabled = true;
        
        if (game is MonoBehaviour gameMB)
        {
            gameMB.gameObject.SetActive(true);
        }
            
        game.StartGame(state, onComplete);
        Time.timeScale = 0f;
    }

    public void Close(bool success)
    {
        // Восстанавливаем время СРАЗУ
        Time.timeScale = 1f;
        
        // Завершаем игру без повторного вызова onComplete
        if (running)
        {
            running = false;
            miniGameCanvas.enabled = false;
            
            // Отключаем конкретную игру
            if (onComplete != null)
            {
                // Находим игру через onComplete (это замыкание)
                var callback = onComplete;
                onComplete = null; // Очищаем ссылку
                callback.Invoke(success);
            }
        }
    }
}

public interface IMiniGame
{
    void StartGame(GreenhouseState state, Action<bool> onComplete);
}
