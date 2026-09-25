using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Player_State : NetworkBehaviour
{
    //State of the player
    public enum PlayerState {Normal_State, Trolley_State }
    [SerializeField] private PlayerState _currentState;
    public PlayerState readCurrentState => _currentState;

    //Player object ref
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private SimplePlayerMovement _movementScript;
    [SerializeField] private Trolley_PlayerInput _trolleyInput;

    //Trolley object ref
    [SerializeField] private GameObject _pilotPosition;

    private void Awake()
    {
       _movementScript = GetComponent<SimplePlayerMovement>();
        _trolleyInput = GetComponent<Trolley_PlayerInput>();

    }

    private void LateUpdate()
    {
        if (_currentState == PlayerState.Trolley_State  && _pilotPosition != null)
        {
            _playerPrefab.transform.position = _pilotPosition.transform.position;
        }
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
    public void PublicStateSwitch()
    {
        if (_currentState == PlayerState.Normal_State) TransitionTo(PlayerState.Trolley_State);
        else TransitionTo(PlayerState.Normal_State);
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
        }
        
    }

    //In this state, player should snap on the Trolley/pilot position while controlling the trolley and is not moving on its own
    //ideally put the specific player prefab as a child of the trolley and undo it when the player is no longer controlling

    //Network object cannot be child of a non network object... snap position for now
    private void PlayerTrolleyState() 
    {
        _movementScript.enabled = false;
        _trolleyInput.enabled = true;
        //Snap player to position and set its local position to 0,0,0


        if (_pilotPosition != null)
        {

            _playerPrefab.transform.position = _pilotPosition.transform.position;
            
           // _playerPrefab.transform.SetParent(_pilotPosition.transform);
           // _playerPrefab.transform.localPosition = Vector3.zero;
        }
    }

    public void GetTrolleyPilotPosition(GameObject position)
    {
        _pilotPosition = position;
    }



    //Debug list

    [ContextMenu("Debug: SwitchState")]
    public void Debug_StateSwitching() => PublicStateSwitch();
    [ContextMenu("Debug: Move")]
    public void Debug_DisableMovement() => _movementScript.enabled = false;
    [ContextMenu("Debug: Trolley")]
    public void Debug_DisableTrolleyInput() => _trolleyInput.enabled = true;



}