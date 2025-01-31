using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    // TMP Inputfield
    public TMP_InputField playerNameInputField;
    public TMP_InputField relayCodeInputField;

    public GameObject mainMenu;
    public GameObject connectionMenu;

    public Toggle toggleDDA;
    void Start()
    {
        // load the player name
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerName");
        }
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

    public void SwitchToConnectionMenu()
    {
        mainMenu.SetActive(false);
        connectionMenu.SetActive(true);
    }

    public void SwitchToMainMenu()
    {
        mainMenu.SetActive(true);
        connectionMenu.SetActive(false);
    }

    public void StartHost()
    {
        NetworkGameManager.enableDDA = toggleDDA.isOn;
        // chagne scene and start host once scene is loaded
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            G4GNetworkManager networkManager = FindObjectOfType<G4GNetworkManager>();
            networkManager.StartRelayGame();
        };
        SceneManager.LoadScene("SampleScene");
    }

    public void ConnectToRelay()
    {
        NetworkGameManager.enableDDA = toggleDDA.isOn;
        // chagne scene and start host once scene is loaded
        SceneManager.sceneLoaded += (scene, mode) =>
        {

            G4GNetworkManager networkManager = FindObjectOfType<G4GNetworkManager>();
            networkManager.JoinRelayGame(relayCodeInputField.text);
        };
        SceneManager.LoadScene("SampleScene");

    }
}
