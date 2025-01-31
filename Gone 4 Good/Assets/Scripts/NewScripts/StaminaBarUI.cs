using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    public Image staminaBar;

    public void SetStamina(float value)
    {
        staminaBar.fillAmount = value;
        if(value >= 1)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
