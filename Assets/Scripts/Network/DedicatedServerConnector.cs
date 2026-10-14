using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// This is the class that manages client connection to a dedicated server
/// </summary>
public class DedicatedServerConnector : MonoBehaviour
{
    [Header("Dedicated Server")]
    // The server's IPv4
    [SerializeField] private string _serverAddress = "127.0.0.1";
    // The port on which the server is hosted
    [SerializeField] private ushort _serverPort = 7777;
    
    /// <summary>
    /// I'm not sure why Josh is subscribing to events on Awake() and unsubscribing to them on Destroy()
    /// instead of subscribing on OnEnable() and unsubscribing on OnDisable(), so I didn't dare to touch it
    /// </summary>
    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    /// <summary>
    /// I'm not sure why Josh is subscribing to events on Awake() and unsubscribing to them on Destroy()
    /// instead of subscribing on OnEnable() and unsubscribing on OnDisable(), so I didn't dare to touch it
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

    /// <summary>
    /// I am not sure when Connect() and ConnectLocalHost() are called as right now they are not since Jun moved us to LAN only after Josh shut the server. 
    /// I imagine it depended on if you're connecting to the game from a client or the server itself. 
    /// </summary>
    public void Connect()
    {
        ConnectToServer(_serverAddress);
    }

    /// <summary>
    /// I am not sure when Connect() and ConnectLocalHost() are called as right now they are not since Jun moved us to LAN only after Josh shut the server. 
    /// I imagine it depended on if you're connecting to the game from a client or the server itself. 
    /// </summary>
    public void ConnectLocalhost()
    {
        ConnectToServer("127.0.0.1");
    }

    /// <summary>
    /// The logic that allows to connect to a dedicated server
    /// </summary>
    /// <param name="address">The address of the server you are trying to connect to</param>
    public void ConnectToServer(string address)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "DedicatedServerConnector: NetworkManager.Singleton not found."
            );

            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning(
                "NetworkManager is already running."
            );

            return;
        }

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError(
                "DedicatedServerConnector: UnityTransport not found."
            );

            return;
        }

        _serverAddress = address;

        transport.SetConnectionData(
            _serverAddress,
            _serverPort
        );

        Debug.Log(
            $"Connecting to AppleCity Server at " +
            $"{_serverAddress}:{_serverPort}..."
        );

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            Debug.LogError(
                "Failed to start NetworkManager as client."
            );
        }
    }

    /// <summary>
    /// Debug logs for when the client connects
    /// </summary>
    /// <param name="clientId">the id of the client that just connected</param>
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        Debug.Log(
            $"SUCCESS: Connected to AppleCity Dedicated Server! " +
            $"Client ID: {clientId}"
        );
    }

    /// <summary>
    /// Debug logs for when the client disconnects
    /// </summary>
    /// <param name="clientId">the id of the client that just disconnected</param>
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        Debug.LogWarning(
            $"Disconnected from AppleCity Dedicated Server. " +
            $"Client ID: {clientId}"
        );
    }
}