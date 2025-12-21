using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    private int _muteRequests;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Request-based muting (safe for nested callers)
    public void PushMute()
    {
        _muteRequests++;
        ApplyMuteState();
    }

    public void PopMute()
    {
        _muteRequests = Mathf.Max(0, _muteRequests - 1);
        ApplyMuteState();
    }

    private void ApplyMuteState()
    {
        if (musicSource == null) return;

        bool muted = _muteRequests > 0;
        musicSource.mute = muted;          // or: musicSource.Pause()/UnPause()
    }
}
