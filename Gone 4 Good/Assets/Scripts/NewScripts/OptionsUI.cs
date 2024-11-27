using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{

	public Slider moseSensitiviySlider;
	public TextMeshProUGUI mouseSensitivityText;
    public Toggle vSync;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

    private void Start()
    {
        UpdateMouseSensitivty(Options.mouseSensitivity);
        moseSensitiviySlider.value = Options.mouseSensitivity;
        vSync.isOn = Options.VSync;
        masterVolumeSlider.value = Options.MasterVolume;
        musicVolumeSlider.value = Options.MusicVolume;
        sfxVolumeSlider.value = Options.SfxVolume;
        UpdateMasterVolume(Options.MasterVolume);
        UpdateMusicVolume(Options.MusicVolume);
        UpdateSfxVolume(Options.SfxVolume);
    }
    public void UpdateMouseSensitivty(float value)
	{
		Options.mouseSensitivity = value;
		mouseSensitivityText.text = value.ToString("F2");
        Options.SaveOptions();
	}

    public void UpdateMasterVolume(float value)
    {
        Options.MasterVolume = value;
        masterVolumeText.text = value.ToString("F2");
        Options.SaveOptions();
    }

    public void UpdateMusicVolume(float value)
    {
        Options.MusicVolume = value;
        musicVolumeText.text = value.ToString("F2");
        Options.SaveOptions();
    }

    public void UpdateSfxVolume(float value)
    {
        Options.SfxVolume = value;
        sfxVolumeText.text = value.ToString("F2");
        Options.SaveOptions();
    }

    public void UpdateVSync(bool value)
    {
        Options.VSync = value;
        Options.SaveOptions();
    }
}