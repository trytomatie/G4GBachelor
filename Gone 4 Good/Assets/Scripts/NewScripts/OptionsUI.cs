using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{

	public Slider moseSensitiviySlider;
	public TextMeshProUGUI mouseSensitivityText;
    public Toggle vSync;

    private void Start()
    {
        UpdateMouseSensitivty(Options.mouseSensitivity);
        moseSensitiviySlider.value = Options.mouseSensitivity;
        vSync.isOn = Options.VSync;
    }
    public void UpdateMouseSensitivty(float value)
	{
		Options.mouseSensitivity = value;
		mouseSensitivityText.text = value.ToString("F2");
        Options.SaveOptions();
	}

    public void UpdateVSync(bool value)
    {
        Options.VSync = value;
        Options.SaveOptions();
    }
}