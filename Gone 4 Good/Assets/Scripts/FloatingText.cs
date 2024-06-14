using TMPro;
using UnityEngine;

public  class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI text;

    [Header("Animation Parameters")]
    public float duration = 1f;
    private float initialDuration;
    public float speed = 1f;
    public AnimationCurve curve;
    public AnimationCurve opacityCurve;
    public Vector3 offset = new Vector3(0, 1, 0);
    private Vector3 animationPos;
    private Vector3 worldPos;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        initialDuration = duration;
    }
    public void SetUpText(string value, Vector3 worldPos)
    {
        text.text = value;
        this.worldPos = worldPos;

    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            Destroy(gameObject);
        }
        float evaluation = curve.Evaluate(1-(duration/initialDuration));
        animationPos = Vector3.up * evaluation * speed;
        transform.position = mainCamera.WorldToScreenPoint(worldPos + offset+ animationPos);

        float opacity = opacityCurve.Evaluate(1 - (duration / initialDuration));
        text.color = new Color(text.color.r, text.color.g, text.color.b, opacity);
    }
}