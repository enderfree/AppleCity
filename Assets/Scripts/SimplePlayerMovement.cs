using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerMovement : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float targetSpeed = 4f;

    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 0.7f;

    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.5f;

    [SerializeField] private float targetRange = 15f;

    [SerializeField] private Animator animator;

    private CharacterController controller;

    private float verticalSpeed;
    private float slideTimer;

    private Vector3 slideDirection;

    public Transform Target { get; private set; }

    public NetworkVariable<bool> IsCrouching =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public NetworkVariable<bool> IsSliding =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public NetworkVariable<bool> IsTargeting =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        bool isLanding =
            animator.GetCurrentAnimatorStateInfo(0).IsName("Land") ||
            animator.GetNextAnimatorStateInfo(0).IsName("Land");

        // Targeting
        if (Keyboard.current.tabKey.wasPressedThisFrame &&
            !isLanding &&
            !IsSliding.Value)
        {
            if (IsTargeting.Value)
                StopTargeting();
            else
                FindTarget();
        }

        if (IsTargeting.Value && Target == null)
            StopTargeting();

        Vector2 input = Vector2.zero;

        if (!isLanding && !IsSliding.Value)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Vector3 movement;

        // Targeting movement
        if (IsTargeting.Value && Target != null)
        {
            Vector3 forward =
                Target.position - transform.position;

            forward.y = 0f;
            forward.Normalize();

            Vector3 right =
                Vector3.Cross(Vector3.up, forward);

            movement =
                forward * input.y +
                right * input.x;

            // Always face target
            if (forward != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(forward);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        turnSpeed * Time.deltaTime
                    );
            }
        }

        // Normal movement
        else
        {
            Vector3 forward =
                Camera.main.transform.forward;

            Vector3 right =
                Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement =
                forward * input.y +
                right * input.x;

            if (movement != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        movement.normalized
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        turnSpeed * Time.deltaTime
                    );
            }
        }

        bool isRunning =
            !IsTargeting.Value &&
            !IsCrouching.Value &&
            Keyboard.current.leftShiftKey.isPressed &&
            movement != Vector3.zero;

        // Crouch / Slide
        if (Keyboard.current.cKey.wasPressedThisFrame &&
            controller.isGrounded &&
            !isLanding &&
            !IsSliding.Value)
        {
            // Slide only while running normally
            if (isRunning)
            {
                IsCrouching.Value = true;
                IsSliding.Value = true;

                slideDirection = movement.normalized;
                slideTimer = slideDuration;
            }
            else
            {
                // Works during normal movement AND targeting
                IsCrouching.Value =
                    !IsCrouching.Value;
            }
        }

        float speed;

        if (IsSliding.Value)
        {
            movement = slideDirection;
            speed = slideSpeed;

            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
            {
                IsSliding.Value = false;
            }
        }
        else if (IsCrouching.Value)
        {
            speed = crouchSpeed;
        }
        else if (IsTargeting.Value)
        {
            speed = targetSpeed;
        }
        else if (isRunning)
        {
            speed = runSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        // Gravity / Jump
        if (controller.isGrounded)
        {
            verticalSpeed = -2f;

            if (!isLanding &&
                !IsCrouching.Value &&
                !IsSliding.Value &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalSpeed =
                    Mathf.Sqrt(
                        jumpHeight *
                        -2f *
                        Physics.gravity.y
                    );
            }
        }
        else
        {
            verticalSpeed +=
                Physics.gravity.y *
                Time.deltaTime;
        }

        movement =
            movement.normalized * speed;

        movement.y = verticalSpeed;

        controller.Move(
            movement * Time.deltaTime
        );
    }

    private void FindTarget()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        float closestDistance = targetRange;
        Transform closestTarget = null;

        foreach (GameObject enemy in enemies)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = enemy.transform;
            }
        }

        if (closestTarget == null)
            return;

        Target = closestTarget;

        IsTargeting.Value = true;
    }

    private void StopTargeting()
    {
        Target = null;
        IsTargeting.Value = false;
    }
}