using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player camera through the network
/// </summary>
public class TPSPlayerCamera : NetworkBehaviour
{
    [Header("Camera Move")]
    // how reactive is the camera
    [SerializeField] private float sensitivity = 0.1f;

    // camerat offset
    [SerializeField] private Vector3 _offset;

    // how faror close the camera can get
    [SerializeField] private float _cameraUpLimit = 45f, _cameraDownLimit = -40f;

    // where the camera points
    [SerializeField] private Transform _cameraPoint;
    
    // handles camera angle
    private float _yaw;

    // handles camera width
    private float _pitch;

    // offset for the camera smoothing
    [SerializeField] private Vector3 _smoothOffset;

    // how fast the smoothing happens
    [SerializeField] private float _smoothSpeed = 10f;

    //Default Mode
    [Header("Default")]
    // camera's default distance from the player
    [SerializeField] private float _defaultDistance = -2.1f;

    // camera height
    [SerializeField] private float _defaultHeight = 1.5f;

    // which side does the camera points to by dafault
    [SerializeField] private float _defaultSide = 0.5f;

    //Aiming Mode
    [Header("Aim")]
    // distance from the player while in aiming mode
    [SerializeField] private float _aimDistance = -1.4f;

    // camera height while aiming
    [SerializeField] private float _aimHeight = 1.8f;

    // camera side while aiming
    [SerializeField] private float _aimSide = 0.5f;

    // if camera is in aiming mode
    [SerializeField] private bool _isAiming;

    [Header("Reference")]
    // the camera object
    [SerializeField] private Camera _playerCamera;

    // the movement logic
    private SimplePlayerMovement _movement;

    [Header("TPS Raycast")]
    // where the ray should end
    [SerializeField] private Vector3 _rayEndPoint;

    // where the ray should start
    private Transform _rayStartPoint;

    /// <summary>
    /// public accessor for the _rayEndPoint
    /// </summary>
    public Vector3 ReadRayEndPoint => _rayEndPoint;

    // how far is the ray allowed to go
    [SerializeField] private float _maxDistance = 50f;

    // on which layer is the ray doing its detection
    [SerializeField] private LayerMask _layer;


    /// <summary>
    /// set a few things when the camera appears on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _playerCamera = Camera.main;
        _movement = GetComponent<SimplePlayerMovement>();
        _playerCamera.transform.SetParent(null);
        _yaw = transform.eulerAngles.y;
        
    }

    /// <summary>
    /// Update the camera
    /// </summary>
    private void LateUpdate()
    {
        if (!IsOwner) return;

        UpdateCameraPosition();

        NormalCamera();
        CameraModeSwitch();
        RaycastThrowDirection(_rayStartPoint.position, _rayStartPoint.forward);
    }

    /// <summary>
    /// handles inputs
    /// </summary>
    private void NormalCamera()
    {
        //Mouse Input Value
        Vector2 mouse = Mouse.current.delta.ReadValue();

        _yaw += mouse.x * sensitivity;
        _pitch -= mouse.y * sensitivity;
        _pitch =  Mathf.Clamp( _pitch, _cameraDownLimit, _cameraUpLimit);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);


        //Camera update Position and Rotation
        _playerCamera.transform.position = _cameraPoint.transform.position + rotation * _smoothOffset;
        _playerCamera.transform.rotation = rotation;
    }

    /// <summary>
    /// allow to go in and out of aiming mode
    /// </summary>
    private void CameraModeSwitch()
    {
        _offset = _isAiming ? new Vector3(_aimSide, _aimHeight, _aimDistance) : new Vector3(_defaultSide, _defaultHeight, _defaultDistance);
        _smoothOffset = Vector3.MoveTowards(_smoothOffset, _offset, Time.deltaTime * _smoothSpeed);
    }


    /// <summary>
    /// Raycast the point for aiming
    /// </summary>
    /// <param name="origin">where the raycast is starting from</param>
    /// <param name="direction">where the raycast is going</param>
    /// <returns>where the raycast stopped</returns>
    private Vector3 RaycastThrowDirection(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxDistance, _layer))
        {
            _rayEndPoint = hit.point;

            Debug.DrawLine(origin, hit.point, Color.red);
        }
        else
        {
            Ray noHit = new Ray(origin, direction);
            _rayEndPoint = noHit.GetPoint(_maxDistance);

            Debug.DrawRay(origin, direction * _maxDistance, Color.red);
        }
            

        return _rayEndPoint;
    }

    /// <summary>
    /// moves the camera
    /// </summary>
    private void UpdateCameraPosition()
    {
        _rayStartPoint = _playerCamera.transform;
    }

    public void ReadisAimLazyVersion(bool readBool)
    { 
     //I wonder what are the best way to pass variable around without too much referencing... but now I am running out of time

     _isAiming = readBool;
    }
}