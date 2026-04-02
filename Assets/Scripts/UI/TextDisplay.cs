using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Assign a <see cref="TextMeshProUGUI"/> and style it in the Inspector (font, size, alignment, rich text).
/// Call <see cref="Show"/> for one-off lines (location titles, barks) or <see cref="Enqueue"/> to play dialogue lines in order.
/// Fading uses an optional <see cref="CanvasGroup"/> on the same hierarchy; if omitted, the TMP color alpha is driven instead.
/// </summary>
public class TextDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [Tooltip("If set, alpha is applied here (recommended). TMP text color can stay fully opaque.")]
    [SerializeField] CanvasGroup canvasGroup;
    [Tooltip("When true, the root object is disabled while idle so it does not intercept raycasts.")]
    [SerializeField] bool hideRootWhenIdle = true;
    [SerializeField] GameObject rootToToggle;

    [Header("Default timing (seconds)")]
    [SerializeField] float delayBeforeFadeIn = 0f;
    [SerializeField] float fadeInDuration = 0.35f;
    [SerializeField] float holdDuration = 2.5f;
    [SerializeField] float fadeOutDuration = 0.5f;

    [Header("Time")]
    [SerializeField] bool useUnscaledTime;

    readonly Queue<string> _queue = new Queue<string>();
    Coroutine _running;
    Color _colorFull = Color.white;

    void Awake()
    {
        if (rootToToggle == null)
            rootToToggle = gameObject;
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
            _colorFull = label.color;

        ApplyVisualAlpha(0f);
        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(false);
    }

    void OnValidate()
    {
        delayBeforeFadeIn = Mathf.Max(0f, delayBeforeFadeIn);
        fadeInDuration = Mathf.Max(0f, fadeInDuration);
        holdDuration = Mathf.Max(0f, holdDuration);
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
    }

    float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    /// <summary>Play one message immediately; clears any queued lines and stops the current animation.</summary>
    public void Show(string message)
    {
        Show(message, delayBeforeFadeIn, fadeInDuration, holdDuration, fadeOutDuration);
    }

    public void Show(string message, float delayBeforeIn, float fadeIn, float hold, float fadeOut)
    {
        _queue.Clear();
        StopRunning();
        _running = StartCoroutine(CoShowSingle(message, delayBeforeIn, fadeIn, hold, fadeOut));
    }

    /// <summary>Add a line to the queue. Uses default timing for each line. Starts processing if idle.</summary>
    public void Enqueue(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
        _queue.Enqueue(message);
        if (_running == null)
            _running = StartCoroutine(ProcessQueue());
    }

    public void ClearQueue()
    {
        _queue.Clear();
    }

    public void HideImmediate()
    {
        _queue.Clear();
        StopRunning();
        ApplyVisualAlpha(0f);
        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(false);
    }

    void StopRunning()
    {
        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
        }
    }

    IEnumerator CoShowSingle(string message, float delayBeforeIn, float fadeIn, float hold, float fadeOut)
    {
        yield return StartCoroutine(PlayOne(message, delayBeforeIn, fadeIn, hold, fadeOut));
        _running = null;
    }

    IEnumerator ProcessQueue()
    {
        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(true);

        while (_queue.Count > 0)
        {
            string line = _queue.Dequeue();
            yield return StartCoroutine(PlayOne(line, delayBeforeFadeIn, fadeInDuration, holdDuration, fadeOutDuration));
        }

        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(false);

        _running = null;
    }

    IEnumerator PlayOne(string message, float delayBeforeIn, float fadeIn, float hold, float fadeOut)
    {
        if (label == null)
            yield break;

        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(true);

        _colorFull = label.color;

        label.text = message;
        label.ForceMeshUpdate();

        ApplyVisualAlpha(0f);

        if (delayBeforeIn > 0f)
            yield return WaitFor(delayBeforeIn);

        yield return FadeTo(1f, fadeIn);

        if (hold > 0f)
            yield return WaitFor(hold);

        yield return FadeTo(0f, fadeOut);

        if (hideRootWhenIdle && rootToToggle != null)
            rootToToggle.SetActive(false);
    }

    IEnumerator WaitFor(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += DeltaTime;
            yield return null;
        }
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float start = CurrentVisualAlpha();
        if (duration <= 0f)
        {
            ApplyVisualAlpha(targetAlpha);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += DeltaTime;
            float u = Mathf.Clamp01(t / duration);
            ApplyVisualAlpha(Mathf.Lerp(start, targetAlpha, u));
            yield return null;
        }

        ApplyVisualAlpha(targetAlpha);
    }

    float CurrentVisualAlpha()
    {
        if (canvasGroup != null)
            return canvasGroup.alpha;
        return label != null ? label.color.a : 0f;
    }

    void ApplyVisualAlpha(float a)
    {
        a = Mathf.Clamp01(a);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = a;
            return;
        }

        if (label == null)
            return;

        Color c = _colorFull;
        c.a = _colorFull.a * a;
        label.color = c;
    }
}
