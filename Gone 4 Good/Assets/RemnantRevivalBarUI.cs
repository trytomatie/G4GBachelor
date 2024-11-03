using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemnantRevivalBarUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image fillBar;

    public void SetBar(string name, float fillAmount)
    {
        gameObject.SetActive(true);
        text.text = name;
        fillBar.fillAmount = fillAmount;
    }

    public void HideBar()
    {
        text.text = "";
        fillBar.fillAmount = 0;
        gameObject.SetActive(false);
    }
}
