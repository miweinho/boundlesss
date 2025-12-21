using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Play Button Label (optional)")]
    [SerializeField] private TMP_Text playButtonLabelTMP;
    [SerializeField] private Text playButtonLabelUGUI;

    void OnEnable()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        bool canResume = GameManager.Instance != null && GameManager.Instance.CanResume;
        string label = canResume ? "Resume" : "Play";

        if (playButtonLabelTMP != null)
            playButtonLabelTMP.text = label;

        if (playButtonLabelUGUI != null)
            playButtonLabelUGUI.text = label;
    }

    public void PlayGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuUI] No GameManager found. Loading Bootstrap.");
            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
            return;
        }

        if (GameManager.Instance.CanResume)
        {
            GameManager.Instance.HideMainMenuOverlay();
            return;
        }

        GameManager.Instance.StartNewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT GAME!");
    }

    public void OpenOptionsMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ShowOptionsOverlay();
        else
            SceneManager.LoadScene("OptionsMenu", LoadSceneMode.Additive);
    }

    public void CloseMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.HideMainMenuOverlay();
    }
}

