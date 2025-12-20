using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Audio;

public class IntroController : MonoBehaviour
{
    [Header("Components")]
    public VideoPlayer videoPlayer;
    public GameObject startButton;
    public GameObject skipButton;

    [Header("Audio (optional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private float mutedDb = -80f;

    private bool _hasCachedMusicDb;
    private float _cachedMusicDb;

    private bool _didPushMute;
    private bool _ended;

    void Start()
    {
        if (startButton != null) startButton.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Prepare();
        }
        else
        {
            Debug.LogError("[IntroController] No video assigned.");
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        // Only undo what this controller actually did.
        RestoreMusic();
        if (_didPushMute && MusicManager.Instance != null)
        {
            MusicManager.Instance.PopMute();
            _didPushMute = false;
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // Mute game music while intro video plays.
        if (!_didPushMute && MusicManager.Instance != null)
        {
            MusicManager.Instance.PushMute();
            _didPushMute = true;
        }

        // Optional fallback: also mute via mixer parameter if assigned.
        MuteMusic();

        vp.Play();
        if (skipButton != null) skipButton.SetActive(true);

        Debug.Log("[IntroController] Intro video started (music muted).");
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (_ended) return;
        _ended = true;

        RestoreMusic();

        if (_didPushMute && MusicManager.Instance != null)
        {
            MusicManager.Instance.PopMute();
            _didPushMute = false;
        }

        if (startButton != null) startButton.SetActive(true);
        if (skipButton != null) skipButton.SetActive(false);

        Debug.Log("[IntroController] Intro video ended (music restored).");
    }

    public void SkipIntro()
    {
        if (_ended) return;

        if (videoPlayer != null)
            videoPlayer.Stop();

        // Stop() does not reliably trigger loopPointReached; handle end state manually.
        OnVideoEnd(videoPlayer);
    }

    private void MuteMusic()
    {
        if (audioMixer == null) return;

        if (!_hasCachedMusicDb)
        {
            if (audioMixer.GetFloat(musicVolumeParam, out _cachedMusicDb))
                _hasCachedMusicDb = true;
            else
                Debug.LogWarning($"[IntroController] AudioMixer param not found/exposed: '{musicVolumeParam}'.");
        }

        audioMixer.SetFloat(musicVolumeParam, mutedDb);
    }

    private void RestoreMusic()
    {
        if (audioMixer == null) return;
        if (!_hasCachedMusicDb) return;

        audioMixer.SetFloat(musicVolumeParam, _cachedMusicDb);
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            Debug.LogWarning("[IntroController] No GameManager.Instance found. Player will stay blocked (GameplayActive=false).");

        SceneManager.LoadScene("IntroMap");
    }
}
