using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerState { Normal_State, Trolley_State }
public class Player_State : NetworkBehaviour
{
    //State of the player
    
    [SerializeField] private PlayerState _currentState;
    public PlayerState readCurrentState => _currentState;

    //Player object ref
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private SimplePlayerMovement _movementScript;
    [SerializeField] private Trolley_PlayerInput _trolleyInput;
    private NetworkObject _playerNetworkObject;

    //Trolley object ref
    [SerializeField] private GameObject _trolleyPrefab;
    [SerializeField] private Trolley_Script _trolleyScript;
    [SerializeField] private Trolley_Interact _interact;

    [SerializeField] private Transform _pilotPosition;


    private void Awake()
    {
       _movementScript = GetComponent<SimplePlayerMovement>();
        _trolleyInput = GetComponent<Trolley_PlayerInput>();
        _playerNetworkObject = GetComponent<NetworkObject>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Pressed F");
            if(_trolleyScript != null)
            _trolleyScript.PlayerInteract(_playerPrefab);
        }

        InteractReset();
    }



    public override void OnNetworkSpawn()
    {
        TransitionTo(PlayerState.Normal_State);
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
        TransitionTo(next);
    }
    
    private void PlayerNormalState() 
    {
        _movementScript.enabled = true;
        _trolleyInput.enabled = false;
        //Undo Parenting when return to Normal State
        _playerPrefab.transform.SetParent(null);
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
        _movementScript.enabled = false;
        _trolleyInput.enabled = true;
        _trolleyInput.GetTrolleyScript(_trolleyScript);

        //Set the player as Trolley child and pilotPosition.
        if (_pilotPosition != null)
        {
            _playerPrefab.transform.SetParent(_trolleyPrefab.transform);
            _playerPrefab.transform.position = _pilotPosition.transform.position;
        }
    }

    public void GetTrolleyPilotPosition(Transform position)
    {
        _pilotPosition = position;
    }

    public void GetTrolleyRef(GameObject prefab, Trolley_Script script, Trolley_Interact interact)
    {
        _trolleyPrefab = prefab ;
        _trolleyScript = script;
        _interact = interact;
    }

    public void InteractReset()
    {
        if(_interact != null) {
            if (_interact._playerBool == false)
            {
              //  GetTrolleyRef(null, null, null);
            }
        }
        
    }

}