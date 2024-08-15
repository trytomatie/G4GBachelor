using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioList", menuName = "Audio/AudioList", order = 1)]

public class AudioList : ScriptableObject
{
    public float randomPitchRange = 0;
    public AudioClip[] audioClips;

}