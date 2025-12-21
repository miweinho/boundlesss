using UnityEngine;

public static class DialoguePresenterRuntime
{
    private static NPCDialogue _presenter;

    public static NPCDialogue GetOrCreate(NPCDialogueView viewPrefab, Transform parentOverride = null)
    {
        if (_presenter != null)
            return _presenter;

        var go = new GameObject("DialoguePresenter(Runtime)");
        Object.DontDestroyOnLoad(go);

        _presenter = go.AddComponent<NPCDialogue>();
        _presenter.ConfigureView(viewPrefab, parentOverride != null ? parentOverride : FindCanvasTransform());

        return _presenter;
    }

    private static Transform FindCanvasTransform()
    {
#if UNITY_2023_1_OR_NEWER
        var canvas = Object.FindFirstObjectByType<Canvas>();
#else
        var canvas = Object.FindObjectOfType<Canvas>();
#endif
        return canvas != null ? canvas.transform : null;
    }
}