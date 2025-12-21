using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, UIDialog, GameOver }
    public GameState State { get; private set; } = GameState.MainMenu;

    public bool HasActiveGame { get; private set; }

    public event Action<GameState, GameState> OnStateChanged;

    [Header("Overlay Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string optionsMenuSceneName = "OptionsMenu";

    private readonly List<string> _overlayStack = new();
    private string _previousActiveSceneName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Global ESC handling (New Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If any overlay is open: close the topmost; otherwise open the main menu overlay.
            if (_overlayStack.Count > 0) CloseTopOverlay();
            else ShowMainMenuOverlay();
        }
    }

    public bool GameplayActive => State == GameState.Playing;

    public void StartGame()
    {
        // Marks that a playable session exists (enables "Resume" behavior in MainMenuUI).
        HasActiveGame = true;
        SetState(GameState.Playing);
    }

    public void EndGame()
    {
        HasActiveGame = false;
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        if (newState == State) return;

        var oldState = State;
        State = newState;

        Debug.Log($"[GameManager] State: {oldState} -> {newState}");

        // Pause simulation when in menu/dialog overlays.
        Time.timeScale = (State == GameState.Paused || State == GameState.UIDialog) ? 0f : 1f;

        OnStateChanged?.Invoke(oldState, newState);
    }

    // --- Overlay API ---

    public bool IsOverlayLoaded(string sceneName) => SceneManager.GetSceneByName(sceneName).isLoaded;

    public void ShowMainMenuOverlay() => ShowOverlay(mainMenuSceneName);
    public void HideMainMenuOverlay() => HideOverlay(mainMenuSceneName);

    public void ShowOptionsOverlay() => ShowOverlay(optionsMenuSceneName);
    public void HideOptionsOverlay() => HideOverlay(optionsMenuSceneName);

    public void CloseTopOverlay()
    {
        if (_overlayStack.Count == 0) return;
        HideOverlay(_overlayStack[^1]);
    }

    public void ShowOverlay(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (IsOverlayLoaded(sceneName)) return;

        // Remember the active (gameplay) scene when opening the first overlay.
        if (_overlayStack.Count == 0)
            _previousActiveSceneName = SceneManager.GetActiveScene().name;

        // If no game is running, showing the main menu should put us into MainMenu state.
        // Otherwise overlays pause the gameplay.
        if (sceneName == mainMenuSceneName && !HasActiveGame)
            SetState(GameState.MainMenu);
        else
            SetState(GameState.Paused);

        StartCoroutine(LoadOverlayRoutine(sceneName));
    }

    public void HideOverlay(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (!IsOverlayLoaded(sceneName)) return;

        StartCoroutine(UnloadOverlayRoutine(sceneName));
    }

    private IEnumerator LoadOverlayRoutine(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null) yield break;

        while (!op.isDone) yield return null;

        // Push to top of stack (also removes duplicates)
        _overlayStack.Remove(sceneName);
        _overlayStack.Add(sceneName);

        // Optional: set active to help UI focus/navigation.
        var s = SceneManager.GetSceneByName(sceneName);
        if (s.IsValid() && s.isLoaded)
            SceneManager.SetActiveScene(s);
    }

    private IEnumerator UnloadOverlayRoutine(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op != null)
            while (!op.isDone) yield return null;

        _overlayStack.Remove(sceneName);

        // If another overlay is still open, keep paused and set that overlay active.
        if (_overlayStack.Count > 0)
        {
            var top = SceneManager.GetSceneByName(_overlayStack[^1]);
            if (top.IsValid() && top.isLoaded)
                SceneManager.SetActiveScene(top);

            yield break;
        }

        // No overlays open: restore previous active scene (if still loaded).
        if (!string.IsNullOrEmpty(_previousActiveSceneName))
        {
            var prev = SceneManager.GetSceneByName(_previousActiveSceneName);
            if (prev.IsValid() && prev.isLoaded)
                SceneManager.SetActiveScene(prev);
        }

        // Resume or stay in main menu depending on whether a game exists.
        SetState(HasActiveGame ? GameState.Playing : GameState.MainMenu);
    }
}
