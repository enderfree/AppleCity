using UnityEngine;

/// <summary>
/// This class handles player animations
/// </summary>
public class SimplePlayerAnimation : MonoBehaviour
{
    // player animator
    [SerializeField] private Animator _animator;

    // player character controller
    private CharacterController _controller;

    // the class that handles movements
    private SimplePlayerMovement _playerMovement;

    // character speed
    private float _speed;

    // if the character is jumping
    private bool _isJump;

    // not sure, was made by Josh and while Jun uses them, our understanding of this field is vague
    [SerializeField] private float _speedDeltaTimeMultiplier = 1f;

    // where the player last was
    private Vector3 _lastPosition;

    /// <summary>
    /// Initialize on Start()
    /// </summary>
    private void Start()
    {
        Innit();
    }

    /// <summary>
    /// Initialize a few variables
    /// </summary>
    private void Innit()
    {
        _controller =
            GetComponent<CharacterController>();

        _playerMovement =
            GetComponent<SimplePlayerMovement>();

        _lastPosition =
            transform.position;
    }

    /// <summary>
    /// Handle animations on LateUpdate
    /// </summary>
    private void LateUpdate()
    {
        HandleAnimation();
    }

    /// <summary>
    /// Handles the animations
    /// </summary>
    private void HandleAnimation()
    {
        Vector3 movement =
            transform.position -
            _lastPosition;

        Vector3 horizontalMovement =
            new Vector3(
                movement.x,
                0f,
                movement.z
            );

        _speed = Mathf.MoveTowards(_speed, horizontalMovement.magnitude / Time.deltaTime, Time.deltaTime * _speedDeltaTimeMultiplier);


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

        _animator.SetFloat(
            "Speed",
            _speed
        );

        _animator.SetFloat(
            "VerticalSpeed",
            verticalSpeed
        );

        _animator.SetFloat(
            "MoveX",
            localMovement.x,
            0.1f,
            Time.deltaTime
        );

        _animator.SetFloat(
            "MoveY",
            localMovement.z,
            0.1f,
            Time.deltaTime
        );

        _animator.SetBool(
            "IsGrounded",
            _controller.isGrounded
        );

        _animator.SetBool(
            "IsCrouching",
            _playerMovement._isCrouching.Value
        );

        _animator.SetBool(
            "IsSliding",
            _playerMovement._isSliding.Value
        );

        _animator.SetBool(
            "IsTargeting",
            _playerMovement._isTargeting.Value
        );

        _lastPosition =
            transform.position;

        if (!_isJump && verticalSpeed >= 0.1 && !_controller.isGrounded)
        {
            //  animator.SetTrigger("Jump");
            _isJump = true;
        }

        if (_controller.isGrounded)
        {
            _isJump = false;
        }
    }
}