using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerState
{
    Normal_State,
    Trolley_State 
}
public class Player_State : NetworkBehaviour
{
    //State of the player
    
    [SerializeField] private PlayerState _currentState;
    public PlayerState readCurrentState => _currentState;

    //Player object ref
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private SimplePlayerMovement _movementScript;
    [SerializeField] private Trolley_PlayerInput _trolleyInput;
    [SerializeField] private NetworkObject _playerNetworkObject;
    private CharacterController _cc;

    //Trolley object ref
    [SerializeField] private GameObject _trolleyPrefab;
    [SerializeField] private Trolley_Script _trolleyScript;
    [SerializeField] private Trolley_Interact _interact;

    [SerializeField] private Transform _pilotPosition;


    private void Awake()
    {
       _movementScript = GetComponent<SimplePlayerMovement>();
        _trolleyInput = GetComponent<Trolley_PlayerInput>();
        _cc = GetComponent<CharacterController>();
        _playerNetworkObject = GetComponent<NetworkObject>();
    }


    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Pressed F");
            if(_trolleyScript != null)
            _trolleyScript.InteractRpc();
        }

        
    }



    public override void OnNetworkSpawn()
    {
        TransitionTo(PlayerState.Normal_State);
        UiManager.Instance.PlayerInitiateUI();
    }
    //State define whenever player is controlling the character or the trolley or else
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
    public void PublicStateSwitch(PlayerState next)
    {
        //This call twice locally and on everyone's side, the local is a quick fix to inconsistent local bugs, which sometime or most of the time pressing F doesn't do anything.(worst: 10 click to make it work once)
        TransitionTo(next);
        SetPlayerStateOnServerRpc(next);
    }

    [Rpc(SendTo.Everyone)]
    private void SetPlayerStateOnServerRpc(PlayerState next)
    {
        TransitionTo(next);
    }

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

    //In this state, player should snap on the Trolley/pilot position while controlling the trolley and is not moving on its own
    //ideally put the specific player prefab as a child of the trolley and undo it when the player is no longer controlling

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

    private void PilotOnPosition()
    {
        _playerPrefab.transform.position = _pilotPosition.position;
        _playerPrefab.transform.rotation = _pilotPosition.rotation;

    }

    //Character controller is causing a really weird player collision teleport bug on the server while driving the trolley, disabling it is a quick fix
    private void CC_State(bool newBool)
    {
        _cc.enabled = newBool; 
    }

    public void GetTrolleyRef(GameObject prefab, Trolley_Script script, Trolley_Interact interact, Transform position)
    {
        _trolleyPrefab = prefab ;
        _trolleyScript = script;
        _interact = interact;
        _pilotPosition = position;
    }

  

}