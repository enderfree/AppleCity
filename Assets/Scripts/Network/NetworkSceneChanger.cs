using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This is the class that actually handles the scene change itself
/// </summary>
public class NetworkSceneChanger : NetworkBehaviour
{
    [Header("Destination")]
    // the name of the scene you're trying to go to
    [SerializeField] private string _destinationScene = "AppleForest";

    // if the scene change started or not
    private bool _sceneChangeStarted;

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

    /// <summary>
    /// Loads the scene with RPC parameters
    /// </summary>
    /// <param name="rpcParams">I don't know, I don't think Josh ever gives custom ones</param>
    [Rpc(
        SendTo.Server,
        RequireOwnership = false // could likely be updated to InvokePermission.Everyone, but I'm just gonnaa leave it alone for now
    )]
    private void RequestSceneChangeRpc(
        RpcParams rpcParams = default)
    {
        Debug.Log(
            $"SERVER: Client {rpcParams.Receive.SenderClientId} " +
            $"requested scene '{_destinationScene}'."
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

        if (_sceneChangeStarted)
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

        _sceneChangeStarted = true;

        Debug.Log(
            $"SERVER: Loading network scene '{_destinationScene}'."
        );

        SceneEventProgressStatus status =
            networkManager.SceneManager.LoadScene(
                _destinationScene,
                LoadSceneMode.Single
            );

        if (status != SceneEventProgressStatus.Started)
        {
            _sceneChangeStarted = false;

            Debug.LogError(
                $"SERVER: Failed to load '{_destinationScene}'. " +
                $"Status: {status}"
            );
        }
    }
}