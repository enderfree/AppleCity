using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is all the logic used in the ready system... the actual going from the city to the forest is managed by NetworkSceneChanger
/// </summary>
public class ForestExtractionManager : NetworkBehaviour
{
    // a static reference to this manager
    public static ForestExtractionManager Instance { get; private set; }

    [Header("Scene Change")]
    // the class in charge of changing the scene to another
    [SerializeField]
    private NetworkSceneChanger _sceneChanger;

    [Header("Extraction")]
    // delay before the logic actually starts (likely to let player back out last minute if needed)
    [SerializeField]
    private float _extractionDelay = 1f;

    // The amout of players ready to start the game
    public NetworkVariable<int> _readyPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // The ammount of players connected to the server
    public NetworkVariable<int> _connectedPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // if the countdown before game start is counting down rn
    public NetworkVariable<bool> _extractionCountdownActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // The coroutine in which the scene change and countdown are happening
    private Coroutine _extractionRoutine;

    // if the scene change has begun or not
    private bool _sceneChangeStarted;

    /// <summary>
    /// Handles new people connecting to the server
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Instance = this;

        _extractionCountdownActive.OnValueChanged +=
            OnExtractionCountdownChanged;

        ApplyExtractionUI(
            _extractionCountdownActive.Value
        );

        if (!IsServer)
        {
            return;
        }

        NetworkManager.OnClientConnectedCallback +=
            OnClientConnected;

        NetworkManager.OnClientDisconnectCallback +=
            OnClientDisconnected;

        EvaluateReadyPlayers();
    }

    /// <summary>
    /// Handles people disconnecting from the server
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _extractionCountdownActive.OnValueChanged -=
            OnExtractionCountdownChanged;

        TravelUI.Instance?.HideTransition();

        if (IsServer &&
            NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -=
                OnClientConnected;

            NetworkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;
        }

        CancelExtractionCountdown();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Verifies if everyone is ready. 
    /// </summary>
    public void EvaluateReadyPlayers()
    {
        if (!IsServer)
        {
            return;
        }

        if (_sceneChangeStarted)
        {
            return;
        }

        GetReadyCounts(
            out int playerCount,
            out int readyCount
        );

        _connectedPlayers.Value = playerCount;
        _readyPlayers.Value = readyCount;

        Debug.Log(
            $"SERVER: Extraction Ready Players = " +
            $"{readyCount}/{playerCount}"
        );

        bool everyoneReady =
            playerCount > 0 &&
            readyCount == playerCount;

        if (!everyoneReady)
        {
            CancelExtractionCountdown();
            return;
        }

        if (_extractionRoutine != null)
        {
            return;
        }

        _extractionCountdownActive.Value = true;

        _extractionRoutine =
            StartCoroutine(
                BeginExtractionRoutine()
            );
    }

    /// <summary>
    /// Arraganges everything to procede to the scene change
    /// </summary>
    /// <returns>a delay</returns>
    private IEnumerator BeginExtractionRoutine()
    {
        Debug.Log(
            $"SERVER: All {_connectedPlayers.Value} players " +
            $"are ready to extract. Extraction in " +
            $"{_extractionDelay:0.#} seconds."
        );

        yield return new WaitForSeconds(
            _extractionDelay
        );

        _extractionRoutine = null;

        /*
         * Validate again immediately before extraction.
         */
        if (!AreAllPlayersStillReady())
        {
            Debug.Log(
                "SERVER: Extraction cancelled. " +
                "A player is no longer ready."
            );

            _extractionCountdownActive.Value = false;

            EvaluateReadyPlayers();

            yield break;
        }

        if (_sceneChanger == null)
        {
            Debug.LogError(
                "ForestExtractionManager: " +
                "NetworkSceneChanger is not assigned."
            );

            _extractionCountdownActive.Value = false;
            yield break;
        }

        _sceneChangeStarted = true;

        Debug.Log(
            "SERVER: Everyone is still ready. " +
            "Extracting to AppleCity."
        );

        _sceneChanger.LoadDestinationScene();
    }

    /// <summary>
    /// Cancels the countdown
    /// </summary>
    private void CancelExtractionCountdown()
    {
        if (!IsServer)
        {
            return;
        }

        if (_extractionRoutine != null)
        {
            StopCoroutine(_extractionRoutine);
            _extractionRoutine = null;

            Debug.Log(
                "SERVER: Extraction countdown cancelled."
            );
        }

        if (_extractionCountdownActive.Value)
        {
            _extractionCountdownActive.Value = false;
        }
    }

    /// <summary>
    /// Verifies that all players are ready
    /// </summary>
    /// <returns>If all players are ready or not</returns>
    private bool AreAllPlayersStillReady()
    {
        GetReadyCounts(
            out int playerCount,
            out int readyCount
        );

        return playerCount > 0 &&
               readyCount == playerCount;
    }

    /// <summary>
    /// Get how many players are ready
    /// </summary>
    /// <param name="playerCount">Number of player connected</param>
    /// <param name="readyCount">Number of player ready</param>
    private void GetReadyCounts(
        out int playerCount,
        out int readyCount)
    {
        playerCount = 0;
        readyCount = 0;

        if (NetworkManager == null)
        {
            return;
        }

        foreach (
            NetworkClient client
            in NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
            {
                continue;
            }

            HubDeparturePlayer player =
                client.PlayerObject
                    .GetComponent<HubDeparturePlayer>();

            if (player == null)
            {
                continue;
            }

            playerCount++;

            if (player.IsReady.Value)
            {
                readyCount++;
            }
        }
    }

    /// <summary>
    /// Progresses the countdown
    /// </summary>
    /// <param name="previousValue">I have no idea...</param>
    /// <param name="newValue">I have no idea...</param>
    private void OnExtractionCountdownChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyExtractionUI(newValue);
    }

    /// <summary>
    /// show the UI of people readying and the countdown when appropriate
    /// </summary>
    /// <param name="countdownActive">if the countdown is active or not</param>
    private void ApplyExtractionUI(
        bool countdownActive)
    {
        if (countdownActive)
        {
            TravelUI.Instance?.ShowTransition(
                "Extracting to Apple City..."
            );
        }
        else
        {
            TravelUI.Instance?.HideTransition();
        }
    }

    /// <summary>
    /// Add the new client to the ready system when they connect
    /// </summary>
    /// <param name="clientId">the id of the client that just connected</param>
    private void OnClientConnected(
        ulong clientId)
    {
        EvaluateReadyPlayers();
    }

    /// <summary>
    /// Cancels the countdown if someone disconnects
    /// </summary>
    /// <param name="clientId">the id of the client that just disconnected</param>
    private void OnClientDisconnected(
        ulong clientId)
    {
        if (_extractionRoutine != null)
        {
            CancelExtractionCountdown();
        }

        StartCoroutine(
            EvaluateNextFrame()
        );
    }

    /// <summary>
    /// What the heck is that? Probably just a wrapper to run EvaluateReadyPlayers() as a coroutine, but what a name...
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator EvaluateNextFrame()
    {
        yield return null;

        EvaluateReadyPlayers();
    }
}