using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class TPSPlayerCamera : NetworkBehaviour
{
    [Header("Camera Move")]
    [SerializeField] private float sensitivity = 0.1f;

    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _cameraUpLimit = 45f, _cameraDownLimit = -40f;
    [SerializeField] private Transform _cameraPoint;
    
    private float yaw;
    private float pitch;

    [SerializeField] private Vector3 _smoothOffset;
    [SerializeField] private float _smoothSpeed = 10f;

    //Default Mode
    [Header("Default")]
    [SerializeField] private float _defaultDistance = -2.1f;
    [SerializeField] private float _defaultHeight = 1.5f;
    [SerializeField] private float _defaultSide = 0.5f;

    //Aiming Mode
    [Header("Aim")]
    [SerializeField] private float _aimDistance = -1.4f;
    [SerializeField] private float _aimHeight = 1.8f;
    [SerializeField] private float _aimSide = 0.5f;
    [SerializeField] private bool _isAiming;

    [Header("Reference")]
    [SerializeField] private Camera playerCamera;
    private SimplePlayerMovement movement;

    [Header("TPS Raycast")]
    [SerializeField] private Vector3 _rayEndPoint;
    private Transform _rayStartPoint;
    public Vector3 ReadRayEndPoint => _rayEndPoint;
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private LayerMask _layer;



    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        playerCamera = Camera.main;
        movement = GetComponent<SimplePlayerMovement>();
        playerCamera.transform.SetParent(null);
        yaw = transform.eulerAngles.y;
        
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        UpdateCameraPosition();

        NormalCamera();
        CameraModeSwitch();
        RaycastThrowDirection(_rayStartPoint.position, _rayStartPoint.forward);
    }

    private void NormalCamera()
    {
        //Mouse Input Value
        Vector2 mouse = Mouse.current.delta.ReadValue();

        yaw += mouse.x * sensitivity;
        pitch -= mouse.y * sensitivity;
        pitch =  Mathf.Clamp( pitch, _cameraDownLimit, _cameraUpLimit);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);


        //Camera update Position and Rotation
        playerCamera.transform.position = _cameraPoint.transform.position + rotation * _smoothOffset;
        playerCamera.transform.rotation = rotation;
    }

    private void CameraModeSwitch()
    {
        _offset = _isAiming ? new Vector3(_aimSide, _aimHeight, _aimDistance) : new Vector3(_defaultSide, _defaultHeight, _defaultDistance);
        _smoothOffset = Vector3.MoveTowards(_smoothOffset, _offset, Time.deltaTime * _smoothSpeed);
    }


    //Raycast the point for aiming
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

    private void UpdateCameraPosition()
    {
        _rayStartPoint = playerCamera.transform;
    }

    public void ReadisAimLazyVersion(bool readBool)
    { 
     //I wonder what are the best way to pass variable around without too much referencing... but now I am running out of time

     _isAiming = readBool;
    }
}