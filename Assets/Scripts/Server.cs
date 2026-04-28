using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    public string ipAddress = "127.0.0.1";
    public ushort port = 9000;

    void Start()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.Parse(ipAddress, port);
        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        if (endpoint.IsValid)
        {
            if (driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port " + port);
            else
                driver.Listen();
        }

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.localPlayerColor = PieceColor.White;
            Debug.Log("Playing as Server: WHITE Pieces");
        }
    }

    // Subscribe to the local move event
    void OnEnable() { BoardManager.OnLocalMoveMade += SendMoveToClient; }
    void OnDisable() { BoardManager.OnLocalMoveMade -= SendMoveToClient; }

    void Update()
    {
        driver.ScheduleUpdate().Complete();
        CleanUpConnections();
        AcceptNewConnections();
        ReadNetworkEvents();
    }

    private void ReadNetworkEvents()
    {
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    // Read the 4 coordinates sent by the client
                    int startX = stream.ReadInt();
                    int startY = stream.ReadInt();
                    int targetX = stream.ReadInt();
                    int targetY = stream.ReadInt();

                    Debug.Log($"Server received move: ({startX},{startY}) to ({targetX},{targetY})");
                    BoardManager.Instance.ExecuteNetworkMove(startX, startY, targetX, targetY);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");
                    connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    // Called automatically when the server moves a White piece locally
    private void SendMoveToClient(int startX, int startY, int targetX, int targetY)
    {
        if (connections.Length == 0 || !connections[0].IsCreated) return;

        driver.BeginSend(connections[0], out var writer);
        writer.WriteInt(startX);
        writer.WriteInt(startY);
        writer.WriteInt(targetX);
        writer.WriteInt(targetY);
        driver.EndSend(writer);
    }

    private void CleanUpConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }

    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            Debug.Log("A Client (Black pieces) has connected!");
        }
    }

    private void OnDestroy()
    {
        if (driver.IsCreated)
        {
            driver.Dispose();
            connections.Dispose();
        }
    }
}