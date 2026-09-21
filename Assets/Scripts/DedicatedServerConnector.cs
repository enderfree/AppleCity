using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class DedicatedServerConnector : MonoBehaviour
{
    [Header("Dedicated Server")]
    [SerializeField] private string serverAddress = "127.0.0.1";
    [SerializeField] private ushort serverPort = 7777;

    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public void Connect()
    {
        ConnectToServer(serverAddress);
    }

    public void ConnectLocalhost()
    {
        ConnectToServer("127.0.0.1");
    }

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

        serverAddress = address;

        transport.SetConnectionData(
            serverAddress,
            serverPort
        );

        Debug.Log(
            $"Connecting to AppleCity Server at " +
            $"{serverAddress}:{serverPort}..."
        );

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            Debug.LogError(
                "Failed to start NetworkManager as client."
            );
        }
    }

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