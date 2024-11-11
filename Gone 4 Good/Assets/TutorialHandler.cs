using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    G4GNetworkManager networkManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = FindObjectOfType<G4GNetworkManager>();
        networkManager.StartLanGame();
    }
}
