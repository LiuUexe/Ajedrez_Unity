using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public TMP_InputField ipInput;
    public ushort port = 9000;
    public NetworkDriver driver;
    public NetworkConnection connection;
    void Start()
    {
        driver = NetworkDriver.Create();
        connection = default(NetworkConnection);
        var serverEndpoint = NetworkEndpoint.Parse(ipInput.text, port);
        connection = driver.Connect(serverEndpoint);
           
    }

    void OnDestroy()
    {
        connection.Disconnect(driver);
        connection = default(NetworkConnection);
        driver.Dispose();
    }
    void Update()
    {
        driver.ScheduleUpdate().Complete();
        CheckConnection();

    }

    private void CheckConnection()
    {
        if (!connection.IsCreated)
        {
            Debug.Log("Something went wrong during connect");
            return;
        }
    }
}
