using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    [SerializeField] private GameObject menuPanel;

    [Header("Configuración de Conexión")]
    [Tooltip("Campo donde el jugador escribe la IP o el código de la sala para unirse")]
    [SerializeField] private TMP_InputField inputJoinCodeOrIP; 

    private void Start()
    {
        // Nos aseguramos de que el panel principal esté activo al empezar
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }

    public void ClickHostGame()
    {
        Debug.Log("Iniciando partida como Host...");
        
        // Ocultar el menú (opcional, dependiendo de si cambias de escena o no)
        // menuPanel.SetActive(false);

        // TODO: Aquí debes llamar a la función de tu librería de red.
        // Ejemplo si usas Unity Netcode for GameObjects:
        // NetworkManager.Singleton.StartHost();
        
        // Ejemplo si usas Mirror:
        // NetworkManager.singleton.StartHost();
        
        // Ejemplo si usas Photon (PUN):
        // PhotonNetwork.CreateRoom("SalaAjedrez");
    }

    /// <summary>
    /// Se ejecuta al pulsar el botón "Join Server"
    /// </summary>
    public void ClickJoinGame()
    {
        string joinData = inputJoinCodeOrIP != null ? inputJoinCodeOrIP.text : "";
        Debug.Log($"Intentando unirse al servidor con el dato: {joinData}");

        // TODO: Aquí debes llamar a la función de tu librería de red.
        // Ejemplo si usas Unity Netcode for GameObjects (necesita configuración de IP previa):
        // NetworkManager.Singleton.StartClient();
        
        // Ejemplo si usas Mirror (asignando la IP primero):
        // NetworkManager.singleton.networkAddress = joinData;
        // NetworkManager.singleton.StartClient();
        
        // Ejemplo si usas Photon (PUN):
        // PhotonNetwork.JoinRoom(joinData);
    }
}