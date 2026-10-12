using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


public class Enemy_Bear_AttackPattern : NetworkBehaviour
{
    //ref
    private EnemyBehavior _behaviour;
    private NavMeshAgent _agent;
    private Animator _animator;

    [Header("Attack Parameters")]
    [SerializeField] private bool _isBoss;
    //Attack damage parameter are on the hitbox directly base on Sam's script
    [SerializeField] private List<GameObject> _hitboxes;
    [SerializeField] private float _targetRange = Mathf.Infinity;
    [SerializeField] private bool _isAttacking;
    [SerializeField] private int _random;



    //cd is cooldown time
    //all attack speed refere to movement speed in Navmesh Agent

    [Header("Claw Attack")]
    
    [SerializeField] private float _clawAttackCD = 3f;
    [SerializeField] private float _clawAttackSpped = 0f;
    [SerializeField] private float _clawAttackAngularSpeed = 120f;
    [SerializeField] private float _clawAttackRange = 4.5f;
    private string _clawAttack = "ClawAttack";
    private bool _canClawAtk = true;

    [Header("Double Claw Attack")]
    
    [SerializeField] private float _doubleClawCD = 8f;
    [SerializeField] private float _doubleClawAttackSpped = 0f;
    [SerializeField] private float _doubleClawAttackAngularSpeed = 60f;
    [SerializeField] private float _doubleClawAttackRange = 5f;
    private string _doubleClawAttack = "DoubleClawAttack";
    private bool _canDoubleClawAtk = true;

    [Header("Dash Attack")]
    
    [SerializeField] private float _dashAttackCD = 2f;
    [SerializeField] private float _dashAttackSpped = 60f;
    [SerializeField] private float _dashAngularSpeed = 30f;
    [SerializeField] private float _dashAttackMinRange = 7f;
    [SerializeField] private float _dashAttackMaxRange = 12f;
    private string _dashAttack = "DashAttack";
    private bool _canDashAtk = true;

    [Header("Drop Attack")]
    
    [SerializeField] private float _dropAttackCD = 10f;
    [SerializeField] private float _dropAttackSpped = 10f;
    [SerializeField] private float _dropAttackAngularSpeed = 30f;
    [SerializeField] private float _dropAttackRange = 4.5f;
    private string _dropAttack = "DropAttack";
    private bool _canDropAtk = true;

    [Header("Spin Attack")]
    
    [SerializeField] private float _spinCD = 20f;
    [SerializeField] private float _spinAttackSpped = 0.5f;
    [SerializeField] private float _spinAttackAngularSpeed = 30f;
    [SerializeField] private float _spinAttackRange = 3f;
    private string _spinAttack = "SpinAttack";
    private bool _canSpinAtk = true;

    private void Awake()
    {
        _behaviour = GetComponent<EnemyBehavior>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        DeactivateHitboxes();
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;

        AttackLogic();
    }

    private void AttackLogic()
    {
        if (_behaviour.Target != null)
        {
            _targetRange = GetTargetDistance(_behaviour.Target);

            if(_random <= 0)
            _random = Random.Range(0,9);

            if (_random > 0 && _targetRange <= _clawAttackRange && !_isAttacking)
            {
                ChoiceAttack(_random);
            }

            //Too buggy
          //  if (_targetRange > _dashAttackMinRange && _targetRange < _dashAttackMaxRange && !_animator.GetBool("isAttacking"))
           // { 
           //  DashAttack();
          //  }
        }
    }

    private void ChoiceAttack(int prob)
    {
        if (_isBoss)
        {
            switch (prob)
            {
                case 1: NormalClawAttack(); break;
                case 2: DoubleClawAttack(); break;
                case 3: DropAttack(); break;
                case 4: SpinAttack(); break;
                case 5: NormalClawAttack(); break;
                case 6: DoubleClawAttack(); break;
                case 7: DropAttack(); break;
                case 8: DropAttack(); break;
                case 9: DropAttack(); break;
                case 10: DoubleClawAttack(); break;
                    //case 11: DashAttack();  break;
            }
        }
        else 
        {
            switch (prob)
            {
                case 1: NormalClawAttack(); break;
                case 2: DoubleClawAttack(); break;
                case 3: NormalClawAttack(); break;
                case 4: NormalClawAttack(); break;
                case 5: NormalClawAttack(); break;
                case 6: DoubleClawAttack(); break;
                case 7: NormalClawAttack(); break;
                case 8: NormalClawAttack(); break;
                case 9: DropAttack(); break;
                case 10: DoubleClawAttack(); break;
                    //case 11: DashAttack();  break;
            }
        }
        
    }

    private async void NormalClawAttack()
    {
        if (_canClawAtk && !_isAttacking)
        {
            _isAttacking = true;
            _random = Random.Range(0, 9);
            _canClawAtk = false;
            PlayAttack(_clawAttack, _clawAttackSpped, _clawAttackAngularSpeed);

            await Delay(_clawAttackCD);

            _canClawAtk = true;

        }
        else if (_canClawAtk == false)
        {
            _random = 1;
        }
    }

    private async void DoubleClawAttack()
    {
        if (_canDoubleClawAtk && !_isAttacking)
        {
            _isAttacking = true;
            _random = Random.Range(0, 9);
            _canDoubleClawAtk = false;
            PlayAttack(_doubleClawAttack, _doubleClawAttackSpped, _doubleClawAttackAngularSpeed);

            await Delay(_doubleClawCD);

            _canDoubleClawAtk = true;

        }
        else if (_canDoubleClawAtk == false)
        {
            _random = 1;
        }
    }

    private async void DropAttack()
    {
        if (_canDropAtk && !_isAttacking)
        {
            _isAttacking = true;
            _random = Random.Range(0, 9);
            _canDropAtk = false;
            PlayAttack(_dropAttack, _dropAttackSpped, _dropAttackAngularSpeed);

            await Delay(_dropAttackCD);

            _canDropAtk = true;

        }
        else if (_canDropAtk == false)
        {
            _random = 1;
        }
    }
    private async void DashAttack()
    {
        if (_canDashAtk && !_isAttacking)
        {
            _isAttacking = true;
            _random = Random.Range(0, 9);
            _canDashAtk = false;
            PlayAttack(_dashAttack, _dashAttackSpped, _dashAngularSpeed);

            await Delay(_dashAttackCD);

            _canDashAtk = true;

        }
        else if (_canDashAtk == false)
        {
            _random = 1;
        }
    }

    private async void SpinAttack()
    {
        if (_canSpinAtk && !_isAttacking)
        {
            _isAttacking = true;
            _random = Random.Range(0, 9);
            _canSpinAtk = false;
            PlayAttack(_spinAttack, _spinAttackSpped, _spinAttackAngularSpeed);

            await Delay(_spinCD);

            _canSpinAtk = true;

        }
        else if (_canSpinAtk == false)
        {
            _random = 1;
        }
    }

    private float GetTargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    private void PlayAttack(string attackName, float a_speed, float a_angular)
    {
        _animator.SetTrigger(attackName);
        _agent.speed = a_speed;
        _agent.angularSpeed = a_angular;
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

    public void IsAttacking()
    { 
        _animator.SetBool("isAttacking", true);
    }

    public void NotAttacking()
    {
        _animator.SetBool("isAttacking", false);
        _isAttacking = false;
    }

    private void DeactivateHitboxes()
    {
        foreach (GameObject box in _hitboxes)
        { 
          box.SetActive(false);
        }
    }
    //Async and Countdown utilities

    //Delay X second
    private async Task Delay(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }

    //count any variable to 0
    private float Countdown(float time)
    {
        time = Mathf.MoveTowards(time, 0, Time.deltaTime);

        return time;
    }
}












//Jun: Personal note
//This could be another child Class and have access to the same variable without referencing it again.
//But it would also requiring to redo everything inside the prefab
//Use Struct for each Attack parameter as they are repetitive
