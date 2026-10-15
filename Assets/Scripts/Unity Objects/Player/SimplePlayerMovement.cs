using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

/// <summary>
/// The logic that handles player movement
/// </summary>
public class SimplePlayerMovement : NetworkBehaviour
{
    // the spped at which the player walks
    [SerializeField] private float _walkSpeed = 4f;

    // the speed at which the player runs
    [SerializeField] private float _runSpeed = 8f;

    // how fast the player moves when crouching
    [SerializeField] private float _crouchSpeed = 2.5f;

    // the speed at which the player goes while aiming
    [SerializeField] private float _targetSpeed = 4f;

    // the speed at which the player slides
    [SerializeField] private float _slideSpeed = 10f;

    // how long does a slide last
    [SerializeField] private float _slideDuration = 0.7f;

    // the speed at which the player can turn
    [SerializeField] private float _turnSpeed = 10f;

    // how high the player can jump
    [SerializeField] private float _jumpHeight = 1.5f;

    // how much push back do you get from gravity
    [SerializeField] private float _gravityMultiplier = 1.2f;

    // if the player can jump right now
    private bool _canJump = true;

    // if the ^player can double jump right now
    private bool _canDoubleJump = true;
    
    // the player's animator
    [SerializeField] private Animator _animator;

    // the player's character controller
    private CharacterController _controller;

    // the current going up or down speed
    private float _verticalSpeed;

    // timer for the slide
    private float _slideTimer;

    // in which direction are you sliding
    private Vector3 _slideDirection;

    /// <summary>
    /// public accessor for the target
    /// </summary>
    public Transform Target { get; private set; }

    // is the player currently crouching
    public NetworkVariable<bool> _isCrouching =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
    
    // is the player currently sliding
    public NetworkVariable<bool> _isSliding =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    // is the player currently aiming
    public NetworkVariable<bool> _isTargeting =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    /// <summary>
    /// Innitialize on Awake();
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// get some components
    /// </summary>
    private void Innit()
    {
        _controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Josh put input detection and movement together in Update() and I don't have the time to untangel that, 
    /// but I've moved it to FixedUpdate in the hope it helps with performance
    /// </summary>
    private void FixedUpdate()
    {
        HandleInputs();
    }

    /// <summary>
    /// Manages most movement options... I say most because we don't have a centralized place for inputs
    /// because that's how Josh does things and we couldn't just hunt all of them to redo them with the time we had
    /// </summary>
    private void HandleInputs()
    {
        if (!IsOwner)
            return;

        bool isLanding =
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Land") ||
            _animator.GetNextAnimatorStateInfo(0).IsName("Land");

        // Targeting
        if (Keyboard.current.tabKey.wasPressedThisFrame &&
            !isLanding &&
            !_isSliding.Value)
        {
            if (_isTargeting.Value) { }
            //  StopTargeting();
            else { }
            //  FindTarget();
        }

        if (_isTargeting.Value && Target == null)
        { } //StopTargeting();

        Vector2 input = Vector2.zero;

        if (!_isSliding.Value)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Vector3 movement;

        // Targeting movement
        if (_isTargeting.Value && Target != null)
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
                        _turnSpeed * Time.deltaTime
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
                        _turnSpeed * Time.deltaTime
                    );
            }
        }

        bool isRunning =
            !_isTargeting.Value &&
            !_isCrouching.Value &&
            Keyboard.current.leftShiftKey.isPressed &&
            movement != Vector3.zero;

        // Crouch / Slide
        if (Keyboard.current.cKey.wasPressedThisFrame &&
            _controller.isGrounded &&
            !isLanding &&
            !_isSliding.Value)
        {
            // Slide only while running normally
            if (isRunning)
            {
                _isCrouching.Value = true;
                _isSliding.Value = true;

                _slideDirection = movement.normalized;
                _slideTimer = _slideDuration;
            }
            else
            {
                // Works during normal movement AND targeting
                _isCrouching.Value =
                    !_isCrouching.Value;
            }
        }

        float speed;

        if (_isSliding.Value)
        {
            movement = _slideDirection;
            speed = _slideSpeed;

            _slideTimer -= Time.deltaTime;

            if (_slideTimer <= 0f)
            {
                _isSliding.Value = false;
            }
        }
        else if (_isCrouching.Value)
        {
            speed = _crouchSpeed;
        }
        else if (_isTargeting.Value)
        {
            speed = _targetSpeed;
        }
        else if (isRunning)
        {
            speed = _runSpeed;
        }
        else
        {
            speed = _walkSpeed;
        }

        // Gravity / Jump
        if (_controller.isGrounded)
        {
            _verticalSpeed = -2f;
            _canDoubleJump = true;

            if (_canJump && Input.GetKeyDown(KeyCode.Space))
                Jump();


        }
        else
        {
            _verticalSpeed += Physics.gravity.y * Time.deltaTime * _gravityMultiplier;

            if (_canDoubleJump && _canJump && Input.GetKeyDown(KeyCode.Space))
            {
                _verticalSpeed = 0;

                Jump();
                _canDoubleJump = false;
            }
        }

        movement =
            movement.normalized * speed;

        movement.y = _verticalSpeed;

        _controller.Move(
            movement * Time.deltaTime
        );
    }

    /// <summary>
    /// Handles the jump!
    /// </summary>
    private void Jump()
    {
            JumpCooldown();
            _verticalSpeed = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);
            _animator.SetTrigger("Jump");
    }

    /// <summary>
    /// stop you from spamming jump
    /// </summary>
    private async void JumpCooldown()
    {
        _canJump = false;
       await Delay(0.4f);
        _canJump = true;
    }

    /// <summary>
    /// wait
    /// </summary>
    /// <param name="seconds">the number of seconds to wait for</param>
    /// <returns></returns>
    private async Task Delay(float seconds)
    {
        await Awaitable.WaitForSecondsAsync(seconds);
    }

}