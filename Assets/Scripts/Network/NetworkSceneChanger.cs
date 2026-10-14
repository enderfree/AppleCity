using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneChanger : NetworkBehaviour
{
    [Header("Destination")]
    [SerializeField] private string destinationScene = "AppleForest";

    private bool sceneChangeStarted;

    /// <summary>
    /// Called by a CLIENT when it wants the server
    /// to perform this scene change.
    /// </summary>
    public void RequestSceneChange()
    {
        if (!IsSpawned)
        {
            Debug.LogWarning(
                "NetworkSceneChanger: NetworkObject is not spawned."
            );

            return;
        }

        RequestSceneChangeRpc();
    }

    [Rpc(
        SendTo.Server,
        RequireOwnership = false
    )]
    private void RequestSceneChangeRpc(
        RpcParams rpcParams = default)
    {
        Debug.Log(
            $"SERVER: Client {rpcParams.Receive.SenderClientId} " +
            $"requested scene '{destinationScene}'."
        );

        LoadDestinationScene();
    }

    /// <summary>
    /// Server-side scene change.
    /// HubDepartureManager can continue calling this directly.
    /// </summary>
    public void LoadDestinationScene()
    {
        if (!IsServer)
        {
            return;
        }

        if (sceneChangeStarted)
        {
            return;
        }

        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError(
                "NetworkSceneChanger: NetworkManager.Singleton not found."
            );

            return;
        }

        if (!networkManager.IsListening)
        {
            Debug.LogError(
                "NetworkSceneChanger: NetworkManager is not running."
            );

            return;
        }

        sceneChangeStarted = true;

        Debug.Log(
            $"SERVER: Loading network scene '{destinationScene}'."
        );

        SceneEventProgressStatus status =
            networkManager.SceneManager.LoadScene(
                destinationScene,
                LoadSceneMode.Single
            );

        if (status != SceneEventProgressStatus.Started)
        {
            sceneChangeStarted = false;

            Debug.LogError(
                $"SERVER: Failed to load '{destinationScene}'. " +
                $"Status: {status}"
            );
        }
    }
}