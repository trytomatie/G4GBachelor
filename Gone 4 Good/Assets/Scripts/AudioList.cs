using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioList", menuName = "Audio/AudioList", order = 1)]

public class AudioList : ScriptableObject
{
    public float randomPitchRange = 0;
    public AudioClip[] audioClips;
}

public class TriggerLights : MonoBehaviour
{
    public GameObject[] candles;

    public float timeIntervalCandles = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - timeIntervalCandles >= 0)
        {
            foreach (GameObject Candles in candles)
            {
                {
                    Candles.SetActive(true);
                }
            }
        }

    }

    private IEnumerator Itterator()
    {
        foreach (GameObject Candles in candles)
        {
            Candles.SetActive(true);
            return yield new WaitForSeconds(timeIntervalCandles);
        }
        return null;
    }
}
