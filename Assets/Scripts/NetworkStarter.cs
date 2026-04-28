using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    void Awake()
    {
        // Check what role was selected in the Menu Scene
        if (NetworkConfig.CurrentRole == NetworkConfig.Role.Host)
        {
            // If Host, add the Server script to this GameObject
            gameObject.AddComponent<Server>();
            Debug.Log("NetworkStarter: Initialized as SERVER.");
        }
        else if (NetworkConfig.CurrentRole == NetworkConfig.Role.Client)
        {
            Client clientScript = gameObject.AddComponent<Client>();
            clientScript.ipAddress = NetworkConfig.IPAddress;
            Debug.Log($"NetworkStarter: Initialized as CLIENT. Connecting to {NetworkConfig.IPAddress}");
        }
        else
        {
            Debug.LogWarning("NetworkStarter: No role selected. Did you start from the Menu Scene?");
        }
    }
}
