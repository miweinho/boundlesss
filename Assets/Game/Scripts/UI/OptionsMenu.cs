using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Must match the exposed parameter name in the AudioMixer.")]
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [Tooltip("Must match the exposed parameter name in the AudioMixer.")]
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [Tooltip("Must match the exposed parameter name in the AudioMixer.")]
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Graphics")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Context Buttons (optional)")]
    [SerializeField] private GameObject backToMainMenuButton;
    [SerializeField] private GameObject closeOptionsButton;

    // Slider is expected to be in [0..1]. Never allow 0 because log10(0) = -Infinity.
    private const float MinLinear = 0.0001f;
    private const float MaxLinear = 1f;

    void OnEnable()
    {
        // Enforce slider ranges at runtime (prevents accidental -Infinity).
        ConfigureSlider(masterSlider);
        ConfigureSlider(musicSlider);
        ConfigureSlider(sfxSlider);

        // Remove listeners to avoid double subscriptions when reopening the overlay.
        UnhookUIEvents();

        // Initialize UI without triggering OnValueChanged.
        LoadUIFromSystem();
        UpdateContextButtons();

        // Hook events after initialization.
        HookUIEvents();
    }

    void OnDisable()
    {
        UnhookUIEvents();
    }

    private void ConfigureSlider(Slider s)
    {
        if (s == null) return;

        s.minValue = MinLinear;
        s.maxValue = MaxLinear;
        s.wholeNumbers = false;

        // Clamp current value in case the scene/prefab had 0 or out-of-range.
        s.SetValueWithoutNotify(Mathf.Clamp(s.value, s.minValue, s.maxValue));
    }

    private void HookUIEvents()
    {
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void UnhookUIEvents()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
    }

    private void LoadUIFromSystem()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

        LoadParamToSlider(masterVolumeParam, masterSlider);
        LoadParamToSlider(musicVolumeParam, musicSlider);
        LoadParamToSlider(sfxVolumeParam, sfxSlider);
    }

    private void LoadParamToSlider(string paramName, Slider slider)
    {
        if (slider == null) return;

        // If mixer is missing, do not leave the slider at 0 (would mute everything when moved).
        if (audioMixer == null)
        {
            Debug.LogWarning("[OptionsMenu] No AudioMixer assigned. Falling back to 1.0 on sliders.");
            slider.SetValueWithoutNotify(1f);
            return;
        }

        if (audioMixer.GetFloat(paramName, out float db))
        {
            slider.SetValueWithoutNotify(DbToLinear(db));
        }
        else
        {
            // Parameter not exposed / wrong name -> keep UI usable and clearly warn.
            Debug.LogWarning($"[OptionsMenu] AudioMixer parameter not found/exposed: '{paramName}'. Falling back to 1.0.");
            slider.SetValueWithoutNotify(1f);
        }
    }

    private static float LinearToDb(float linear)
    {
        linear = Mathf.Clamp(linear, MinLinear, MaxLinear);
        return Mathf.Log10(linear) * 20f; // 1.0 -> 0 dB, 0.0001 -> -80 dB
    }

    private static float DbToLinear(float db)
    {
        return Mathf.Clamp(Mathf.Pow(10f, db / 20f), MinLinear, MaxLinear);
    }

    public void SetMasterVolume(float linear01) => SetMixerParam(masterVolumeParam, linear01);
    public void SetMusicVolume(float linear01) => SetMixerParam(musicVolumeParam, linear01);
    public void SetSFXVolume(float linear01) => SetMixerParam(sfxVolumeParam, linear01);

    private void SetMixerParam(string paramName, float linear01)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("[OptionsMenu] No AudioMixer assigned.");
            return;
        }

        float db = LinearToDb(linear01);
        audioMixer.SetFloat(paramName, db);

        // Useful while debugging: verifies that we never write -Infinity
        // Debug.Log($"[OptionsMenu] {paramName} = {linear01:0.0000} -> {db:0.0} dB");
    }

    public void SetFullscreen(bool isFullscreen) => Screen.fullScreen = isFullscreen;

    private void UpdateContextButtons()
    {
        bool mainMenuIsLoaded = GameManager.Instance != null
                                && GameManager.Instance.IsOverlayLoaded("MainMenu");

        if (backToMainMenuButton != null) backToMainMenuButton.SetActive(!mainMenuIsLoaded);
        if (closeOptionsButton != null) closeOptionsButton.SetActive(mainMenuIsLoaded);
    }

    public void Back()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsOverlayLoaded("MainMenu"))
        {
            GameManager.Instance.HideOptionsOverlay();
            return;
        }

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void CloseOptions()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.HideOptionsOverlay();
        else
            SceneManager.UnloadSceneAsync("OptionsMenu");
    }
}
