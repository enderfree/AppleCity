using UnityEngine;

public class Enemy_Animation : MonoBehaviour
{
    private Animator _animator;

    private CharacterController _controller;
    private EnemyBehavior _behaviour;
    private float _speed;
    private float speedDeltaTimeMultiplier = 1f;

    private Vector3 lastPosition;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller =GetComponent<CharacterController>();
        _behaviour = GetComponent<EnemyBehavior>();

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        MovementSpeed();

        
    }

    private void MovementSpeed()
    {
        Vector3 movement = transform.position - lastPosition;
        Vector3 horizontalMovement = new Vector3(movement.x, 0f, movement.z);
        _speed = Mathf.MoveTowards(_speed, horizontalMovement.magnitude / Time.deltaTime, Time.deltaTime * speedDeltaTimeMultiplier);
        
        float verticalSpeed = movement.y / Time.deltaTime;
        Vector3 localMovement = Vector3.zero;

        if (horizontalMovement.magnitude > 0.001f)
        {
            localMovement = transform.InverseTransformDirection(horizontalMovement.normalized);
        }

        _animator.SetFloat("Speed_f", _speed);

        lastPosition = transform.position;
    }
}
