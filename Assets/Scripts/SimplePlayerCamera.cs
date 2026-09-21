using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerCamera : NetworkBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;

    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 2f, -4f);

    [SerializeField] private float targetDistance = 5f;
    [SerializeField] private float targetHeight = 2f;
    [SerializeField] private float smoothSpeed = 10f;

    private Camera playerCamera;
    private SimplePlayerMovement movement;

    private float yaw;
    private float pitch;

    private bool wasTargeting;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        playerCamera = Camera.main;

        movement =
            GetComponent<SimplePlayerMovement>();

        playerCamera.transform.SetParent(null);

        yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        bool targeting =
            movement.IsTargeting.Value &&
            movement.Target != null;

        if (targeting)
        {
            TargetCamera();

            wasTargeting = true;
            return;
        }

        if (wasTargeting)
        {
            yaw =
                playerCamera.transform.eulerAngles.y;

            pitch =
                playerCamera.transform.eulerAngles.x;

            if (pitch > 180f)
                pitch -= 360f;

            wasTargeting = false;
        }

        NormalCamera();
    }

    private void NormalCamera()
    {
        Vector2 mouse =
            Mouse.current.delta.ReadValue();

        yaw +=
            mouse.x * sensitivity;

        pitch -=
            mouse.y * sensitivity;

        pitch =
            Mathf.Clamp(
                pitch,
                -40f,
                60f
            );

        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        playerCamera.transform.position =
            transform.position +
            rotation * offset;

        playerCamera.transform.rotation =
            rotation;
    }

    private void TargetCamera()
    {
        Vector3 targetPosition =
            movement.Target.position +
            Vector3.up;

        Vector3 direction =
            targetPosition -
            transform.position;

        direction.y = 0f;
        direction.Normalize();

        Vector3 wantedPosition =
            transform.position -
            direction * targetDistance +
            Vector3.up * targetHeight;

        Vector3 lookPoint =
            (
                transform.position +
                targetPosition
            ) / 2f;

        lookPoint += Vector3.up * 0.5f;

        playerCamera.transform.position =
            Vector3.Lerp(
                playerCamera.transform.position,
                wantedPosition,
                smoothSpeed * Time.deltaTime
            );

        Quaternion wantedRotation =
            Quaternion.LookRotation(
                lookPoint -
                playerCamera.transform.position
            );

        playerCamera.transform.rotation =
            Quaternion.Slerp(
                playerCamera.transform.rotation,
                wantedRotation,
                smoothSpeed * Time.deltaTime
            );
    }
}