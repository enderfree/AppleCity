using System.Transactions;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

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

    //Object Variable
    [Header("Object Settings")]
    [SerializeField] private GameObject _pilot;
    [SerializeField] private ulong? _pilotNetID;
    [SerializeField] private Trolley_PlayerInput _playerInput;
    [SerializeField] private bool _isControlling;
    [SerializeField] private WheelCollider _wheel_front, _wheel_back_R, _wheel_back_L;
    private Rigidbody rb;
    [SerializeField] private BoxCollider _interactionArea;

    //Player Input (Adapt this into _pilot player sending input to this input)
    [SerializeField] private float _horizontalInput, _verticalInput;

    //Ref
    public bool isControl_m => _isControlling;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //public override void OnNetworkSpawn()
    //public override void OnNetworkDespawn()

    private void Update()
    {
        if (!IsServer) return;

        SendPlayerInputServerRPC(_playerInput.readVertical, _playerInput.readHorizon);


    }
    private void FixedUpdate()
    {
        if (!IsServer) return;

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

    [ServerRpc]
    private void SendPlayerInputServerRPC(float verticalInput,float horizontalInput, ServerRpcParams rpcParams = default)
    {
        ulong detectedPilot = rpcParams.Receive.SenderClientId;

        if (!_isControlling) return;
        if (_pilotNetID == detectedPilot)
        {
            _verticalInput = verticalInput;
            _horizontalInput = horizontalInput;
        }

    }

    //Pilot is the main player that is currently controlling the Trolley 

    //This only set who is controlling, don't put this on Update(), use unity event when Interact
    public void PlayerInteract()
    {

    }

    //Assign Control Ownership
    private void PilotControl()
    {

    }
    public virtual GameObject Pilot
    {
        get { return _pilot; }
        set {_pilot = value;}
    }
  
}
