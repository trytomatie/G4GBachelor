using UnityEngine;

public class TutorialRemnantHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<FPSController>().playerName.Value = "Tutorial Remnant";
        GetComponent<FPSController>().UpdateNameCardRpc();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
