using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ForestExtractionManager : NetworkBehaviour
{
    public static ForestExtractionManager Instance { get; private set; }

    [Header("Scene Change")]
    [SerializeField]
    private NetworkSceneChanger sceneChanger;

    [Header("Extraction")]
    [SerializeField]
    private float extractionDelay = 1f;

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

    public NetworkVariable<bool> ExtractionCountdownActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Coroutine extractionRoutine;
    private bool sceneChangeStarted;

    public override void OnNetworkSpawn()
    {
        Instance = this;

        ExtractionCountdownActive.OnValueChanged +=
            OnExtractionCountdownChanged;

        ApplyExtractionUI(
            ExtractionCountdownActive.Value
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
        ExtractionCountdownActive.OnValueChanged -=
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

        if (extractionRoutine != null)
        {
            return;
        }

        ExtractionCountdownActive.Value = true;

        extractionRoutine =
            StartCoroutine(
                BeginExtractionRoutine()
            );
    }

    private IEnumerator BeginExtractionRoutine()
    {
        Debug.Log(
            $"SERVER: All {ConnectedPlayers.Value} players " +
            $"are ready to extract. Extraction in " +
            $"{extractionDelay:0.#} seconds."
        );

        yield return new WaitForSeconds(
            extractionDelay
        );

        extractionRoutine = null;

        /*
         * Validate again immediately before extraction.
         */
        if (!AreAllPlayersStillReady())
        {
            Debug.Log(
                "SERVER: Extraction cancelled. " +
                "A player is no longer ready."
            );

            ExtractionCountdownActive.Value = false;

            EvaluateReadyPlayers();

            yield break;
        }

        if (sceneChanger == null)
        {
            Debug.LogError(
                "ForestExtractionManager: " +
                "NetworkSceneChanger is not assigned."
            );

            ExtractionCountdownActive.Value = false;
            yield break;
        }

        sceneChangeStarted = true;

        Debug.Log(
            "SERVER: Everyone is still ready. " +
            "Extracting to AppleCity."
        );

        sceneChanger.LoadDestinationScene();
    }

    private void CancelExtractionCountdown()
    {
        if (!IsServer)
        {
            return;
        }

        if (extractionRoutine != null)
        {
            StopCoroutine(extractionRoutine);
            extractionRoutine = null;

            Debug.Log(
                "SERVER: Extraction countdown cancelled."
            );
        }

        if (ExtractionCountdownActive.Value)
        {
            ExtractionCountdownActive.Value = false;
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

    private void OnExtractionCountdownChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyExtractionUI(newValue);
    }

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

    private void OnClientConnected(
        ulong clientId)
    {
        EvaluateReadyPlayers();
    }

    private void OnClientDisconnected(
        ulong clientId)
    {
        if (extractionRoutine != null)
        {
            CancelExtractionCountdown();
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