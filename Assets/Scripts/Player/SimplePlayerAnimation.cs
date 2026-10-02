using UnityEngine;

public class SimplePlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private SimplePlayerMovement playerMovement;

    private Vector3 lastPosition;

    private void Start()
    {
        controller =
            GetComponent<CharacterController>();

        playerMovement =
            GetComponent<SimplePlayerMovement>();

        lastPosition =
            transform.position;
    }

    private void Update()
    {
        Vector3 movement =
            transform.position -
            lastPosition;

        Vector3 horizontalMovement =
            new Vector3(
                movement.x,
                0f,
                movement.z
            );

        float speed =
            horizontalMovement.magnitude /
            Time.deltaTime;

        float verticalSpeed =
            movement.y /
            Time.deltaTime;

        Vector3 localMovement =
            Vector3.zero;

        if (horizontalMovement.magnitude > 0.001f)
        {
            localMovement =
                transform.InverseTransformDirection(
                    horizontalMovement.normalized
                );
        }

        animator.SetFloat(
            "Speed",
            speed
        );

        animator.SetFloat(
            "VerticalSpeed",
            verticalSpeed
        );

        animator.SetFloat(
            "MoveX",
            localMovement.x,
            0.1f,
            Time.deltaTime
        );

        animator.SetFloat(
            "MoveY",
            localMovement.z,
            0.1f,
            Time.deltaTime
        );

        animator.SetBool(
            "IsGrounded",
            controller.isGrounded
        );

        animator.SetBool(
            "IsCrouching",
            playerMovement.IsCrouching.Value
        );

        animator.SetBool(
            "IsSliding",
            playerMovement.IsSliding.Value
        );

        animator.SetBool(
            "IsTargeting",
            playerMovement.IsTargeting.Value
        );

        lastPosition =
            transform.position;
    }
}