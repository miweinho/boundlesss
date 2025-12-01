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
        // Garante que o botão começa desativado
        if (startButton != null)
            startButton.SetActive(false);

        // Configura eventos do vídeo
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;

            Debug.Log("🎬 A preparar o vídeo de introdução...");
        }
        else
        {
            Debug.LogError("❌ Nenhum VideoPlayer atribuído ao IntroController!");
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("▶️ Vídeo preparado — a iniciar reprodução.");
        vp.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("🏁 Vídeo terminou — a mostrar botão START.");
        if (startButton != null)
            startButton.SetActive(true);
    }

    public void StartGame()
    {
        Debug.Log("🎮 A iniciar o jogo...");
        SceneManager.LoadScene("FightScene");
    }
}
