using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("IntroScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT GAME!");
    }

    public void OpenOptionsMenu()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

}

