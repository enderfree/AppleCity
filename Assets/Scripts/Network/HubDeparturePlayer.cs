using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// This is handling player options and ststus related to HubDepartureManager
/// </summary>
public class HubDeparturePlayer : NetworkBehaviour
{
    // if the player is ready for the game to start or not
    public NetworkVariable<bool> _isReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // if the player is inside of the departure zone 
    // (unreadies you if you leave the departure zone)
    private bool _insideDepartureZone;

    // if this is waiting for an answer from the server
    private bool _waitingForServer;

    /// <summary>
    /// I'm not actually sure what this is. I think it is from NetworkBehavior and triggers when things NGOspawn, but I'm not sure why Josh subscribe to events here...
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _isReady.OnValueChanged += OnReadyChanged;

            SceneManager.activeSceneChanged +=
                OnActiveSceneChanged;
        }

        if (IsServer)
        {
            SceneManager.activeSceneChanged +=
                OnServerActiveSceneChanged;
        }
    }

    /// <summary>
    /// I'm guessing this is another NetworkBehavior function... it looks to work in pair with the otherone I'm not sure of above...
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _isReady.OnValueChanged -= OnReadyChanged;

            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            InteractionTooltipUI.Instance?.Hide();
        }

        if (IsServer)
        {
            SceneManager.activeSceneChanged -=
                OnServerActiveSceneChanged;
        }

        _insideDepartureZone = false;
        _waitingForServer = false;
    }

    /// <summary>
    /// Josh never used unity character controller to make the char and inputs are all over the place, 
    /// so we never had the time to basically redo the whole character (although both me and Jun tried)
    /// </summary>
    private void Update()
    {
        ReadInput();
    }

    /// <summary>
    /// Check if player is readying
    /// </summary>
    private void ReadInput()
    {
        /*
         * Only the locally-owned Player reads input.
         */
        if (!IsOwner)
        {
            return;
        }

        if (!_insideDepartureZone)
        {
            return;
        }

        if (_waitingForServer)
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
            !_isReady.Value;

        _waitingForServer = true;

        Debug.Log(
            requestedReadyState
                ? "Requesting READY status..."
                : "Requesting NOT READY status..."
        );

        SetReadyRpc(requestedReadyState);
    }

    /// <summary>
    /// Handles what happen when player enters the departure zone
    /// </summary>
    public void EnterDepartureZone()
    {
        if (!IsOwner)
        {
            return;
        }

        _insideDepartureZone = true;

        RefreshDepartureTooltip();

        Debug.Log(
            _isReady.Value
                ? "Departure Zone: Press E to cancel Ready."
                : "Departure Zone: Press E when ready to leave."
        );
    }

    /// <summary>
    /// Handles what happen when player exits the departure zone
    /// </summary>
    public void ExitDepartureZone()
    {
        if (!IsOwner)
        {
            return;
        }

        _insideDepartureZone = false;

        InteractionTooltipUI.Instance?.Hide();

        /*
         * If the player walks away after becoming Ready,
         * automatically cancel their Ready state.
         */
        if (_isReady.Value &&
            !_waitingForServer)
        {
            _waitingForServer = true;

            Debug.Log(
                "Leaving Departure Zone. " +
                "Cancelling READY status..."
            );

            SetReadyRpc(false);
        }
    }

    /// <summary>
    /// This seems to be handling the player per player readying
    /// </summary>
    /// <param name="ready">is this player ready or not</param>
    /// <param name="rpcParams">not sure, Josh does not seem to provide custum ones ever</param>
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

        _isReady.Value = ready;

        Debug.Log(
            $"SERVER: Player {OwnerClientId} Ready = {ready}"
        );

        HubDepartureManager.Instance?
            .EvaluateReadyPlayers();

        ForestExtractionManager.Instance?
            .EvaluateReadyPlayers();
    }

    /// <summary>
    /// What to do when players become ready or someone unreadies
    /// </summary>
    /// <param name="previousValue">not sure</param>
    /// <param name="newValue">not sure</param>
    private void OnReadyChanged(
        bool previousValue,
        bool newValue)
    {
        _waitingForServer = false;

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
        if (_insideDepartureZone)
        {
            RefreshDepartureTooltip();
        }
        else
        {
            InteractionTooltipUI.Instance?.Hide();
        }
    }

    /// <summary>
    /// Refreshes the tooltip
    /// </summary>
    private void RefreshDepartureTooltip()
    {
        if (!IsOwner ||
            !_insideDepartureZone)
        {
            return;
        }

        bool isForest =
            SceneManager.GetActiveScene().name == "AppleForest";

        if (_isReady.Value)
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

    /// <summary>
    /// what to do when the scene changes
    /// </summary>
    /// <param name="previousScene">the previous scene</param>
    /// <param name="newScene">the scene you're going to</param>
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
        _insideDepartureZone = false;
        _waitingForServer = false;

        InteractionTooltipUI.Instance?.Hide();

        Debug.Log(
            $"Player entered scene '{newScene.name}'. " +
            "Departure-zone state cleared."
        );
    }

    /// <summary>
    /// what to do server-side when the scene changes
    /// </summary>
    /// <param name="previousScene">the previous scene</param>
    /// <param name="newScene">the scene you're going to</param>
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
        if (_isReady.Value)
        {
            _isReady.Value = false;
        }

        Debug.Log(
            $"SERVER: Player {OwnerClientId} Ready reset " +
            $"after entering '{newScene.name}'."
        );
    }
}