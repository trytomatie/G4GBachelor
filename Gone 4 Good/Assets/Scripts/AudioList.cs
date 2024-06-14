using UnityEngine;

[CreateAssetMenu(fileName = "AudioList", menuName = "Audio/AudioList", order = 1)]

public class AudioList : ScriptableObject
{
    public AudioClip[] audioClips;
}
