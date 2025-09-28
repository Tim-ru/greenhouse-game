using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GlassCleaningMiniGame : MonoBehaviour, IMiniGame
{
    public Slider progressBar;
    public TextMeshProUGUI instructionText; // Добавляем текст с инструкциями
    public float progressPerSecondWhileMoving = 0.35f;
    public float successThreshold = 0.85f;

    Action<bool> onComplete;
    Vector2 lastMouse;
    bool running;
    GreenhouseState state;
    bool isMousePressed = false;

    public void StartGame(GreenhouseState s, Action<bool> done)
    {
        state = s;
        onComplete = done;
        
        // НАЧИНАЕМ С 0 - игрок должен сам начистить стекло
        progressBar.value = 0f;
        
        // Показываем инструкции
        if (instructionText != null)
        {
            instructionText.text = "Держите ЛКМ и водите курсором по экрану!\nНажмите ESC для выхода";
            instructionText.gameObject.SetActive(true);
        }
        
        // Безопасное получение позиции мыши
        if (Mouse.current != null)
            lastMouse = Mouse.current.position.ReadValue();
        else
            lastMouse = Vector2.zero;
            
        running = true;
    }

    void Update()
    {
        if (!running) return;

        // Проверяем наличие устройств ввода
        if (Mouse.current == null || Keyboard.current == null) return;

        Vector2 now = Mouse.current.position.ReadValue();
        float moved = (now - lastMouse).magnitude;
        lastMouse = now;

        // Проверяем нажатие мыши через новую Input System
        isMousePressed = Mouse.current.leftButton.isPressed;

        if (isMousePressed && moved > 0.5f)
        {
            progressBar.value = Mathf.Clamp01(progressBar.value + progressPerSecondWhileMoving * Time.unscaledDeltaTime);
        }

        if (progressBar.value >= successThreshold)
        {
            // Уменьшаем грязь пропорционально достигнутому прогрессу
            // Чем больше прогресс, тем больше грязи убираем
            float dirtReduction = progressBar.value * 0.5f; // Максимум убираем 50% грязи
            state.AddDirt(-dirtReduction); // Отрицательное значение уменьшает грязь
            Finish(true);
        }

        // Проверяем Escape через новую Input System
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Finish(false);
    }

    void Finish(bool success)
    {
        running = false;
        
        // Дополнительная защита - убеждаемся что время восстановлено
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        
        // Не управляем активностью здесь - это делает MiniGameController
        onComplete?.Invoke(success);
    }
}
