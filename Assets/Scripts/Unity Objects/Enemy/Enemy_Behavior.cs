using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : NPC
{
    //HP and other parameters are handled by parent classes
    //Changes: 1. HP parameter should be handled with SyncVar 2. Character GrandParent should be networkBehavior to make everything online

  
     private Animator _animator;

    [Header("Navmesh/Navigation AI")]
    [SerializeField] private float _IdleSpeed = 0f;
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 30f;
    [SerializeField] private float _pursuitRange = 50f;
    [SerializeField] private float _idleStopDis = 0f;
    [SerializeField] private float _aggressiveStopDis = 2f;
    [SerializeField] private float _normalAngularSpeed = 500f;
    [SerializeField] private float _idleTime = 7f;
    [SerializeField] private float _idleTimeShort = 2f;

    //Random point and compare currentposition to designated position to decide whenever it should go Idle or choose a new point
    [SerializeField] private float _searchMinRadius = 30f,_searchMaxRadius = 200f ;
    [SerializeField] private Vector3 _recordedPosition;

    //Bear kinda need a rest too if being active for too long
    [SerializeField] private float _stateActiveTime;
    [SerializeField] private float _stateActiveTimeLimit = 60f;

    [Header("Player Detection")]
    [SerializeField] private GameObject _visionBox;
    [SerializeField] private GameObject _searchBox;
    private EnemyViewDetection _vision;
    private bool _seePlayer => _vision._readSawPlayer;
    [SerializeField] private List<GameObject> _targetPlayers;

    [Header("Attack State")]
    [SerializeField] private float _attackAnimDuration;
    [SerializeField] private float _attackCooldown = 1.1f;
    private bool _canAttack = true;

    [Header("Sound")]
    [SerializeField] private AudioClip _bearNoise;
    [SerializeField] private AudioClip _deathSound;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _vision = _visionBox.GetComponent<EnemyViewDetection>();
    }

    private void OnEnable()
    {
        TransitionTo(NPCStateEnum.Idle, _idleTimeShort);
        _stateActiveTime = _stateActiveTimeLimit;
        _canAttack = true;
        

    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        BehaviourLogic();
    }

    

    private async void BehaviourLogic()
    {
        // Bear see you
        if (_seePlayer && NPCState == NPCStateEnum.Active || NPCState == NPCStateEnum.Idle)
        {
            if (NPCState == NPCStateEnum.Attacking) return;
            await Delay(1f);
            TransitionTo(NPCStateEnum.Aggressive,0);
        }

        //Bear is angry at you
        if (NPCState == NPCStateEnum.Aggressive)
        {
            if (Agent.isActiveAndEnabled)
                  ChaseTarget();
        }

        //Bear just chill around
        else if (NPCState == NPCStateEnum.Active)
        {
            if(Agent.isActiveAndEnabled)
                CheckPatrolProgress();

            _stateActiveTime = Countdown(_stateActiveTime);
        }

        //Bear tired, need rest
        if (_stateActiveTime == 0)
        {
            TransitionTo(NPCStateEnum.Idle, _idleTime);
        }

    }

    //Enemy State

    private void TransitionTo(NPCStateEnum next, float idleTime)
    {
        switch (next)
        {
            case NPCStateEnum.Idle: IdleState(idleTime); break;
            case NPCStateEnum.Active: ActiveState(); break;
            case NPCStateEnum.Aggressive: AggressiveState(); break;
            case NPCStateEnum.Attacking: AttackingState(); break;
            case NPCStateEnum.Death: DeathState(); break;
        }

        NPCState = next;
    }


    private async void IdleState(float idleTime)
    {
        //Set state parameters
        Agent.speed = _IdleSpeed;
        Agent.stoppingDistance = _idleStopDis;
        _stateActiveTime = _stateActiveTimeLimit;
        Agent.angularSpeed = _normalAngularSpeed;
        StopNavAgent();

        //Bear rest time
        await Delay(idleTime);

        RestartNavAgent();
        
        //Bear saw no one, start walking around
        if (!_seePlayer || _targetPlayers.Count == 0)
        {
            TransitionTo(NPCStateEnum.Active, 0);
        }

    }

    // Go back to Idle when It is super close to the destination
    private void CheckPatrolProgress()
    {
        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
        {
            TransitionTo(NPCStateEnum.Idle, _idleTime);
        }
    }
    private void ActiveState()
    {
        //Set state parameters
        _visionBox.SetActive(true);

        if (Agent.isActiveAndEnabled)
        {
            Agent.angularSpeed = _normalAngularSpeed;
            Agent.speed = _walkSpeed;

            //looking for where it wanna go
            Vector3 generateRandomPoint = RandomNavmeshLocation(_searchMinRadius, _searchMaxRadius);

            _recordedPosition = generateRandomPoint;
            Agent.SetDestination(generateRandomPoint);

            RestartNavAgent();
        }

        
    }
    private void AggressiveState()
    {
        if (NPCState == NPCStateEnum.Aggressive) return;
        if (_animator.GetBool("isAttacking") == true) return;

        //disable unnecessary detection, vision can be heavy
        _visionBox.SetActive(false);

       
         //set parameter
            Agent.stoppingDistance = _aggressiveStopDis;
            Agent.speed = _runSpeed;
            Agent.angularSpeed = _normalAngularSpeed;

           
             GetPlayerTargetAndChase();
       
    }

    //AttackState should exit after Attack ended and go back to Aggressive or Active
    //New script for Attack Patterns
    private async void AttackingState()
    {
        _canAttack = false;
        AnimatorStateInfo animState = _animator.GetCurrentAnimatorStateInfo(0);
        _attackAnimDuration = animState.length;
        
        await Delay(_attackAnimDuration);

        //clear target and get a new one if detected
        //ActiveSearchBox() should have a 1 sec async and active SearchPlayerList() later that update
        Target = null;
        ActiveSearchBox(); 
        

        if (_targetPlayers.Count >= 1)
        {
            TransitionTo(NPCStateEnum.Aggressive, 0);
        }
        else
        {
            TransitionTo(NPCStateEnum.Idle, _idleTimeShort);
        }


        await Delay(_attackCooldown);

        _canAttack = true;
    }

    private async Task DeathState()
    {
        if (!IsServer) return;

        StopNavAgent();
        this.GetComponent<AudioSource>().PlayOneShot(_deathSound);
        await Delay(2f);
        NetworkObject.Despawn();
    }

    public override void OnHit(float damage)
    {
        base.OnHit(damage);

        if (HP > 0)
        {
            TransitionTo(NPCStateEnum.Aggressive, 0);
        }
        else if (HP <= 0)
        {
            TransitionTo(NPCStateEnum.Death, 0);
        }
    }

    //Navemesh Functions

    public void StopNavAgent()
    {
        if (Agent.isActiveAndEnabled)
            Agent.isStopped = true;
    }

    public void RestartNavAgent()
    {
        if (Agent.isActiveAndEnabled)
            Agent.isStopped = false;
    }

    private void ChaseTarget()
    {
        // Chase player within range limit, if out of range, go back to idle
        if (Agent.isActiveAndEnabled && Target != null && Vector3.Distance(transform.position, Target.position) < PursuitRange)
        {
            Agent.SetDestination(Target.position);
        }
        else 
        {
            TransitionTo(NPCStateEnum.Idle, _idleTimeShort);
        }
    }

    private void GetPlayerTargetAndChase()
    {
        if (Target != null ||  _targetPlayers.Count > 0)
        {
            Transform newTarget = _targetPlayers[0].gameObject.transform;

            Target = newTarget;
        }

        ChaseTarget();
    }

    public Vector3 RandomNavmeshLocation(float minRadius, float maxRadius)
    {
        //Set a min-max radius and randomize the sphere unit to generate a random patrol point
        float radius = UnityEngine.Random.Range(minRadius, maxRadius);
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;

        // Based on the current position
        randomDirection += transform.position;

        //vecotr zero mean nothing, just can't be null
        Vector3 finalPosition = Vector3.zero;

        //similar to raycast
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }
    //Make it trigger by animation
    public void EnterAttackState()
    {
        TransitionTo(NPCStateEnum.Attacking,0);
        Debug.Log("Is Attacking State");
    }

    //Get player list from search to update target to Attack
    //Search at the end of transition from Attack>Aggressive or CountTime after x time without Attacking, if all player escaped from the detection range,bear will go back to Active Mode
    public void ActiveSearchBox()
    {
        _searchBox.gameObject.SetActive(true);
    }

    //This one is called from Search Detection
    public void SearchPlayerList(List<GameObject> list)
    {
        _targetPlayers = list;
        

        if (_targetPlayers == null || _targetPlayers.Count == 0)
        {
            TransitionTo(NPCStateEnum.Active, 0);
        }
    }

    public void AddTargetOnSight(GameObject target)
    {
        if (!_targetPlayers.Contains(target))
        { 
         _targetPlayers.Add(target);
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
        time = Mathf.MoveTowards(time,0, Time.deltaTime);

        return time;
    }

    // Getters and Setters
    public virtual float PursuitRange
    {
        get
        {
            return _pursuitRange;
        }
        set
        {
            _pursuitRange = value;
        }
    }

    public virtual float AttackCooldown
    {
        get
        {
            return _attackCooldown;
        }
        set
        {
            _attackCooldown = value;
        }
    }

    public virtual bool CanAttack
    {
        get
        {
            return _canAttack;
        }
        set
        {
            _canAttack = value;
        }
    }

    //Debug

    [ContextMenu("Debug: Stanby")]
    public void StanbyTest() => TransitionTo(NPCStateEnum.Idle, _idleTime);

    [ContextMenu("Debug: Patrol")]
    public void PatrolTest() => TransitionTo(NPCStateEnum.Active,0);

    [ContextMenu("Debug: Chase")]
    public void ChaseTest() => TransitionTo(NPCStateEnum.Aggressive,0);

    [ContextMenu("Debug: Attack")]
    public void AttackTest() => TransitionTo(NPCStateEnum.Attacking,0);
}
