using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HubDepartureManager : NetworkBehaviour
{
    public static HubDepartureManager Instance { get; private set; }

    [Header("Scene Change")]
    [SerializeField]
    private NetworkSceneChanger sceneChanger;

    [Header("Departure")]
    [SerializeField]
    private float departureDelay = 3f;

    public NetworkVariable<int> ReadyPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> ConnectedPlayers =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> DepartureCountdownActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Coroutine departureRoutine;
    private bool sceneChangeStarted;

    public override void OnNetworkSpawn()
    {
        Instance = this;

        DepartureCountdownActive.OnValueChanged +=
            OnDepartureCountdownChanged;

        /*
         * Apply current state for late-spawning clients.
         */
        ApplyDepartureCountdownUI(
            DepartureCountdownActive.Value
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

    public override void OnNetworkDespawn()
    {
        DepartureCountdownActive.OnValueChanged -=
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

    public void EvaluateReadyPlayers()
    {
        if (!IsServer)
        {
            return;
        }

        if (sceneChangeStarted)
        {
            return;
        }

        GetReadyCounts(
            out int playerCount,
            out int readyCount
        );

        ConnectedPlayers.Value = playerCount;
        ReadyPlayers.Value = readyCount;

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

        if (departureRoutine != null)
        {
            return;
        }

        DepartureCountdownActive.Value = true;

        departureRoutine =
            StartCoroutine(
                BeginDepartureRoutine()
            );
    }

    private IEnumerator BeginDepartureRoutine()
    {
        Debug.Log(
            $"SERVER: All {ConnectedPlayers.Value} players " +
            $"are Ready. Departure in {departureDelay:0.#} seconds."
        );

        yield return new WaitForSeconds(
            departureDelay
        );

        departureRoutine = null;

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

            DepartureCountdownActive.Value = false;

            EvaluateReadyPlayers();

            yield break;
        }

        if (sceneChanger == null)
        {
            Debug.LogError(
                "HubDepartureManager: " +
                "NetworkSceneChanger is not assigned."
            );

            DepartureCountdownActive.Value = false;
            yield break;
        }

        sceneChangeStarted = true;

        Debug.Log(
            "SERVER: Everyone is still Ready. " +
            "Loading AppleForest."
        );

        sceneChanger.LoadDestinationScene();
    }

    private void CancelDepartureCountdown()
    {
        if (!IsServer)
        {
            return;
        }

        if (departureRoutine != null)
        {
            StopCoroutine(departureRoutine);
            departureRoutine = null;

            Debug.Log(
                "SERVER: Departure countdown cancelled."
            );
        }

        if (DepartureCountdownActive.Value)
        {
            DepartureCountdownActive.Value = false;
        }
    }

    private bool AreAllPlayersStillReady()
    {
        GetReadyCounts(
            out int playerCount,
            out int readyCount
        );

        return playerCount > 0 &&
               readyCount == playerCount;
    }

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

    private void OnDepartureCountdownChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyDepartureCountdownUI(newValue);
    }

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

    private void OnClientConnected(
        ulong clientId)
    {
        EvaluateReadyPlayers();
    }

    private void OnClientDisconnected(
        ulong clientId)
    {
        if (departureRoutine != null)
        {
            CancelDepartureCountdown();
        }

        StartCoroutine(
            EvaluateNextFrame()
        );
    }

    private IEnumerator EvaluateNextFrame()
    {
        yield return null;

        EvaluateReadyPlayers();
    }
}