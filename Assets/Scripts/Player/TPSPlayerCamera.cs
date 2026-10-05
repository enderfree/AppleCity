using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class TPSPlayerCamera : NetworkBehaviour
{
    [Header("Camera Move")]
    [SerializeField] private float sensitivity = 0.1f;

    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
    [SerializeField] private float _cameraUpLimit = 45f, _cameraDownLimit = -40f;
    [SerializeField] private float _smoothSpeed = 10f;
    //Default Mode
    [Header("Default")]
    [SerializeField] private float _defaultDistance = -2.1f;
    [SerializeField] private float _defaultHeight = 1.5f;

    //Aiming Mode
    [Header("Aim")]
    [SerializeField] private float _targetDistance = 5f;
    [SerializeField] private float _targetHeight = 2f;
    

    private float yaw;
    private float pitch;

    [Header("Reference")]
    [SerializeField] private Camera playerCamera;
    private SimplePlayerMovement movement;

    [Header("TPS Raycast")]
     private Transform _rayStartPoint;
    [SerializeField] private Vector3 _rayEndPoint;
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


        //Camera Position and Rotation
        playerCamera.transform.position = transform.position + rotation * offset;
        playerCamera.transform.rotation = rotation;
    }

    private Vector3 RaycastThrowDirection(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxDistance, _layer))
        {
            Debug.DrawLine(origin, hit.point, Color.green);

            _rayEndPoint = hit.point;
        }
        else
        {
            Debug.DrawRay(origin, direction * _maxDistance, Color.red);

            Ray noHit = new Ray(origin, direction);
            _rayEndPoint = noHit.GetPoint(_maxDistance);
        }
            

        return _rayEndPoint;
    }

    private void UpdateCameraPosition()
    {
        _rayStartPoint = playerCamera.transform;
    }
}