using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// The class starting the dedicated server
/// </summary>
public class DedicatedServerBootstrap : MonoBehaviour
{
    [Header("Server Settings")]
    // The port on which the server is running
    [SerializeField] private ushort _serverPort = 7777;

    /// <summary>
    /// Compilation directives in MonoBehavior's Start()? I'm pretty sure this is wrong, but I've never used compilation directives so I don't dare touching it. 
    /// </summary>
    private void Start()
    {
#if UNITY_SERVER && !UNITY_EDITOR
    StartDedicatedServer();
#endif
    }

    /// <summary>
    /// Function that starts the server (only called on host dedicated servers)
    /// </summary>
    private void StartDedicatedServer()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError(
                "DedicatedServerBootstrap: NetworkManager.Singleton was not found."
            );

            return;
        }

        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError(
                "DedicatedServerBootstrap: UnityTransport was not found."
            );

            return;
        }

        /*
         * Listen on every network interface.
         *
         * This works locally, on LAN, and on an internet-hosted server.
         */
        transport.SetConnectionData(
            "0.0.0.0",
            _serverPort,
            "0.0.0.0"
        );

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        bool started = networkManager.StartServer();

        if (started)
        {
            Debug.Log(
                $"AppleCity Dedicated Server started on port {_serverPort}."
            );

            Debug.Log(
                "AppleCity Server listening on 0.0.0.0:" + _serverPort
            );
        }
        else
        {
            Debug.LogError(
                "AppleCity Dedicated Server failed to start."
            );
        }
    }

    /// <summary>
    /// Debug logs for when the client connects... I think they are meant to be server-side only, but I don't think they are at the moment...
    /// </summary>
    /// <param name="clientId">the id of the client that just connected</param>
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log(
            $"APPLECITY SERVER: Client connected! ID = {clientId}"
        );
    }

    /// <summary>
    /// Debug logs for when the client disconnectsconnects... I think they are meant to be server-side only, but I don't think they are at the moment...
    /// </summary>
    /// <param name="clientId">the id of the client that just disconnected</param>
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log(
            $"APPLECITY SERVER: Client disconnected. ID = {clientId}"
        );
    }

    /// <summary>
    /// If this object is broken, kill the event subscription unless the NetworkManager still exists
    /// </summary>
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}