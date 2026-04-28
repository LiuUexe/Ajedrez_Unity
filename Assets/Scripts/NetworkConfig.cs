using UnityEngine;

public static class NetworkConfig
{
    public enum Role 
    { 
        None, 
        Host, 
        Client 
    }

    public static Role CurrentRole = Role.None;
    public static string IPAddress = "127.0.0.1";
}