using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class DedicatedServerBootstrap : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private ushort serverPort = 7777;

    private void Start()
    {
#if UNITY_SERVER && !UNITY_EDITOR
    StartDedicatedServer();
#endif
    }

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
            serverPort,
            "0.0.0.0"
        );

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        bool started = networkManager.StartServer();

        if (started)
        {
            Debug.Log(
                $"AppleCity Dedicated Server started on port {serverPort}."
            );

            Debug.Log(
                "AppleCity Server listening on 0.0.0.0:" + serverPort
            );
        }
        else
        {
            Debug.LogError(
                "AppleCity Dedicated Server failed to start."
            );
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log(
            $"APPLECITY SERVER: Client connected! ID = {clientId}"
        );
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log(
            $"APPLECITY SERVER: Client disconnected. ID = {clientId}"
        );
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
}