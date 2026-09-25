using System.Transactions;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Trolley_Script : NetworkBehaviour
{


    //Movement Variables
    [Header("Movement")]
    [SerializeField] private float _frontWheelMultiplier = 0.6f;
    [SerializeField] private float _backWheelMultiplier = 1.5f;
    [SerializeField] private float _motorSpeed = 50f;
    [SerializeField] private float _rbSpeed = 50f;
    [SerializeField] private float _pushForce = 100f;
    [SerializeField] private float _rotationSteer = 20f;

    //Bools
    [SerializeField] private bool _isControlling;
    [SerializeField] private bool _F_button_CD = true;
    [SerializeField] private float _F_button_CT = 1f;

    //Object Reference
    [Header("Object Settings")]
    [SerializeField] private GameObject _pilot;
    [SerializeField] private ulong? _pilotNetID;
    [SerializeField] private Trolley_PlayerInput _pilotInput;
    [SerializeField] private Player_State _pilotState;
    [SerializeField] private WheelCollider _wheel_front, _wheel_back_R, _wheel_back_L;
    private Rigidbody rb;
    [SerializeField] private GameObject _interactionBox;
    [SerializeField] private GameObject _pilotPosition;


    //Player Input (Adapt this into _pilot player sending input to this input)
    [SerializeField] private float _horizontalInput, _verticalInput;

    //Ref
    public bool isControl_m => _isControlling;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        _F_button_CD = true;
    }
    //public override void OnNetworkDespawn()

    private void Update()
    {
        if (!IsServer) return;

        if (_pilotInput != null) 
        {
            _horizontalInput = _pilotInput.readHorizon;
            _verticalInput = _pilotInput.readVertical;
            SendPlayerInputServerRPC(_pilotInput.readVertical, _pilotInput.readHorizon);
        }

        
         

    }
    private void FixedUpdate()
    {
        

        CartSimulation();
    }

    private void CartSimulation()
    {
        if (!_isControlling) return;

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

    private void TestAddPushSpeed(float addSpeed)
    {
        rb.AddForce(gameObject.transform.forward * addSpeed *1000 * Time.deltaTime,ForceMode.Impulse);
    }

    private void AddRBSpeed(float addSpeed)
    {
        rb.AddForce(gameObject.transform.forward * _verticalInput * addSpeed * 1000 * Time.deltaTime, ForceMode.Force);
    }

    public void QuitPilot()
    {
      Debug.Log("Pressed F to Exit");
      PlayerInteractExit();
      _pilot = null;
      _pilotNetID = null;
      _pilotInput = null;
      _pilotState = null;
        
        
    }


    [ServerRpc]
    public void SendPlayerInputServerRPC(float verticalInput,float horizontalInput, ServerRpcParams rpcParams = default)
    {
        ulong detectedPilot = rpcParams.Receive.SenderClientId;

        if (_pilotNetID == detectedPilot)
        {
            _verticalInput = verticalInput;
            _horizontalInput = horizontalInput;
        }

    }

    //Pilot is the main player that is currently controlling the Trolley 

    //This only set who is controlling, don't put this on Update(), use unity event when Interact
    //Enable and Disable Interact when the player is there
    public void PlayerInteractEnter(GameObject getPilot)
    {
        if (_F_button_CD == true) 
        {
            Debug.Log("Pressed F to enter");
            FButtonCooldown();
            //Get pilot info to Trolley script and pilot get the seat
            GetPilotInfo(getPilot);
            _pilotState.GetTrolleyPilotPosition(_pilotPosition);

            if (_pilotState.readCurrentState == Player_State.PlayerState.Normal_State)
            {
             _interactionBox.SetActive(false);
             _pilotState.PublicStateSwitch();
             _isControlling = true;
            }
        }
    }

    private void PlayerInteractExit()
    {
        if (_F_button_CD == true) 
        {
            FButtonCooldown();

            _pilotState.PublicStateSwitch();
            _isControlling = false;
            _interactionBox.SetActive(true);
        }

    }

    private async void FButtonCooldown()
    { 
        _F_button_CD = false;
        await DelayActiveBox(_F_button_CT);
        _F_button_CD = true;
    }
    private async Task DelayActiveBox(float seconds)
    {
        await Awaitable.WaitForSecondsAsync(seconds);
    }

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

            if (_pilotInput != null)
            {
                _pilotInput.GetTrolleyScript(this);
            }
        }
    }

    // This is a input testing function for SinglePlayer
    // Change into Camera based Forward input for any direction if time allows, steerAngle would become (rotationSteer * Input ) + CameraCenterAdjustedAngle ??

    private void TestInputSimplified()
    {
        if (!_isControlling) return;

        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");

        //This will change into a push function for all player that isn't the pilot, to help pushing depend on the direction
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAddPushSpeed(_pushForce);
        }
    }

}
