using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


public class Enemy_Bear_AttackPattern : NetworkBehaviour
{
    //ref
    // enemy logic
    private EnemyBehavior _behaviour;

    // NavMeshAgent
    private NavMeshAgent _agent;

    // Enemy Animator
    private Animator _animator;

    [Header("Attack Parameters")]
    // if the enemy is a boss or not
    [SerializeField] private bool _isBoss;

    //Attack damage parameter are on the hitbox directly base on Sam's script
    [SerializeField] private List<GameObject> _hitboxes;
    
    // detection range
    [SerializeField] private float _targetRange = Mathf.Infinity;

    // if the enemy is attacking or not
    [SerializeField] private bool _isAttacking;

    // a random number
    [SerializeField] private int _random;



    //cd is cooldown time
    //all attack speed refere to movement speed in Navmesh Agent

    [Header("Claw Attack")]
    // claw attack cooldown
    [SerializeField] private float _clawAttackCD = 3f;

    // claw attack speed
    [SerializeField] private float _clawAttackSpped = 0f;

    // I think angular speed is rotation velocity
    [SerializeField] private float _clawAttackAngularSpeed = 120f;

    // claw range
    [SerializeField] private float _clawAttackRange = 4.5f;

    // name of the claw attack
    private string _clawAttack = "ClawAttack";

    // if the bear can claw right now
    private bool _canClawAtk = true;

    [Header("Double Claw Attack")]
    // double claw attack cooldown
    [SerializeField] private float _doubleClawCD = 8f;

    // double claw attack speed
    [SerializeField] private float _doubleClawAttackSpped = 0f;

    // double claw attack rotation velocity
    [SerializeField] private float _doubleClawAttackAngularSpeed = 60f;

    // double claw attack range
    [SerializeField] private float _doubleClawAttackRange = 5f;

    // name of the double claw attack
    private string _doubleClawAttack = "DoubleClawAttack";

    // if the bear can double claw attack rn
    private bool _canDoubleClawAtk = true;

    [Header("Dash Attack")]
    // dash attack cooldown
    [SerializeField] private float _dashAttackCD = 2f;

    // dash attack speed
    [SerializeField] private float _dashAttackSpped = 60f;

    // dash attack rotation velocity
    [SerializeField] private float _dashAngularSpeed = 30f;

    // minimum dash range
    [SerializeField] private float _dashAttackMinRange = 7f;

    // maximum dash range
    [SerializeField] private float _dashAttackMaxRange = 12f;

    // dash attack name
    private string _dashAttack = "DashAttack";

    // if the enemy can dash attack right now
    private bool _canDashAtk = true;

    [Header("Drop Attack")]
    // drop attack cooldown
    [SerializeField] private float _dropAttackCD = 10f;

    // drop attack speed
    [SerializeField] private float _dropAttackSpped = 10f;

    // drop attack rotation speed
    [SerializeField] private float _dropAttackAngularSpeed = 30f;

    // drop attack speed
    [SerializeField] private float _dropAttackRange = 4.5f;

    // drop attack name
    private string _dropAttack = "DropAttack";

    // if the enemy can drop attack right now
    private bool _canDropAtk = true;

    [Header("Spin Attack")]
    // spin attack cooldown
    [SerializeField] private float _spinCD = 20f;

    // spin attack speed
    [SerializeField] private float _spinAttackSpped = 0.5f;

    // spin attack rotation velocity
    [SerializeField] private float _spinAttackAngularSpeed = 30f;

    // spin attack range
    [SerializeField] private float _spinAttackRange = 3f;

    // spin attack name
    private string _spinAttack = "SpinAttack";

    // if the enemy can spin attack right now
    private bool _canSpinAtk = true;

    /// <summary>
    /// Innit on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// get a few components
    /// </summary>
    private void Innit()
    {
        _behaviour = GetComponent<EnemyBehavior>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// deactivate hitboxes by default
    /// </summary>
    private void OnEnable()
    {
        DeactivateHitboxes();
    }

    /// <summary>
    /// Attack!
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsServer) return;

        AttackLogic();
    }

    /// <summary>
    /// the enemy behavior
    /// </summary>
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

    /// <summary>
    /// attack behavior is semi-random
    /// </summary>
    /// <param name="prob">the attack chosen</param>
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

    /// <summary>
    /// claw attack logic
    /// </summary>
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

    /// <summary>
    /// double claw attack logic
    /// </summary>
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

    /// <summary>
    /// drop attack logic
    /// </summary>
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

    /// <summary>
    /// dash attack logic
    /// </summary>
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

    /// <summary>
    /// spin attack logic
    /// </summary>
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

    /// <summary>
    /// Gets the distance from the target
    /// </summary>
    /// <param name="target">the target</param>
    /// <returns>the distance from the target</returns>
    private float GetTargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    /// <summary>
    /// Start attacks
    /// </summary>
    /// <param name="attackName">the attack to start</param>
    /// <param name="a_speed">attack speed</param>
    /// <param name="a_angular">angular velocity</param>
    private void PlayAttack(string attackName, float a_speed, float a_angular)
    {
        _animator.SetTrigger(attackName);
        _agent.speed = a_speed;
        _agent.angularSpeed = a_angular;
    }

    //Attack methods that can be called with animation
    /// <summary>
    /// activate the hitbox when needed
    /// </summary>
    /// <param name="boxNumber">which hitbox</param>
    public void ActiveHitBox(int boxNumber)
    {
        _hitboxes[boxNumber].gameObject.SetActive(true);
    }

    /// <summary>
    /// deactivate the hitbox when not needed
    /// </summary>
    /// <param name="boxNumber">which hitbox</param>
    public void DeactiveHitBox(int boxNumber)
    {
        _hitboxes[boxNumber].gameObject.SetActive(false);
    }

    /// <summary>
    /// starts attack animation
    /// </summary>
    public void IsAttacking()
    { 
        _animator.SetBool("isAttacking", true);
    }

    /// <summary>
    /// stop attacking animation
    /// </summary>
    public void NotAttacking()
    {
        _animator.SetBool("isAttacking", false);
        _isAttacking = false;
    }

    /// <summary>
    /// deactivate all hitboxes
    /// </summary>
    private void DeactivateHitboxes()
    {
        foreach (GameObject box in _hitboxes)
        { 
          box.SetActive(false);
        }
    }
    //Async and Countdown utilities

    //Delay X second
    /// <summary>
    /// wait for a moment
    /// </summary>
    /// <param name="sec">nb of seconds it waits for</param>
    private async Task Delay(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }

    /// <summary>
    /// count any variable to 0
    /// </summary>
    /// <param name="time">the time at which the countdown begins</param>
    /// <returns>the remaining time to the countdown</returns>
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
