using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    public NetworkDriver driver;
    public string ipAdress = "127.0.0.1";
    public ushort port = 9000;

    void Start()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.Parse(ipAdress, port);
        if (endpoint.IsValid)
        {
            driver = NetworkDriver.Create();
            if (driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port " + port);
            else
                driver.Listen();
        }
        else
        {
            Debug.Log("Invalid IP address");
        }
    }

    void Update()
    {
        ManageConnections();        
    }

    private NativeList<NetworkConnection> connections;
    
    private void ManageConnections()
    {
        driver.ScheduleUpdate().Complete();

        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
                connections.RemoveAtSwapBack(i);
                i--;
        }
    }

    private void OnDestroy()
    {
        if (driver.IsCreated)
        {
            driver.Dispose();
        }
    }
}
