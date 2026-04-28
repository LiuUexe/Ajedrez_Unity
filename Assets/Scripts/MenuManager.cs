using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;

    [Header("Connection Settings")]
    [SerializeField] private TMP_InputField ipAddressInput;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }

    public void ClickHostGame()
    {
        Debug.Log("Starting game as Host...");
        
        NetworkConfig.CurrentRole = NetworkConfig.Role.Host;
        
        SceneManager.LoadScene(gameSceneName);
    }

    public void ClickJoinGame()
    {
        string ip = (ipAddressInput != null && !string.IsNullOrEmpty(ipAddressInput.text)) 
            ? ipAddressInput.text 
            : "127.0.0.1";

        Debug.Log($"Attempting to join server at: {ip}");

        NetworkConfig.CurrentRole = NetworkConfig.Role.Client;
        NetworkConfig.IPAddress = ip;

        SceneManager.LoadScene(gameSceneName);
    }
}