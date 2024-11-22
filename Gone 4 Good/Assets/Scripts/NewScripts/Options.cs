using UnityEngine;

public class Options
{
    public static float mouseSensitivity;
    private static bool vSync = true;

    public static void LoadOptions()
    {
        // Load options from PlayerPrefs
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
        VSync = PlayerPrefs.GetInt("VSync", 1) == 1;
    }

    public static void SaveOptions()
    {
        // Save options to PlayerPrefs
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("VSync", VSync ? 1 : 0);
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

}