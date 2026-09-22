using System.Transactions;
using UnityEngine;

public class Trolley_Script : MonoBehaviour
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
    [SerializeField] private bool _isControlling;
    [SerializeField] private WheelCollider _wheel_front, _wheel_back_R, _wheel_back_L;

    //Input
    private Rigidbody rb;
    private float _horizontalInput, _verticalInput;
    



    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {

        InputSimplified();
        
    }
    private void FixedUpdate()
    {
        CartSimulation();
    }

    private void CartSimulation()
    {
        float motor = (Input.GetAxis("Vertical") * _motorSpeed);
        AddRBSpeed(_rbSpeed);
        _wheel_front.motorTorque = motor ;
        _wheel_back_L.motorTorque = motor ;
        _wheel_back_R.motorTorque = motor ;
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


    // This is a input testing function
    // Change into Camera based Front input for any direction if possible
    private void InputSimplified()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAddPushSpeed(_pushForce);
        }
    }
}
