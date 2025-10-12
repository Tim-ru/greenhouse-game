using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void Play()
    {
        Debug.Log("[MenuScript] Starting game...");
        
        // Пробуем загрузить по имени, если не получается - по индексу
        try
        {
            SceneManager.LoadScene("Bootstrap");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MenuScript] Could not load 'Bootstrap' scene: {e.Message}");
            Debug.Log("[MenuScript] Trying to load scene by index 1...");
            SceneManager.LoadScene(1);
        }
    }
    
    public void Quit()
    {
        Debug.Log("[MenuScript] Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
