using UnityEngine;

public class Options
{
    public static float mouseSensitivity;
    public static void LoadOptions()
    {
        // Load options from PlayerPrefs
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
    }

    public static void SaveOptions()
    {
        // Save options to PlayerPrefs
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity); 
    }
}