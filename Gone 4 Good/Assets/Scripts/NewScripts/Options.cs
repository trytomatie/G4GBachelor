using UnityEngine;
using UnityEngine.Audio;

public class Options
{
    public static float mouseSensitivity;
    private static bool vSync = true;
    private static float masterVolume = 1f;
    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;
    public static AudioMixerGroup musicAudioGroup;
    public static AudioMixerGroup sfxAudioGroup;

    public static void Initilize(AudioMixerGroup musicAudioGroup, AudioMixerGroup sfxAudioGroup)
    {
        Options.musicAudioGroup = musicAudioGroup;
        Options.sfxAudioGroup = sfxAudioGroup;
    }

    public static void LoadOptions()
    {
        // Load options from PlayerPrefs
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
        VSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 50f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 50f);
        SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 50f);

    }

    public static void SaveOptions()
    {
        // Save options to PlayerPrefs
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("VSync", VSync ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
    }

    public static bool VSync 
    { 
        get => vSync; 
        set
        {
            // Set VSync and apply changes
            vSync = value;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
        }
    }

    public static float MasterVolume 
    { 
        get => masterVolume; 
        set
        {
            // Set master volume and apply changes
            masterVolume = value;
            AudioListener.volume = masterVolume/100;
        }
    }
    public static float MusicVolume 
    { 
        get => musicVolume; 
        set
        {
            // Set music volume and apply changes
            musicVolume = value;
            musicAudioGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume/100) * 20);
        }
    }
    public static float SfxVolume
    {
        get => sfxVolume; 
        set
        {
            // Set SFX volume and apply changes
            sfxVolume = value;
            sfxAudioGroup.audioMixer.SetFloat("SfxVolume", Mathf.Log10(sfxVolume/100) * 20);
        }
    }
}