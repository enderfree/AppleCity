using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPSPlayerCamera : NetworkBehaviour
{
    [Header("Camera Move")]
    [SerializeField] private float sensitivity = 0.1f;

    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);

    [SerializeField] private float targetDistance = 5f;
    [SerializeField] private float targetHeight = 2f;
    [SerializeField] private float smoothSpeed = 10f;

    private float yaw;
    private float pitch;

    [Header("Reference")]
    private Camera playerCamera;
    private SimplePlayerMovement movement;

    [Header("TPS Raycast")]
    private Vector3 _rayStartPoint;
    private Vector3 _rayEndPoint;




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

        NormalCamera();
    }

    private void NormalCamera()
    {
        //Mouse Input Value
        Vector2 mouse = Mouse.current.delta.ReadValue();

        yaw += mouse.x * sensitivity;
        pitch -= mouse.y * sensitivity;
        pitch =  Mathf.Clamp( pitch, -40f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);


        //Camera Position
        playerCamera.transform.position = transform.position + rotation * offset;
        playerCamera.transform.rotation = rotation;
    }

}