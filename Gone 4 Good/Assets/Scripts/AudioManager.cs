using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : NetworkBehaviour
{
    public AudioClip[] bgMusic;

    public AudioList[] audioLists;
    public AudioMixerGroup sfxAudioGroup;
    public AudioMixerGroup musicAudioGroup;

    public GameObject audioSourcePrefab;

    public static AudioManager instance;

    private void Awake()
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

    public static void PlaySound(Vector3 position, SoundType type)
    {
        if(type == SoundType.None) return;
        GameObject audioSource = Instantiate(instance.audioSourcePrefab, position, Quaternion.identity);
        AudioSource source = audioSource.GetComponent<AudioSource>();
        source.clip = instance.audioLists[(int)type].audioClips[Random.Range(0, instance.audioLists[(int)type].audioClips.Length)];
        source.outputAudioMixerGroup = instance.sfxAudioGroup;
        source.Play();
        Destroy(audioSource, source.clip.length + 0.1f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlaySoundFromAudiolistRpc(int listIndex, Vector3 pos,float pitch)
    {
        PlayRandomSoundFromList(listIndex, pos,pitch);
    }

    public static void PlayRandomSoundFromList(int ListIndex, Vector3 position,float pitch)
    {
        AudioList audioList = instance.audioLists[ListIndex];
        AudioClip[] audio = audioList.audioClips;
        float randomPitchAdjustment = Random.Range(-audioList.randomPitchRange, audioList.randomPitchRange);
        GameObject audioSource = Instantiate(instance.audioSourcePrefab, position, Quaternion.identity);
        AudioSource source = audioSource.GetComponent<AudioSource>();
        source.clip = audio[Random.Range(0, audio.Length)];
        source.outputAudioMixerGroup = instance.sfxAudioGroup;
        source.pitch = pitch + randomPitchAdjustment;
        source.Play();
        Destroy(audioSource, source.clip.length + 0.1f);
    }

    private int musicIndex = 0;
    public void PlayMusic(int index)
    {
        GetComponent<AudioSource>().clip = instance.bgMusic[musicIndex];
        GetComponent<AudioSource>().outputAudioMixerGroup = instance.musicAudioGroup;
        GetComponent<AudioSource>().Play();
    }

    public void StopMusic(float delay)
    {
        StartCoroutine(FadeMusicOut(delay));
    }

    public IEnumerator FadeMusicOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        float startVolume = GetComponent<AudioSource>().volume;
        while (GetComponent<AudioSource>().volume > 0)
        {
            GetComponent<AudioSource>().volume -= startVolume * Time.deltaTime * 0.125f;
            yield return null;
        }
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().volume = startVolume;
    }
}

public enum HitType
{
    Wood,
    Stone,
    Entity
}

public enum SoundType
{
    None = -1,
    Player_Dash = 0,
    Dog_Bark = 1,
    Dog_Cry = 2
}
