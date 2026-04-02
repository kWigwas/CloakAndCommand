using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wire UI controls here (sliders, toggles, dropdown). Optionally assign an AudioMixer;
/// expose float params named like masterParamName (dB) for each group.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SettingsMenu : MonoBehaviour
{
    [Header("UI — optional, assign what you use")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Audio (optional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterMixerParameter = "MasterVol";
    [SerializeField] private string musicMixerParameter = "MusicVol";
    [SerializeField] private string sfxMixerParameter = "SfxVol";

    private void Awake()
    {
        TryBindSlidersFromHierarchy();
    }

    private void OnEnable()
    {
        GameSettings.EnsureLoaded();
        TryBindSlidersFromHierarchy();
        WireUiHandlers();
        RefreshUIFromSettings();
        ApplyMixerFromSavedSettings();
        StartCoroutine(DeferredRefreshSlidersAndMixer());
    }

    private void OnDisable()
    {
        SaveFromCurrentUiState();
        UnwireUiHandlers();
    }

    private static float SliderToNormalized(Slider s, float rawValue)
    {
        if (s == null)
        {
            // Fallback for missing references: many UIs use 0..100 sliders.
            if (rawValue > 1f)
                return Mathf.Clamp01(rawValue / 100f);
            return Mathf.Clamp01(rawValue);
        }
        float min = s.minValue;
        float max = s.maxValue;
        if (max - min < 0.0001f) return 1f;
        return Mathf.Clamp01((rawValue - min) / (max - min));
    }

    private static float NormalizedToSlider(Slider s, float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (s == null) return normalized;
        float min = s.minValue;
        float max = s.maxValue;
        return Mathf.Lerp(min, max, normalized);
    }

    private void SaveFromCurrentUiState()
    {
        // Persist even if UI events were not wired in Inspector/runtime for some reason.
        if (masterSlider != null)
            GameSettings.SetMasterVolume(SliderToNormalized(masterSlider, masterSlider.value));
        if (musicSlider != null)
            GameSettings.SetMusicVolume(SliderToNormalized(musicSlider, musicSlider.value));
        if (sfxSlider != null)
            GameSettings.SetSfxVolume(SliderToNormalized(sfxSlider, sfxSlider.value));
        if (fullscreenToggle != null)
            GameSettings.SetFullscreen(fullscreenToggle.isOn);
        if (vsyncToggle != null)
            GameSettings.SetVsync(vsyncToggle.isOn);
        if (qualityDropdown != null)
            GameSettings.SetQualityLevel(qualityDropdown.value);

        GameSettings.ApplyDisplay();
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
        GameSettings.Save();
    }

    private void WireUiHandlers()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
        if (vsyncToggle != null)
        {
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
            vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        }
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    private void UnwireUiHandlers()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
    }

    /// <summary>Bind sliders by name when references are missing (SettingsManager is not parented under the canvas).</summary>
    private void TryBindSlidersFromHierarchy()
    {
        masterSlider = FindBestNamedSlider("MasterSlider", masterSlider);
        musicSlider = FindBestNamedSlider("MusicSlider", musicSlider);
        sfxSlider = FindBestNamedSlider("SFXSlider", sfxSlider);
    }

    private static Slider FindBestNamedSlider(string name, Slider fallback)
    {
        List<Slider> candidates = new List<Slider>(8);
        foreach (var s in FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (s != null && s.gameObject.name == name)
                candidates.Add(s);
        }

        if (candidates.Count == 0)
            return fallback;
        if (candidates.Count == 1)
            return candidates[0];

        // Prefer the currently active slider under a Canvas (visible UI), then preserve fallback.
        Slider best = null;
        int bestScore = int.MinValue;
        foreach (var s in candidates)
        {
            int score = 0;
            if (s.gameObject.activeInHierarchy) score += 100;
            if (s.enabled) score += 50;
            if (s.GetComponentInParent<Canvas>() != null) score += 25;
            if (fallback != null && s == fallback) score += 10;
            if (score > bestScore)
            {
                best = s;
                bestScore = score;
            }
        }

        return best ?? fallback;
    }

    public void RefreshUIFromSettings()
    {
        GameSettings.EnsureLoaded();

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(NormalizedToSlider(masterSlider, GameSettings.MasterVolume));
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(NormalizedToSlider(musicSlider, GameSettings.MusicVolume));
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(NormalizedToSlider(sfxSlider, GameSettings.SfxVolume));
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        if (vsyncToggle != null)
            vsyncToggle.SetIsOnWithoutNotify(GameSettings.Vsync);

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (string name in QualitySettings.names)
                options.Add(new TMP_Dropdown.OptionData(name));
            qualityDropdown.AddOptions(options);
            qualityDropdown.SetValueWithoutNotify(GameSettings.QualityLevel);
            qualityDropdown.RefreshShownValue();
        }

        RebuildSliderVisuals(masterSlider, musicSlider, sfxSlider);
    }

    void ApplyMixerFromSavedSettings()
    {
        if (audioMixer == null) return;
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
    }

    IEnumerator DeferredRefreshSlidersAndMixer()
    {
        yield return null;
        TryBindSlidersFromHierarchy();
        RefreshUIFromSettings();
        ApplyMixerFromSavedSettings();
    }

    static void RebuildSliderVisuals(Slider master, Slider music, Slider sfx)
    {
        Canvas.ForceUpdateCanvases();
        RebuildSliderLayout(master);
        RebuildSliderLayout(music);
        RebuildSliderLayout(sfx);
    }

    static void RebuildSliderLayout(Slider s)
    {
        if (s == null) return;
        var rt = s.transform as RectTransform;
        if (rt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public void OnMasterVolumeChanged(float value)
    {
        GameSettings.SetMasterVolume(SliderToNormalized(masterSlider, value));
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
    }

    public void OnMusicVolumeChanged(float value)
    {
        GameSettings.SetMusicVolume(SliderToNormalized(musicSlider, value));
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
    }

    public void OnSfxVolumeChanged(float value)
    {
        GameSettings.SetSfxVolume(SliderToNormalized(sfxSlider, value));
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
    }

    public void OnFullscreenChanged(bool value)
    {
        GameSettings.SetFullscreen(value);
        GameSettings.ApplyDisplay();
    }

    public void OnVsyncChanged(bool value)
    {
        GameSettings.SetVsync(value);
        GameSettings.ApplyDisplay();
    }

    public void OnQualityChanged(int index)
    {
        GameSettings.SetQualityLevel(index);
        GameSettings.ApplyDisplay();
    }

    public void OnSaveSettings()
    {
        GameSettings.ApplyDisplay();
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
        GameSettings.Save();
    }

    public void OnResetDefaults()
    {
        GameSettings.ResetToDefaults();
        GameSettings.ApplyAudio(audioMixer, masterMixerParameter, musicMixerParameter, sfxMixerParameter);
        RefreshUIFromSettings();
    }
}
