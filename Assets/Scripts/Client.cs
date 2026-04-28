using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public string ipAddress = "127.0.0.1";
    public ushort port = 9000;
    public NetworkDriver driver;
    public NetworkConnection connection;

    public float timeoutSeconds = 5f;
    private float timer = 0f;
    private bool isConnected = false;
    
    public string menuSceneName = "Menu"; 

    void Start()
    {
        driver = NetworkDriver.Create();
        connection = default(NetworkConnection);
        
        string ip = string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
        var serverEndpoint = NetworkEndpoint.Parse(ip, port);
        
        connection = driver.Connect(serverEndpoint);

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.localPlayerColor = PieceColor.Black;
            Debug.Log($"Intentando conectar a {ip} como Cliente (Negras)...");
        }
    }

    void OnEnable() { BoardManager.OnLocalMoveMade += SendMoveToServer; }
    void OnDisable() { BoardManager.OnLocalMoveMade -= SendMoveToServer; }

    void Update()
    {
        driver.ScheduleUpdate().Complete();
        CheckConnectionAndReadEvents();
        HandleTimeout();
    }

    private void HandleTimeout()
    {
        if (!isConnected)
        {
            timer += Time.deltaTime;
            
            if (timer >= timeoutSeconds)
            {
                Debug.Log("Tiempo de espera agotado o conexión fallida.");
                ReturnToMenu();
            }
        }
    }

    private void CheckConnectionAndReadEvents()
    {
        if (!connection.IsCreated) return;

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("¡Conectado exitosamente al servidor!");
                isConnected = true;
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                int startX = stream.ReadInt();
                int startY = stream.ReadInt();
                int targetX = stream.ReadInt();
                int targetY = stream.ReadInt();

                BoardManager.Instance.ExecuteNetworkMove(startX, startY, targetX, targetY);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                if (!isConnected)
                {
                    Debug.Log("Fallo al conectar: El servidor rechazó la conexión o no existe.");
                }
                else
                {
                    Debug.Log("Desconectado del servidor (el Host cerró la partida).");
                }
                
                connection = default(NetworkConnection);
                ReturnToMenu();
            }
        }
    }

    private void ReturnToMenu()
    {
        NetworkConfig.CurrentRole = NetworkConfig.Role.None;
        
        if (driver.IsCreated)
        {
            driver.Dispose();
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void SendMoveToServer(int startX, int startY, int targetX, int targetY)
    {
        if (!connection.IsCreated) return;

        driver.BeginSend(connection, out var writer);
        writer.WriteInt(startX);
        writer.WriteInt(startY);
        writer.WriteInt(targetX);
        writer.WriteInt(targetY);
        driver.EndSend(writer);
    }

    void OnDestroy()
    {
        if (driver.IsCreated)
        {
            if (connection.IsCreated) connection.Disconnect(driver);
            driver.Dispose();
        }
    }
}