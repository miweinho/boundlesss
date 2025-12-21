using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateService : MonoBehaviour
{
    public static GameStateService Instance { get; private set; }

    [Header("Persistence")]
    [SerializeField] private bool loadOnAwake = true;
    [SerializeField] private bool autoSaveOnChange = true;
    [SerializeField] private string playerPrefsKey = "boundless.gamestate.v1";

    public event Action<string, bool> OnFlagChanged;

    [Serializable]
    private struct FlagEntry
    {
        public string key;
        public bool value;
    }

    [Serializable]
    private class SaveData
    {
        public List<FlagEntry> flags = new List<FlagEntry>();
    }

    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>(StringComparer.Ordinal);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadOnAwake)
            Load();
    }

    public bool GetFlag(string key, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(key)) return defaultValue;
        return _flags.TryGetValue(key, out var v) ? v : defaultValue;
    }

    public void SetFlag(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        if (_flags.TryGetValue(key, out var oldValue) && oldValue == value)
            return;

        _flags[key] = value;
        OnFlagChanged?.Invoke(key, value);

        if (autoSaveOnChange)
            Save();
    }

    public void Save()
    {
        var data = new SaveData();
        foreach (var kv in _flags)
            data.flags.Add(new FlagEntry { key = kv.Key, value = kv.Value });

        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(playerPrefsKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        _flags.Clear();

        if (!PlayerPrefs.HasKey(playerPrefsKey))
            return;

        var json = PlayerPrefs.GetString(playerPrefsKey, "");
        if (string.IsNullOrWhiteSpace(json))
            return;

        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return;
        }

        if (data?.flags == null) return;

        for (int i = 0; i < data.flags.Count; i++)
        {
            var e = data.flags[i];
            if (string.IsNullOrWhiteSpace(e.key)) continue;
            _flags[e.key] = e.value;
        }
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }
}