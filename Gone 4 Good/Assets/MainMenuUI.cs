using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void PersistentDataPathLocation()
    {
        string path = Application.persistentDataPath;
        path = path.Replace(@"/", @"\");
        if (System.IO.Directory.Exists(path)) // Check if the folder exists
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
        }
        else
        {
            print("The specified folder does not exist.");
        }

    }

    public void SavePlayerName(string text)
    {
        PlayerPrefs.SetString("PlayerName", text);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
