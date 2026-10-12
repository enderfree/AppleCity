using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Enemy_Bear_AttackPattern : NetworkBehaviour
{
    //ref
    private EnemyBehavior _behaviour;
    private NavMeshAgent _agent;
    private Animator _animator;

    [Header("Attack Parameters")]
    //Attack damage parameter are on the hitbox directly base on Sam's script
    [SerializeField] private List<GameObject> _hitboxes;
    [SerializeField] private float _targetRange;


    //cd is cooldown time
    //all attack speed refere to movement speed in Navmesh Agent

    [Header("Claw Attack")]
    private string _clawAttack = "ClawAttack";
    [SerializeField] private float _clawAttackCD = 3f;
    [SerializeField] private float _clawAttackSpped = 0.5f;
    [SerializeField] private float _clawAttackAngularSpeed = 250f;

    [Header("Double Claw Attack")]
    private string _doubleClawAttack = "DoubleClawAttack";
    [SerializeField] private float _doubleClawCD = 8f;
    [SerializeField] private float _doubleClawAttackSpped = 0f;
    [SerializeField] private float _doubleClawAttackAngularSpeed = 60f;

    [Header("Dash Attack")]
    private string _dashAttack = "DashAttack";
    [SerializeField] private float _dashAttackCD = 2f;
    [SerializeField] private float _dashAttackSpped = 60f;
    [SerializeField] private float _dashAngularSpeed = 30f;

    [Header("Drop Attack")]
    private string _dropAttack = "DropAttack";
    [SerializeField] private float _dropAttackCD = 10f;
    [SerializeField] private float _dropAttackSpped = 10f;
    [SerializeField] private float _dropAttackAngularSpeed = 30f;

    [Header("Spin Attack")]
    private string _spinAttack = "SpinAttack";
    [SerializeField] private float _spinCD = 20f;
    [SerializeField] private float _spinAttackSpped = 0.5f;
    [SerializeField] private float _spinAttackAngularSpeed = 30f;

    private void Awake()
    {
        _behaviour = GetComponent<EnemyBehavior>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;

        if(_behaviour.Target != null)
        _targetRange = GetTargetDistance(_behaviour.Target);
    }
    private float GetTargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    private void PlayAttackAnimation(string attackName)
    {
        _animator.SetTrigger(attackName);
    }

    //Attack methods that can be called with animation
    public void ActiveHitBox(int boxNumber)
    {
        _hitboxes[boxNumber].gameObject.SetActive(true);
    }

    public void DeactiveHitBox(int boxNumber)
    {
        _hitboxes[boxNumber].gameObject.SetActive(false);
    }
}












//Jun: Personal note
//This could be another child Class and have access to the same variable without referencing it again.
//But it would also requiring to redo everything inside the prefab