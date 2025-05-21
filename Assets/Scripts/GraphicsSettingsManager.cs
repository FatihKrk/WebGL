using UnityEngine;
using TMPro;

public class GraphicsSettingsManager : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown;             // Very Low … Ultra (0–5)
    public TMP_Dropdown anisotropicTextureDropdown;  // Disabled, PerTexture, ForcedOn (0–2)
    public TMP_Dropdown shadowQualityDropdown;       // Low, Medium, High, VeryHigh (0–3)
    public TMP_Dropdown antiAliasingDropdown;        // Disabled, 2x, 4x, 8x (0–3)

    
    private readonly int[] _aaSamples = { 0, 2, 4, 8 };

    private void Start()
    {
        int preset = PlayerPrefs.GetInt("QualityPreset", 0);
        qualityDropdown.value = preset;
        ApplyQualitySettings(preset);

        qualityDropdown.onValueChanged.AddListener(ApplyQualitySettings);
        anisotropicTextureDropdown.onValueChanged.AddListener(_ => UpdateProjectSettings());
        shadowQualityDropdown.onValueChanged.AddListener(_ => UpdateProjectSettings());
        antiAliasingDropdown.onValueChanged.AddListener(_ => UpdateProjectSettings());
    }

    private void ApplyQualitySettings(int preset)
    {
        // Varsayılan ayarlar
        switch (preset)
        {
            case 0: // Very Low
                anisotropicTextureDropdown.value = 0;
                shadowQualityDropdown.value = 0;
                antiAliasingDropdown.value = 1;
                break;
            case 1: // Low
                anisotropicTextureDropdown.value = 0;
                shadowQualityDropdown.value = 0;
                antiAliasingDropdown.value = 0;
                break;
            case 2: // Medium
                anisotropicTextureDropdown.value = 1;
                shadowQualityDropdown.value = 0;
                antiAliasingDropdown.value = 0;
                break;
            case 3: // High
                anisotropicTextureDropdown.value = 1;
                shadowQualityDropdown.value = 1;
                antiAliasingDropdown.value = 0;
                break;
            case 4: // Very High
                anisotropicTextureDropdown.value = 2;
                shadowQualityDropdown.value = 2;
                antiAliasingDropdown.value = 1;
                break;
            case 5: // Ultra
                anisotropicTextureDropdown.value = 2;
                shadowQualityDropdown.value = 2;
                antiAliasingDropdown.value = 1;
                break;
        }

        
        QualitySettings.SetQualityLevel(preset);

        SaveSettings();
        UpdateProjectSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("QualityPreset", qualityDropdown.value);
        PlayerPrefs.SetInt("AnisotropicTexture", anisotropicTextureDropdown.value);
        PlayerPrefs.SetInt("ShadowQuality", shadowQualityDropdown.value);
        PlayerPrefs.SetInt("AntiAliasing", antiAliasingDropdown.value);
        PlayerPrefs.Save();
    }

    private void UpdateProjectSettings()
    {
        // Anisotropic texture filtering
        QualitySettings.anisotropicFiltering =
            (AnisotropicFiltering)anisotropicTextureDropdown.value;

        // Shadow resolution
        QualitySettings.shadowResolution =
            (ShadowResolution)shadowQualityDropdown.value;

        // Anti-aliasing 
        int aaIndex = antiAliasingDropdown.value;
        QualitySettings.antiAliasing = _aaSamples[aaIndex];
    }

    private void OnDestroy()
    {
        qualityDropdown.onValueChanged.RemoveAllListeners();
        anisotropicTextureDropdown.onValueChanged.RemoveAllListeners();
        shadowQualityDropdown.onValueChanged.RemoveAllListeners();
        antiAliasingDropdown.onValueChanged.RemoveAllListeners();
    }
}
