using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HubDeparturePlayer : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private bool insideDepartureZone;
    private bool waitingForServer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            IsReady.OnValueChanged += OnReadyChanged;

            SceneManager.activeSceneChanged +=
                OnActiveSceneChanged;
        }

        if (IsServer)
        {
            SceneManager.activeSceneChanged +=
                OnServerActiveSceneChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            IsReady.OnValueChanged -= OnReadyChanged;

            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            InteractionTooltipUI.Instance?.Hide();
        }

        if (IsServer)
        {
            SceneManager.activeSceneChanged -=
                OnServerActiveSceneChanged;
        }

        insideDepartureZone = false;
        waitingForServer = false;
    }

    private void Update()
    {
        /*
         * Only the locally-owned Player reads input.
         */
        if (!IsOwner)
        {
            return;
        }

        if (!insideDepartureZone)
        {
            return;
        }

        if (waitingForServer)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        bool requestedReadyState =
            !IsReady.Value;

        waitingForServer = true;

        Debug.Log(
            requestedReadyState
                ? "Requesting READY status..."
                : "Requesting NOT READY status..."
        );

        SetReadyRpc(requestedReadyState);
    }

    public void EnterDepartureZone()
    {
        if (!IsOwner)
        {
            return;
        }

        insideDepartureZone = true;

        RefreshDepartureTooltip();

        Debug.Log(
            IsReady.Value
                ? "Departure Zone: Press E to cancel Ready."
                : "Departure Zone: Press E when ready to leave."
        );
    }

    public void ExitDepartureZone()
    {
        if (!IsOwner)
        {
            return;
        }

        insideDepartureZone = false;

        InteractionTooltipUI.Instance?.Hide();

        /*
         * If the player walks away after becoming Ready,
         * automatically cancel their Ready state.
         */
        if (IsReady.Value &&
            !waitingForServer)
        {
            waitingForServer = true;

            Debug.Log(
                "Leaving Departure Zone. " +
                "Cancelling READY status..."
            );

            SetReadyRpc(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetReadyRpc(
        bool ready,
        RpcParams rpcParams = default)
    {
        /*
         * Prevent one client from changing another
         * player's Ready state.
         */
        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        if (senderClientId != OwnerClientId)
        {
            Debug.LogWarning(
                $"SERVER: Client {senderClientId} attempted " +
                $"to modify Player {OwnerClientId}'s Ready state."
            );

            return;
        }

        IsReady.Value = ready;

        Debug.Log(
            $"SERVER: Player {OwnerClientId} Ready = {ready}"
        );

        HubDepartureManager.Instance?
            .EvaluateReadyPlayers();

        ForestExtractionManager.Instance?
            .EvaluateReadyPlayers();
    }

    private void OnReadyChanged(
        bool previousValue,
        bool newValue)
    {
        waitingForServer = false;

        if (!IsOwner)
        {
            return;
        }

        Debug.Log(
            newValue
                ? "READY to leave Apple City."
                : "NOT READY to leave Apple City."
        );

        /*
         * Immediately update the Synty tooltip if the
         * player is still standing inside the zone.
         */
        if (insideDepartureZone)
        {
            RefreshDepartureTooltip();
        }
        else
        {
            InteractionTooltipUI.Instance?.Hide();
        }
    }

    private void RefreshDepartureTooltip()
    {
        if (!IsOwner ||
            !insideDepartureZone)
        {
            return;
        }

        bool isForest =
            SceneManager.GetActiveScene().name == "AppleForest";

        if (IsReady.Value)
        {
            InteractionTooltipUI.Instance?.Show(
                isForest
                    ? "to cancel extraction"
                    : "to cancel ready",
                "E",
                "Press"
            );
        }
        else
        {
            InteractionTooltipUI.Instance?.Show(
                isForest
                    ? "when ready to extract"
                    : "when ready to leave",
                "E",
                "Press"
            );
        }
    }

    private void OnActiveSceneChanged(
        Scene previousScene,
        Scene newScene)
    {
        if (!IsOwner)
        {
            return;
        }

        /*
         * Triggers can disappear during NGO scene transitions
         * without OnTriggerExit firing.
         *
         * Always clear local state here.
         */
        insideDepartureZone = false;
        waitingForServer = false;

        InteractionTooltipUI.Instance?.Hide();

        Debug.Log(
            $"Player entered scene '{newScene.name}'. " +
            "Departure-zone state cleared."
        );
    }

    private void OnServerActiveSceneChanged(
        Scene previousScene,
        Scene newScene)
    {
        if (!IsServer)
        {
            return;
        }

        /*
         * Player NetworkObjects persist through the scene
         * transition, so their Ready NetworkVariable can persist.
         *
         * Reset it after every scene change.
         */
        if (IsReady.Value)
        {
            IsReady.Value = false;
        }

        Debug.Log(
            $"SERVER: Player {OwnerClientId} Ready reset " +
            $"after entering '{newScene.name}'."
        );
    }
}