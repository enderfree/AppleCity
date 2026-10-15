using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles if the player is pushing the trolley or not
/// </summary>
public enum PlayerState
{
    Normal_State,
    Trolley_State 
}

/// <summary>
/// handles if the player should behave as if pushing a trolley or not
/// </summary>
public class Player_State : NetworkBehaviour
{
    //State of the player
    [SerializeField] private PlayerState _currentState;

    // public accessor for the player state
    public PlayerState readCurrentState => _currentState;

    //Player object ref
    // the player prefab
    [SerializeField] private GameObject _playerPrefab;

    // the script handlng movement
    [SerializeField] private SimplePlayerMovement _movementScript;

    // the script handling trolley inputs
    [SerializeField] private Trolley_PlayerInput _trolleyInput;

    // the player in the network
    [SerializeField] private NetworkObject _playerNetworkObject;

    // the player's character controller
    private CharacterController _cc;

    //Trolley object ref
    // the trolley prefab
    [SerializeField] private GameObject _trolleyPrefab;

    // the trolley script
    [SerializeField] private Trolley_Script _trolleyScript;

    // the troley interraction script
    [SerializeField] private Trolley_Interact _interact;

    // where the player pushing the trolley should be relative to the trolley they push
    [SerializeField] private Transform _pilotPosition;

    /// <summary>
    /// Innit on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// get a few components
    /// </summary>
    private void Innit()
    {
        _movementScript = GetComponent<SimplePlayerMovement>();
        _trolleyInput = GetComponent<Trolley_PlayerInput>();
        _cc = GetComponent<CharacterController>();
        _playerNetworkObject = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// Handle inputs on Update()
    /// </summary>
    private void Update()
    {
        HandleInputs();

        
    }

    /// <summary>
    /// Handles player trying to grab a trolley
    /// </summary>
    private void HandleInputs()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Pressed F");
            if (_trolleyScript != null)
                _trolleyScript.InteractRpc();
        }
    }


    /// <summary>
    /// setup things when trolley gets on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        TransitionTo(PlayerState.Normal_State);
        UiManager.Instance.PlayerInitiateUI();
    }

    /// <summary>
    /// State define whenever player is controlling the character or the trolley or else
    /// </summary>
    /// <param name="next">pending state</param>
    private void TransitionTo(PlayerState next)
    {
        _currentState = next;

        switch (next)
        {
            case PlayerState.Normal_State: PlayerNormalState(); break;
            case PlayerState.Trolley_State: PlayerTrolleyState(); break;
        }
    }


    //Public of TransitionTo
    /// <summary>
    /// This call twice locally and on everyone's side, the local is a quick fix to inconsistent local bugs, which sometime or most of the time pressing F doesn't do anything.(worst: 10 click to make it work once)
    /// </summary>
    /// <param name="next">the pending player state</param>
    public void PublicStateSwitch(PlayerState next)
    {
        TransitionTo(next);
        SetPlayerStateOnServerRpc(next);
    }

    /// <summary>
    /// update player state on the server
    /// </summary>
    /// <param name="next">the pending player state</param>
    [Rpc(SendTo.Everyone)]
    private void SetPlayerStateOnServerRpc(PlayerState next)
    {
        TransitionTo(next);
    }

    /// <summary>
    /// switch out of the trolley state
    /// </summary>
    private void PlayerNormalState() 
    {
        CC_State(true);
        _movementScript.enabled = true;
        _trolleyInput.enabled = false;
        
        //Undo Parenting when return to Normal State
        if (IsServer)
        {
            _playerNetworkObject.TryRemoveParent(true);
        }
        //Clear out pilotPosition reference
        if (_pilotPosition != null)
        {
            _pilotPosition = null;
            _trolleyPrefab = null;
        }
        
    }


    /// <summary>
    /// In this state, player should snap on the Trolley/pilot position while controlling the trolley and is not moving on its own
    //  ideally put the specific player prefab as a child of the trolley and undo it when the player is no longer controlling
    /// </summary>
    private void PlayerTrolleyState() 
    {
        CC_State(false);
        _movementScript.enabled = false;
        _trolleyInput.enabled = true;
        _trolleyInput.GetTrolleyScript(_trolleyScript);

        //Set the player as Trolley child and pilotPosition.
        if (_pilotPosition != null && _trolleyPrefab != null)
        {
            if (IsServer)
            {
                NetworkObject trolleyNet =_trolleyPrefab.GetComponent<NetworkObject>();

                _playerNetworkObject.TrySetParent(trolleyNet, true);
                
            }

            PilotOnPosition();
        }
    }

    /// <summary>
    /// put the pilot in place
    /// </summary>
    private void PilotOnPosition()
    {
        _playerPrefab.transform.position = _pilotPosition.position;
        _playerPrefab.transform.rotation = _pilotPosition.rotation;

    }

    /// <summary>
    /// Character controller is causing a really weird player collision teleport bug on the server while driving the trolley, disabling it is a quick fix
    /// </summary>
    /// <param name="newBool">if the quick fix should be active or not</param>
    private void CC_State(bool newBool)
    {
        _cc.enabled = newBool; 
    }

    /// <summary>
    /// setup the trolley
    /// </summary>
    /// <param name="prefab">trolley prefab</param>
    /// <param name="script">script hadling the trolley</param>
    /// <param name="interact">script handling trolley interractions</param>
    /// <param name="position">position of the trolley</param>
    public void GetTrolleyRef(GameObject prefab, Trolley_Script script, Trolley_Interact interact, Transform position)
    {
        _trolleyPrefab = prefab ;
        _trolleyScript = script;
        _interact = interact;
        _pilotPosition = position;
    }

  

}