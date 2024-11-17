using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{

	public Slider moseSensitiviySlider;
	public TextMeshProUGUI mouseSensitivityText;

    private void Start()
    {
        UpdateMouseSensitivty(Options.mouseSensitivity);
        moseSensitiviySlider.value = Options.mouseSensitivity;
    }
    public void UpdateMouseSensitivty(float value)
	{
		Options.mouseSensitivity = value;
		mouseSensitivityText.text = value.ToString("F2");
        Options.SaveOptions();
	}
}