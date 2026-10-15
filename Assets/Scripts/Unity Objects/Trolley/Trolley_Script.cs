using System.Transactions;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// the main script managing how the trolley should behave
/// </summary>
public class Trolley_Script : NetworkBehaviour
{

    //Movement Variables
    [Header("Movement")]
    // for motor stuff
    [SerializeField] private float _frontWheelMultiplier = 0.6f;

    // for motor stuff
    [SerializeField] private float _backWheelMultiplier = 1.5f;

    // the speed at which the motor runs
    [SerializeField] private float _motorSpeed = 50f;

    // rigid body speed
    [SerializeField] private float _rbSpeed = 50f;

    // how much force to put on that
    [SerializeField] private float _pushForce = 100f;

    // how much we can steer
    [SerializeField] private float _rotationSteer = 20f;

    //Bools
    // if the F button is on cooldown
    [SerializeField] private bool _F_button_CD = true;

    // for how long is the F button on cooldown
    [SerializeField] private float _F_button_CT = 1f;

    //Object Reference
    [Header("Player Settings")]
    // the GameObject representing the character driving the trolley
    [SerializeField] private GameObject _pilot;

    // identity of the pilot on the server
    [SerializeField] private ulong? _pilotNetID;

    // the input script to interface with the pilot
    [SerializeField] private Trolley_PlayerInput _pilotInput;

    // the player state of the pilot
    [SerializeField] private Player_State _pilotState;

    [Header("Trolley Settings")]
    // the prefab of the trolley
    [SerializeField]private GameObject _trolleyPrefab;

    // colliders for the trolley's wheels
    [SerializeField] private WheelCollider _wheel_front, _wheel_back_R, _wheel_back_L;

    // the trolley's rigid body
    private Rigidbody rb;

    // where the interraction with the trolley can happen
    [SerializeField] private GameObject _interactionBox;
    


    //Player Input (Adapt this into _pilot player sending input to this input)
    [SerializeField] private float _horizontalInput, _verticalInput;

    //Online Variable
    // if the trolley is being controlled or not right now
    [SerializeField] private NetworkVariable<bool> _isControlling = new NetworkVariable<bool>(false);


    /// <summary>
    /// Innitialize on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// Initialize components
    /// </summary>
    private void Innit()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Setup when the trolley appears on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _F_button_CD = true;

        if (IsServer) _isControlling.Value = false;

        InteractionBoxState(_isControlling.Value);
        _isControlling.OnValueChanged += OnInteractChanged;
        

    }

    /// <summary>
    /// unsubscribe if the trolley leaves the network
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _isControlling.OnValueChanged -= OnInteractChanged;
    }

    /// <summary>
    /// Handles the physics in the physics loop
    /// </summary>
    private void FixedUpdate()
    {
        CartSimulation();
    }

    /// <summary>
    /// Logic to drive the trolley
    /// </summary>
    private void CartSimulation()
    {
        //Read input
        float motor = (_verticalInput * _motorSpeed);

        //Hybrid adding speed with Rigidbody and Motor, to adapt complex terrain
        AddRBSpeed(_rbSpeed);
        _wheel_front.motorTorque = motor * _frontWheelMultiplier;
        _wheel_back_L.motorTorque = motor * _backWheelMultiplier;
        _wheel_back_R.motorTorque = motor * _backWheelMultiplier;

        // steer left or right rotation, front wheel only
        _wheel_front.steerAngle = _rotationSteer * _horizontalInput;

        
    }

    /// <summary>
    /// add force to the rigid body
    /// </summary>
    /// <param name="addSpeed">how much force to add</param>
    private void AddRBSpeed(float addSpeed)
    {
        rb.AddForce(gameObject.transform.forward * _verticalInput * addSpeed * 1000 * Time.deltaTime, ForceMode.Force);
    }

    /// <summary>
    /// sends player inputs to the server
    /// </summary>
    /// <param name="verticalInput">player vertical inputs</param>
    /// <param name="horizontalInput">player horizontal inputs</param>
    /// <param name="rpcParams">we keep thos default</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendPlayerInputRpc(float verticalInput,float horizontalInput,RpcParams rpcParams = default)
    {
        ulong newPilotId = rpcParams.Receive.SenderClientId;

        if (_pilotNetID != newPilotId) return;

        _verticalInput = verticalInput;
        _horizontalInput = horizontalInput;
    }

    /// <summary>
    /// send the pilot's info to the server
    /// </summary>
    /// <param name="rpcParams">we don't use those</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void InteractRpc(RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client)) return;

        GameObject getPilot = client.PlayerObject.gameObject;

        PlayerInteract(getPilot);
    }

    /// <summary>
    /// Handles the player trying to interract with the trolley
    /// </summary>
    /// <param name="getPilot">the player trying to interract with the trolley</param>
    public void PlayerInteract(GameObject getPilot)
    {
        if (_F_button_CD == true) 
        {
            FButtonCooldown();

            //Get pilot info to Trolley script and pilot get the seat
            GetPilotInfo(getPilot);
            

            if (_pilotState.readCurrentState == PlayerState.Normal_State)
            {
                Debug.Log("Pressed F to enter");

                _isControlling.Value = true;

                _pilotState.PublicStateSwitch(PlayerState.Trolley_State);
           
            }
            else 
            {
                Debug.Log("Pressed F to Exit");
                _pilotState.PublicStateSwitch(PlayerState.Normal_State);

                _isControlling.Value = false;
                
                //Clear information
                _pilot = null;
                _pilotNetID = null;
                _pilotInput = null;
                _pilotState = null;
            }


        }
    }

    /// <summary>
    /// Handles the F button being put in cooldown
    /// </summary>
    private async void FButtonCooldown()
    { 
        _F_button_CD = false;
        await DelayActiveBox(_F_button_CT);
        _F_button_CD = true;
    }

    /// <summary>
    /// wait
    /// </summary>
    /// <param name="sec">how long for in seconds</param>
    /// <returns></returns>
    private async Task DelayActiveBox(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }

    /// <summary>
    /// retrieve the information related to the trolley's pilot
    /// </summary>
    /// <param name="getPilot">the trolley's pilot</param>
    public void GetPilotInfo(GameObject getPilot)
    { 
        _pilot = getPilot;

      if ( _pilot != null)
        {
            _pilotInput = _pilot.GetComponent<Trolley_PlayerInput>();
            _pilotState = _pilot.GetComponent<Player_State>();
            if (_pilot.TryGetComponent<NetworkObject>(out NetworkObject id))
            {
                _pilotNetID = id.OwnerClientId;

                Debug.Log("Show your Pilot ID: " + _pilotNetID);
            }
            else 
            {
                Debug.Log("Pilot Identification failed!!! Deploy Programmer A and execute Order 67");
            }
        }
    }


    //Sync the Interaction box to all client

    /// <summary>
    /// I'm not sure why we have a reverser here
    /// </summary>
    /// <param name="newBool">the state we don't want for the interraction box?</param>
    private void InteractionBoxState(bool newBool)
    {
        _interactionBox.SetActive(!newBool);
    }

    /// <summary>
    /// toggle the interraction box
    /// </summary>
    /// <param name="oldBool">if it was on or off</param>
    /// <param name="newBool">if we want it on or off</param>
    private void OnInteractChanged(bool oldBool, bool newBool)
    {
        InteractionBoxState(newBool);
    }

    //Debug

    [ContextMenu("Debug: IsControlling On")]
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DebugBoolOnServerRPC() 
    {

        _isControlling.Value = true;
    }

    [ContextMenu("Debug: IsControlling Off")]
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DebugBoolOffServerRPC() 
    {

        _isControlling.Value = false;
    } 

}
