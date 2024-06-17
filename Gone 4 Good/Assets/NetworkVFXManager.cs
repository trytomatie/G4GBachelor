using UnityEngine;

public class NetworkVFXManager : MonoBehaviour
{
    private static NetworkVFXManager instance;
    public GameObject[] projectileVFX;
    void Awake()
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



    public static NetworkVFXManager Instance { get => instance;}
}
