using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image hpBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI playerName;

    private float initialWidth;
    private RectTransform hpRect;

    private void Awake()
    {
        hpRect = hpBar.GetComponent<RectTransform>();
        initialWidth = hpRect.sizeDelta.x;
    }
    public void SetHealth(int current,int max)
    {         
        hpRect.sizeDelta = new Vector2(initialWidth * current / max, hpRect.sizeDelta.y);
        hpText.text = current.ToString();
    }
}