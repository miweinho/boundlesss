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
        bool hasGame = GameManager.Instance != null && GameManager.Instance.HasActiveGame;

        string label = hasGame ? "Resume" : "Play";

        if (playButtonLabelTMP != null) playButtonLabelTMP.text = label;
        if (playButtonLabelUGUI != null) playButtonLabelUGUI.text = label;
    }

    public void PlayGame()
    {
        // close menu if game is active
        if (GameManager.Instance != null && GameManager.Instance.HasActiveGame)
        {
            GameManager.Instance.HideMainMenuOverlay();
            GameManager.Instance.SetState(GameManager.GameState.Playing);
            return;
        }

        // if there is no active game load intro scene newly
        SceneManager.LoadScene("IntroScene", LoadSceneMode.Single);
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

