using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public Canvas targetCanvas;

    // Singleton
    public static FloatingTextSpawner instance;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        { 
            Destroy(this);
        }
    }

    public void SpawnFloatingText(string text, Transform location)
    {
        GameObject floatingText = Instantiate(floatingTextPrefab,targetCanvas.transform);
        floatingText.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(location.position);
        floatingText.GetComponent<FloatingText>().SetUpText(text,location.position);
    }
}
