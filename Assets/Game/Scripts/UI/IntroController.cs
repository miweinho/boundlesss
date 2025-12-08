using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [Header("Componentes")]
    public VideoPlayer videoPlayer;
    public GameObject startButton;

    void Start()
    {
        if (startButton != null)
            startButton.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
        else
        {
            Debug.LogError("No video attributed");
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (startButton != null)
            startButton.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("FightScene");
    }
}
