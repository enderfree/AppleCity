using Newtonsoft.Json.Bson;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class AppleThrow : NetworkBehaviour
{
    [Header("Camera")]
    private TPSPlayerCamera _camera;


    [Header("Apple")]

    [SerializeField] private GameObject _theApple;

    [Header("Throw Force")]
    [SerializeField] private float _currentThrowForce;
    [SerializeField] private float _normalForce;
    [SerializeField] private float _chargeForceRate;
    [SerializeField] private float _chargedForce;
    [SerializeField] private bool _isCharging;

    [Header("Input")]
    [SerializeField] private bool _throwIsPressed;
    [SerializeField] private bool _isAiming;


    [Header("Animator")]
    [SerializeField] private Animator _animator;

    private void Update()
    {
        LeftButtonIsPressed();
        RightButtonIsPressed();

    }

    private void LeftButtonIsPressed()
    {
        if (Input.GetKey(KeyCode.Mouse0) && _isAiming)
        {
            _isCharging = true;
        }
        else
        {
            _isCharging = false;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _throwIsPressed = true;
        }
        else
        {
            _throwIsPressed = false;
        }
    }

    private void RightButtonIsPressed()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _isAiming = true;
        }
        else 
        {
            _isAiming = false;
        }
    }
}
