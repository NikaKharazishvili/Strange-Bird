using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>Static helpers shared across the game.</summary>
public static class Utils
{
    #region Coroutine timers
    static readonly Dictionary<float, WaitForSeconds> WaitCache = new(); // One WaitForSeconds per duration to avoid repeated allocations

    /// <summary>Invokes the action after the given seconds, unless the behaviour was destroyed meanwhile. Usage: this.Wait(2f, () => ...);</summary>
    public static void Wait(this MonoBehaviour mb, float seconds, Action action) => mb.StartCoroutine(WaitRoutine(mb, seconds, action));

    static IEnumerator WaitRoutine(MonoBehaviour mb, float seconds, Action action)
    {
        if (!WaitCache.TryGetValue(seconds, out var wait)) WaitCache[seconds] = wait = new WaitForSeconds(seconds);
        yield return wait;
        if (mb) action?.Invoke();
    }
    #endregion


    #region Random & pooling
    /// <summary>True with the given probability (0-100).</summary>
    public static bool PercentChance(int percent) => Random.Range(0, 100) < percent;

    public static T GetRandomElement<T>(IList<T> collection) => collection[Random.Range(0, collection.Count)];

    /// <summary>Returns the first inactive component of the pool, or null if all are in use.</summary>
    public static T PoolComponent<T>(T[] pool) where T : Component
    {
        foreach (var comp in pool) if (!comp.gameObject.activeInHierarchy) return comp;
        return null;
    }
    #endregion


    #region Scene & application
    /// <summary>Loads a scene asynchronously while reporting progress to an optional bar and label.</summary>
    public static IEnumerator LoadSceneAsync(string sceneName, Image loadingBar = null, TextMeshProUGUI loadingText = null, float briefDelay = 0)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Unity stops at 0.9 progress until allowSceneActivation is set
        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f;
            if (loadingText) loadingText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            if (loadingBar) loadingBar.fillAmount = progress;
            yield return null;
        }

        if (loadingText) loadingText.text = "Loading 100%";
        if (loadingBar) loadingBar.fillAmount = 1;
        if (briefDelay > 0) yield return new WaitForSeconds(briefDelay);
        operation.allowSceneActivation = true;
    }

    public static void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion


    #region Persistence & formatting
    public static bool GetPrefBool(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    public static void SetPrefBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

    public static string FpsLabel() => $"FPS: {Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f))}";

    /// <summary>Shortens key names for the UI: Alpha1 -> A1, Keypad1 -> K1, LeftShift -> LShift, BackQuote -> BQuote.</summary>
    public static string FormatKeyName(string keyName) => keyName.Replace("Alpha", "A").Replace("Keypad", "K").Replace("Left", "L").Replace("Right", "R").Replace("Back", "B");
    #endregion
}
