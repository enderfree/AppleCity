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

    [SerializeField] private bool _isAiming;


    [Header("Animator")]
    [SerializeField] private Animator _animator;


}
