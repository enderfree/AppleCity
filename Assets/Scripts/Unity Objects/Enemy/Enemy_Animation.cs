using UnityEngine;

/// <summary>
/// Handles the animation of the enemy
/// </summary>
public class Enemy_Animation : MonoBehaviour
{
    // the enemy's animator
    private Animator _animator;

    // the enemy's character controller
    private CharacterController _controller;

    // the enemy's logic
    private EnemyBehavior _behaviour;

    // the enemy's speed
    private float _speed;

    // I'm not sure, apparently Jun took it from Josh and he is not sure either
    private float _speedDeltaTimeMultiplier = 3f;

    // the last position where the enemy was
    private Vector3 _lastPosition;

    /// <summary>
    /// Initialize in Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// get a ton of components
    /// </summary>
    private void Innit()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _behaviour = GetComponent<EnemyBehavior>();

        _lastPosition = transform.position;
    }

    /// <summary>
    /// Move in update
    /// </summary>
    private void LateUpdate()
    {
        MovementSpeed();

        
    }

    /// <summary>
    /// Move
    /// </summary>
    private void MovementSpeed()
    {
        Vector3 movement = transform.position - _lastPosition;
        Vector3 horizontalMovement = new Vector3(movement.x, 0f, movement.z);
        _speed = Mathf.MoveTowards(_speed, horizontalMovement.magnitude / Time.deltaTime, Time.deltaTime * _speedDeltaTimeMultiplier);
        
        float verticalSpeed = movement.y / Time.deltaTime;
        Vector3 localMovement = Vector3.zero;

        if (horizontalMovement.magnitude > 0.001f)
        {
            localMovement = transform.InverseTransformDirection(horizontalMovement.normalized);
        }

        _animator.SetFloat("Speed_f", _speed);

        _lastPosition = transform.position;
    }
}
