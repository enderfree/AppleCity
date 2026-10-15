using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// the logic needed to throw an apple
/// </summary>
public class AppleThrow : NetworkBehaviour
{
    //Camera
    // the player camera
    private TPSPlayerCamera _camera;

    // the position in which to throw the apple
    private Vector3 _targetPoint => _camera.ReadRayEndPoint;

    // the force to be applied on the apple
    private Vector3 _throwDirection;

    [Header("Apple")]
    //Ref this under the throw hand skeleton, so the apple will always spawn from the hand and targeting crosshair (end point)
    [SerializeField] private Transform _throwPoint;

    // the apple from the server
    [SerializeField] private NetworkObject _netApple;

   // [SerializeField] private NetworkVariable<NetworkObject> _netApples = new NetworkVariable<NetworkObject>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // the rigid body of the apple
    private Rigidbody _appleRB;


    [Header("Apple Interact")]
    // the logic to pick up an apple
    [SerializeField] private ApplePIckup _pick;

    // do you have an apple?
    [SerializeField] private NetworkVariable<bool> _haveApple = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Throw Force")]
    //Apple are around 0.15 weight, throw force min/max look good around 2-8
    [SerializeField] private float _currentThrowForce;

    // the normal force to apply on the apple
    [SerializeField] private float _normalForce;

    // the throw curve apples follow
    [SerializeField] private AnimationCurve _chargeForceCurve;

    // how much force to apply depending on how long did the player charge
     private float _chargedForce;

    // how long did the player charge
     private float _chargedTime = 0;

    [Header("Input")]
    // is the player currently aiming at something
    private bool _isAiming;

    // is the player currently charging a throw
    private bool _isCharging;

    //ref
    // the player's animator
    private Animator _animator;

    // the crosshair object
    private GameObject _crosshair;

    /// <summary>
    /// assing things when they pop in the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _camera = GetComponent<TPSPlayerCamera>();
        _animator = GetComponent<Animator>();
        _crosshair = GameObject.Find("UI/Canvas (UI)/Gameplay UI/MainGameView/Crosshair_parent");
    }

    /// <summary>
    /// update the player about the throw
    /// </summary>
    private void Update()
    {
         

        if (!IsOwner) return;
        //Player Input
        LeftButtonIsPressed();
        RightButtonIsPressed();
        InteractWithApple();

    }

    /// <summary>
    /// update the animation and camera
    /// </summary>
    private void LateUpdate()
    {
        SnapAppleToHand();
        if (!IsOwner) return;

        _camera.ReadisAimLazyVersion(_isAiming);
        

    }

    /// <summary>
    /// throw the apple
    /// </summary>
    public void AnimationThrow()
    {
            ThrowServerRpc(ThrowDirection(), _currentThrowForce);
    }

    /// <summary>
    /// late throw in case of failure
    /// </summary>
    /// <param name="direction">where to shoot the apple</param>
    /// <param name="force">how strongly to shoot the apple</param>
    /// <param name="rpcParams">we're always using default</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowServerRpc(Vector3 direction, float force, RpcParams rpcParams = default)
    {
        //safety check just in case the apple is not synced online, if not, weird nullref or nothing happens happen.
        if (_netApple == null || !_netApple.IsSpawned) return;

        Throw(direction, force);
    }

    /// <summary>
    /// normal local throw
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    private void Throw(Vector3 direction, float force)
    {
        if (!IsServer) return;
        if ( _netApple == null) return;

        //Reset apple quaternion to make sure it is going to the right position
        _netApple.gameObject.transform.rotation = Quaternion.identity;
        AppleRBPhysicBool(true);
        _appleRB.AddForce(direction * force, ForceMode.Impulse);

        ClearAppleComponent();
        ResetThrowForce();
    }

    /// <summary>
    /// get the direction in which to throw the apple
    /// </summary>
    /// <returns>the direction in which to throw the apple</returns>
    private Vector3 ThrowDirection()
    {
        return _throwDirection = (_targetPoint - _throwPoint.position).normalized;
    }

    /// <summary>
    /// the charging logic
    /// </summary>
    private void ChargingAddThrowForce()
    {
        if (_isCharging)
        {
            _chargedTime += Time.deltaTime;
            _chargedForce = _chargeForceCurve.Evaluate(_chargedTime);

            _currentThrowForce = _chargedForce;
        }
    }

    /// <summary>
    /// remove the extra charge force so apples aren't thrown at an exponential pace
    /// </summary>
    private void ResetThrowForce()
    {
        _currentThrowForce = _normalForce;
        _chargedTime = 0;
        _haveApple.Value = false;
    }

    //Player key Input
    /// <summary>
    /// handles left button inputs (throw)
    /// </summary>
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
            //Short of time, animator is here for now and let animation event handle the throw
            _animator.SetTrigger("Throw");
            if (_haveApple.Value) 
            {
               
            }
            //Reset time and throw force
            ChargingAddThrowForce();
        }
    }

    /// <summary>
    /// handles right button inputs (aim)
    /// </summary>
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

    //Apple Function
    /// <summary>
    /// handles the picking up input
    /// </summary>
    private void InteractWithApple()
    {
        if (_pick.AppleList.Count > 0 && !_haveApple.Value)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                //PickUpAnApple();
                PickUpAppleServerRPC();
            }
        }
    }

    /// <summary>
    /// handles picking up apples through the server
    /// </summary>
    [ServerRpc]
    private void PickUpAppleServerRPC()
    {
        PickUpAnApple();
    }

    /// <summary>
    /// handles picking up apples in code
    /// </summary>
    private void PickUpAnApple()
    {
        if (_pick.AppleList.Count > 0 && !_haveApple.Value)
        {
            _netApple = _pick.AppleList[0];
            
            GetAppleComponent();
            
        }
    }

    /// <summary>
    /// get some components from the apple
    /// </summary>
    private void GetAppleComponent()
    {
        if (!IsServer) return;

        _haveApple.Value = true;

        if (_netApple != null)
        {
            _appleRB = _netApple.GetComponent<Rigidbody>();
            AppleRBPhysicBool(false);
        }
    }

    /// <summary>
    /// turns on or off the physics for the apple
    /// </summary>
    /// <param name="boolean">if you should turn it on or off</param>
    private void AppleRBPhysicBool(bool boolean)
    {
        //Kinematic and disable detectCollision to avoid apple pushing the player away
        _appleRB.isKinematic = !boolean;
        _appleRB.detectCollisions = boolean;
    }

    /// <summary>
    /// put an apple in hand when an apple is held
    /// </summary>
    private void SnapAppleToHand()
    {
        if (_netApple != null)
        {
            _netApple.gameObject.transform.position = _throwPoint.position;
        }
    }


    /// <summary>
    /// remove some component from the apple when needed
    /// </summary>
    private void ClearAppleComponent()
    {
        _appleRB = null;
        _netApple = null;
    }




}
