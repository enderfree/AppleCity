using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is all the logic used in the ready system... the actual going from the city to the forest is managed by NetworkSceneChanger
/// </summary>
public class HubDepartureManager : NetworkBehaviour
{
    // static reference to this manager
    public static HubDepartureManager Instance { get; private set; }

    [Header("Scene Change")]
    // the class in charge of changing the scene to another
    [SerializeField]
    private NetworkSceneChanger _sceneChanger;

    [Header("Departure")]
    // delay time before the scene change and after the countdown is over
    [SerializeField]
    private float _departureDelay = 3f;

    // number of players ready
    public NetworkVariable<int> _readyPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // number of players connected
    public NetworkVariable<int> _connectedPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // if the countdown is currently counting down or not
    public NetworkVariable<bool> _departureCountdownActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // The coroutine in which the scene change and countdown are happening
    private Coroutine _departureRoutine;

    // if the scene change has begun or not
    private bool _sceneChangeStarted;

    /// <summary>
    /// Handles new people connecting to the server
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Instance = this;

        _departureCountdownActive.OnValueChanged +=
            OnDepartureCountdownChanged;

        /*
         * Apply current state for late-spawning clients.
         */
        ApplyDepartureCountdownUI(
            _departureCountdownActive.Value
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
        _departureCountdownActive.OnValueChanged -=
            OnDepartureCountdownChanged;

        TravelUI.Instance?.HideTransition();

        if (IsServer &&
            NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -=
                OnClientConnected;

            NetworkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;
        }

        CancelDepartureCountdown();

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
            $"SERVER: Ready Players = " +
            $"{readyCount}/{playerCount}"
        );

        bool everyoneReady =
            playerCount > 0 &&
            readyCount == playerCount;

        if (!everyoneReady)
        {
            CancelDepartureCountdown();
            return;
        }

        if (_departureRoutine != null)
        {
            return;
        }

        _departureCountdownActive.Value = true;

        _departureRoutine =
            StartCoroutine(
                BeginDepartureRoutine()
            );
    }

    /// <summary>
    /// Arraganges everything to procede to the scene change
    /// </summary>
    /// <returns>a delay</returns>
    private IEnumerator BeginDepartureRoutine()
    {
        Debug.Log(
            $"SERVER: All {_connectedPlayers.Value} players " +
            $"are Ready. Departure in {_departureDelay:0.#} seconds."
        );

        yield return new WaitForSeconds(
            _departureDelay
        );

        _departureRoutine = null;

        /*
         * Validate one more time immediately before
         * committing to the scene transition.
         */
        if (!AreAllPlayersStillReady())
        {
            Debug.Log(
                "SERVER: Departure cancelled. " +
                "A player is no longer Ready."
            );

            _departureCountdownActive.Value = false;

            EvaluateReadyPlayers();

            yield break;
        }

        if (_sceneChanger == null)
        {
            Debug.LogError(
                "HubDepartureManager: " +
                "NetworkSceneChanger is not assigned."
            );

            _departureCountdownActive.Value = false;
            yield break;
        }

        _sceneChangeStarted = true;

        Debug.Log(
            "SERVER: Everyone is still Ready. " +
            "Loading AppleForest."
        );

        _sceneChanger.LoadDestinationScene();
    }

    /// <summary>
    /// Cancels the countdown
    /// </summary>
    private void CancelDepartureCountdown()
    {
        if (!IsServer)
        {
            return;
        }

        if (_departureRoutine != null)
        {
            StopCoroutine(_departureRoutine);
            _departureRoutine = null;

            Debug.Log(
                "SERVER: Departure countdown cancelled."
            );
        }

        if (_departureCountdownActive.Value)
        {
            _departureCountdownActive.Value = false;
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

            if (player._isReady.Value)
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
    private void OnDepartureCountdownChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyDepartureCountdownUI(newValue);
    }

    /// <summary>
    /// show the UI of people readying and the countdown when appropriate
    /// </summary>
    /// <param name="countdownActive">if the countdown is active or not</param>
    private void ApplyDepartureCountdownUI(
        bool countdownActive)
    {
        if (countdownActive)
        {
            TravelUI.Instance?.ShowTransition(
                "Departing to AppleForest..."
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
        if (_departureRoutine != null)
        {
            CancelDepartureCountdown();
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