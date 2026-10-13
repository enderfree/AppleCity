using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class AppleThrow : NetworkBehaviour
{
    //Camera
    private TPSPlayerCamera _camera;
    private Vector3 _targetPoint => _camera.ReadRayEndPoint;
    private Vector3 _throwDirection;

    [Header("Apple")]
    //Ref this under the throw hand skeleton, so the apple will always spawn from the hand and targeting crosshair (end point)
    [SerializeField] private Transform _throwPoint;
    [SerializeField] private NetworkObject _netApple;
   // [SerializeField] private NetworkVariable<NetworkObject> _netApples = new NetworkVariable<NetworkObject>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Rigidbody _appleRB;


    [Header("Apple Interact")]
    [SerializeField] private ApplePIckup _pick;
    [SerializeField] private NetworkVariable<bool> _haveApple = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Throw Force")]
    //Apple are around 0.15 weight, throw force min/max look good around 2-8
    [SerializeField] private float _currentThrowForce;
    [SerializeField] private float _normalForce;
    [SerializeField] private AnimationCurve _chargeForceCurve;
     private float _chargedForce;
     private float _chargedTime = 0;

    [Header("Input")]
    private bool _isAiming;
    private bool _isCharging;

    //ref
    private Animator _animator;
    private GameObject _crosshair;


    public override void OnNetworkSpawn()
    {
        _camera = GetComponent<TPSPlayerCamera>();
        _animator = GetComponent<Animator>();
        _crosshair = GameObject.Find("UI/Canvas (UI)/Gameplay UI/MainGameView/Crosshair_parent");
    }

    private void Update()
    {
         

        if (!IsOwner) return;
        //Player Input
        LeftButtonIsPressed();
        RightButtonIsPressed();
        InteractWithApple();

    }
    private void LateUpdate()
    {
        SnapAppleToHand();
        if (!IsOwner) return;

        _camera.ReadisAimLazyVersion(_isAiming);
        

    }

    public void AnimationThrow()
    {
            ThrowServerRpc(ThrowDirection(), _currentThrowForce);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowServerRpc(Vector3 direction, float force, RpcParams rpcParams = default)
    {
        //safety check just in case the apple is not synced online, if not, weird nullref or nothing happens happen.
        if (_netApple == null || !_netApple.IsSpawned) return;

        Throw(direction, force);
    }

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
        _haveApple.Value = false;
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
            //Short of time, animator is here for now and let animation event handle the throw
            _animator.SetTrigger("Throw");
            if (_haveApple.Value) 
            {
               
            }
            //Reset time and throw force
            ChargingAddThrowForce();
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

    //Apple Function

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

    [ServerRpc]
    private void PickUpAppleServerRPC()
    {
        PickUpAnApple();
    }

    private void PickUpAnApple()
    {
        if (_pick.AppleList.Count > 0 && !_haveApple.Value)
        {
            _netApple = _pick.AppleList[0];
            
            GetAppleComponent();
            
        }
    }
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

    private void AppleRBPhysicBool(bool boolean)
    {
        //Kinematic and disable detectCollision to avoid apple pushing the player away
        _appleRB.isKinematic = !boolean;
        _appleRB.detectCollisions = boolean;
    }

    private void SnapAppleToHand()
    {
        if (_netApple != null)
        {
            _netApple.gameObject.transform.position = _throwPoint.position;
        }
    }



    private void ClearAppleComponent()
    {
        _appleRB = null;
        _netApple = null;
    }




}
