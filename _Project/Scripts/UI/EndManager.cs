using UnityEngine;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    public void ExitMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
