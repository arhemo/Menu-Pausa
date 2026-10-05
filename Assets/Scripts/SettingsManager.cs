using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string FullscreenKey = "Fullscreen";
    private const string ResolutionKey = "Resolution";

    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] private AudioSettingsController audioSettings;

    private List<ResolutionOption> resolutions = new List<ResolutionOption>();

    private float appliedMusicVolume;
    private float appliedSfxVolume;
    private bool appliedFullscreen;
    private int appliedResolutionIndex;

    private float pendingMusicVolume;
    private float pendingSfxVolume;
    private bool pendingFullscreen;
    private int pendingResolutionIndex;

    private bool isChangingUI;

    private struct ResolutionOption
    {
        public int width;
        public int height;

        public ResolutionOption(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return width + " x " + height;
        }
    }

    private void Awake()
    {
        BuildResolutionList();
        LoadSettings();
    }

    private void Start()
    {
        ApplyCurrentSettingsToApplication();
        SetupUIEvents();
    }

    private void SetupUIEvents()
    {
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void BuildResolutionList()
    {
        resolutionDropdown.ClearOptions();
        resolutions.Clear();

        Resolution[] availableResolutions = Screen.resolutions;

        HashSet<string> usedResolutions = new HashSet<string>();

        foreach (Resolution resolution in availableResolutions)
        {
            string key = resolution.width + "x" + resolution.height;

            if (usedResolutions.Contains(key))
                continue;

            usedResolutions.Add(key);
            resolutions.Add(new ResolutionOption(resolution.width, resolution.height));
        }

        List<string> options = new List<string>();

        foreach (ResolutionOption resolution in resolutions)
            options.Add(resolution.ToString());

        resolutionDropdown.AddOptions(options);
    }

    private void LoadSettings()
    {
        pendingMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        pendingSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        pendingFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;

        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

        pendingResolutionIndex = FindResolutionIndex(savedWidth, savedHeight);

        appliedMusicVolume = pendingMusicVolume;
        appliedSfxVolume = pendingSfxVolume;
        appliedFullscreen = pendingFullscreen;
        appliedResolutionIndex = pendingResolutionIndex;

        UpdateUIFromPendingValues();
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
                return i;
        }

        return resolutions.Count > 0 ? resolutions.Count - 1 : 0;
    }

    private void UpdateUIFromPendingValues()
    {
        isChangingUI = true;

        musicSlider.value = pendingMusicVolume;
        sfxSlider.value = pendingSfxVolume;

        fullscreenToggle.isOn = pendingFullscreen;

        if (resolutions.Count > 0)
        {
            resolutionDropdown.value = Mathf.Clamp(pendingResolutionIndex, 0, resolutions.Count - 1);
            resolutionDropdown.RefreshShownValue();
        }

        isChangingUI = false;
    }

    private void OnMusicSliderChanged(float value)
    {
        if (isChangingUI)
            return;

        pendingMusicVolume = value;

        audioSettings.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isChangingUI)
            return;

        pendingSfxVolume = value;

        audioSettings.SetSfxVolume(value);
    }

    private void OnResolutionChanged(int index)
    {
        if (isChangingUI)
            return;

        pendingResolutionIndex = index;
    }

    private void OnFullscreenChanged(bool value)
    {
        if (isChangingUI)
            return;

        pendingFullscreen = value;
    }

    public bool HasUnappliedChanges()
    {
        return !Mathf.Approximately(pendingMusicVolume, appliedMusicVolume) || !Mathf.Approximately(pendingSfxVolume, appliedSfxVolume) || pendingFullscreen != appliedFullscreen || pendingResolutionIndex != appliedResolutionIndex;
    }

    public void OpenSettingsSession()
    {
        UpdateUIFromPendingValues();
    }

    public void ApplySettings()
    {
        appliedMusicVolume = pendingMusicVolume;
        appliedSfxVolume = pendingSfxVolume;
        appliedFullscreen = pendingFullscreen;
        appliedResolutionIndex = pendingResolutionIndex;

        ApplyCurrentSettingsToApplication();

        SaveSettings();
    }

    public void DiscardChanges()
    {
        pendingMusicVolume = appliedMusicVolume;
        pendingSfxVolume = appliedSfxVolume;
        pendingFullscreen = appliedFullscreen;
        pendingResolutionIndex = appliedResolutionIndex;

        audioSettings.SetMusicVolume(appliedMusicVolume);
        audioSettings.SetSfxVolume(appliedSfxVolume);

        UpdateUIFromPendingValues();

        ApplyCurrentSettingsToApplication();
    }

    public void SetDefaultValues()
    {
        pendingMusicVolume = 1f;
        pendingSfxVolume = 1f;

        pendingFullscreen = true;

        pendingResolutionIndex = FindResolutionIndex(Screen.currentResolution.width, Screen.currentResolution.height);

        UpdateUIFromPendingValues();
    }

    private void ApplyCurrentSettingsToApplication()
    {
        audioSettings.SetMusicVolume(appliedMusicVolume);
        audioSettings.SetSfxVolume(appliedSfxVolume);

        FullScreenMode mode = appliedFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        if (resolutions.Count > 0)
        {
            ResolutionOption resolution = resolutions[Mathf.Clamp(appliedResolutionIndex, 0, resolutions.Count - 1)];
            Screen.SetResolution(resolution.width, resolution.height, mode);
        }
        else
            Screen.fullScreenMode = mode;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, appliedMusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, appliedSfxVolume);
        PlayerPrefs.SetInt(FullscreenKey, appliedFullscreen ? 1 : 0);

        if (resolutions.Count > 0)
        {
            ResolutionOption resolution = resolutions[Mathf.Clamp(appliedResolutionIndex, 0, resolutions.Count - 1)];

            PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
        }

        PlayerPrefs.Save();
    }
}