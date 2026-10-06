using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class AppleThrow : NetworkBehaviour
{
    [Header("Camera")]
    private TPSPlayerCamera _camera;


    [Header("Apple")]
    //Ref this under the throw hand skeleton, so the apple will always spawn from the hand and targeting crosshair (end point)
    [SerializeField] private Transform _throwPoint;
    [SerializeField] private GameObject _theApple;
    private Rigidbody _appleRB;
    private Vector3 _targetPoint => _camera.ReadRayEndPoint;
    private Vector3 _throwDirection;

    [Header("Throw Force")]
    //Apple are around 0.15 weight, throw force min/max look good around 2-8
    [SerializeField] private float _currentThrowForce;
    [SerializeField] private float _normalForce;
    [SerializeField] private AnimationCurve _chargeForceCurve;
    [SerializeField] private float _chargedForce;
    [SerializeField] private float _chargedTime = 0;
    //execute safety
   // private bool _isThrown;
    

    [Header("Input")]
    // no longer need them, until I need them
    // private bool _throwIsPressed;
     private bool _isAiming;
     private bool _isCharging;

    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _crosshair;

    [Header("Debug")]
    [SerializeField] private GameObject testThrow;

    private void Awake()
    {
        _camera = GetComponent<TPSPlayerCamera>();
    }

    public override void OnNetworkSpawn()
    {
        _crosshair = GameObject.Find("UI/Canvas (UI)/Gameplay UI/MainGameView/Crosshair_parent");
    }

    private void Update()
    {
        if (!IsOwner) return;
        //Player Input
        LeftButtonIsPressed();
        RightButtonIsPressed();
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;

        _camera.ReadisAimLazyVersion(_isAiming);
    }


    //Throw function
    [ServerRpc]
    public void ThrowServerRPC()
    {
        Throw(ThrowDirection(), _currentThrowForce);
    }

    private void Throw(Vector3 direction, float force )
    {
        // Throw direction can add a offset that is a little higher
        _appleRB.AddForce(direction * force, ForceMode.Impulse);

        ClearAppleComponent();
    }

    private void GetAppleComponent()
    { 
        if (_theApple != null)
        {
            _appleRB = _theApple.GetComponent<Rigidbody>();
        }
    }

    private void ClearAppleComponent()
    { 
        _theApple = null;
        _appleRB = null;
    }

    private Vector3 ThrowDirection()
    {
        return _throwDirection = (_targetPoint - _throwPoint.position).normalized;
    }

    private void ChargingAddThrowForce()
    {
        if (_isCharging)
        {
            _chargedTime += Time.deltaTime;
            _chargedForce = _chargeForceCurve.Evaluate(_chargedTime);

            _currentThrowForce = _chargedForce;
        }
    }

    private void ResetThrowForce()
    {
        _currentThrowForce = _normalForce;
        _chargedTime = 0;
    }    

    //Player key Input
    private void LeftButtonIsPressed()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            _isCharging = true;
            ChargingAddThrowForce();
        }
        else
        {
            _isCharging = false;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            //_throwIsPressed = true;


            //Short of time, animator is here for now and let animation event handle the throw
            _animator.SetTrigger("Throw");
            //Reset time and throw force
            ChargingAddThrowForce();

        }
        else
        {
           // _throwIsPressed = false;
        }
    }

    private void RightButtonIsPressed()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _isAiming = true;
            _crosshair.gameObject.SetActive(true);
        }
        else 
        {
            _isAiming = false;
            _crosshair.gameObject.SetActive(false);
        }
    }
    

    [ContextMenu("Debug: Generate Test Apple")]
    public void Generate_TestApple()
    {
        //This Function is the entire process of Throwing an Apple on Local, More modification require for Network:
        //1.Generate apple from Debug > Replace this part with actual pulling out apple from inventory.
       _theApple = Instantiate(testThrow,_throwPoint.position, _throwPoint.rotation);

        //Get the require information to throw the apple and set Kinematic to false, since it is true by default after generating and the apple will stay on air.
        GetAppleComponent();
        _appleRB.isKinematic = false;

        //2.Throw apple with current direction and force.
        Throw(ThrowDirection(),_currentThrowForce);

        //3.Reset parameters before throwing next apple.
        ResetThrowForce();
    }

    [ContextMenu("Debug: Test Throw")]

    public void TestThrow() => Throw(ThrowDirection(), _currentThrowForce);
}
