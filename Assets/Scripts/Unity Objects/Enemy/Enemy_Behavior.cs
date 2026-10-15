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

    // the animator
    private Animator _animator;

    [Header("Navmesh/Navigation AI")]
    // speed when idle
    [SerializeField] private float _IdleSpeed = 0f;

    // speed when walking
    [SerializeField] private float _walkSpeed = 4f;

    // speed when running
    [SerializeField] private float _runSpeed = 30f;

    // range in which the enemy chases
    [SerializeField] private float _pursuitRange = 50f;

    // at which distance does the idle state stop
    [SerializeField] private float _idleStopDis = 0f;

    // at which distance does the aggressive state stop
    [SerializeField] private float _aggressiveStopDis = 2f;

    // velocity
    [SerializeField] private float _normalAngularSpeed = 500f;

    // transition state time
    [SerializeField] private float _idleTime = 7f;

    // short transition time
    [SerializeField] private float _idleTimeShort = 2f;

    //Random point and compare currentposition to designated position to decide whenever it should go Idle or choose a new point
    [SerializeField] private float _searchMinRadius = 30f,_searchMaxRadius = 200f ;

    // last known position
    [SerializeField] private Vector3 _recordedPosition;

    //Bear kinda need a rest too if being active for too long  // cool -SF
    [SerializeField] private float _stateActiveTime;

    // sadly rests have limits too
    [SerializeField] private float _stateActiveTimeLimit = 60f;

    [Header("Player Detection")]
    // enemy vision box
    [SerializeField] private GameObject _visionBox;

    // where the enemy searches
    [SerializeField] private GameObject _searchBox;

    // view component
    private EnemyViewDetection _vision;

    // if the enemy sees the player
    private bool _seePlayer => _vision._readSawPlayer;

    // list of players
    [SerializeField] private List<GameObject> _targetPlayers;

    [Header("Attack State")]
    // duration of the attack animation
    [SerializeField] private float _attackAnimDuration;

    // cooldown between attacks
    [SerializeField] private float _attackCooldown = 1.1f;

    // if the enemy can attack right now
    private bool _canAttack = true;

    [Header("Sound")]
    // the sound of a bear
    [SerializeField] private AudioClip _bearNoise;

    // the sound of a dying bear
    [SerializeField] private AudioClip _deathSound;

    /// <summary>
    /// Initialise on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// Get some components
    /// </summary>
    private void Innit()
    {
        _animator = GetComponent<Animator>();
        _vision = _visionBox.GetComponent<EnemyViewDetection>();
    }

    /// <summary>
    /// setup a few more things when the enemy is enabled
    /// </summary>
    private void OnEnable()
    {
        TransitionTo(NPCStateEnum.Idle, _idleTimeShort);
        _stateActiveTime = _stateActiveTimeLimit;
        _canAttack = true;
        

    }

    /// <summary>
    /// update the enemy
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsServer) return;

        BehaviourLogic();
    }

    
    /// <summary>
    /// Enemy logic
    /// </summary>
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
    /// <summary>
    /// manages state transitions
    /// </summary>
    /// <param name="next">selected state</param>
    /// <param name="idleTime">if state id idle, for how long do you stay idle</param>
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

    /// <summary>
    /// handles the idle state
    /// </summary>
    /// <param name="idleTime">for how long it stays idle</param>
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

    /// <summary>
    /// Go back to Idle when It is super close to the destination
    /// </summary>
    private void CheckPatrolProgress()
    {
        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
        {
            TransitionTo(NPCStateEnum.Idle, _idleTime);
        }
    }

    /// <summary>
    /// walk around
    /// </summary>
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

    /// <summary>
    /// looks for a fight
    /// </summary>
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
    /// <summary>
    /// handles the attack
    /// </summary>
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

    /// <summary>
    /// dying is harder than one would think
    /// </summary>
    private async Task DeathState()
    {
        if (!IsServer) return;

        StopNavAgent();
        this.GetComponent<AudioSource>().PlayOneShot(_deathSound);
        await Delay(2f);
        NetworkObject.Despawn();
    }

    /// <summary>
    /// become aggressive on hit unless it's a lethal hit
    /// </summary>
    /// <param name="damage">how much damage is recieved</param>
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
    /// <summary>
    /// stop the agent
    /// </summary>
    public void StopNavAgent()
    {
        if (Agent.isActiveAndEnabled)
            Agent.isStopped = true;
    }

    /// <summary>
    /// restart the agent
    /// </summary>
    public void RestartNavAgent()
    {
        if (Agent.isActiveAndEnabled)
            Agent.isStopped = false;
    }

    /// <summary>
    /// agent chase logic
    /// </summary>
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

    /// <summary>
    /// manages which player is being chased
    /// </summary>
    private void GetPlayerTargetAndChase()
    {
        if (Target != null ||  _targetPlayers.Count > 0)
        {
            Transform newTarget = _targetPlayers[0].gameObject.transform;

            Target = newTarget;
        }

        ChaseTarget();
    }

    /// <summary>
    /// get a random location on the navmesh
    /// </summary>
    /// <param name="minRadius">how close the point is allowed to be</param>
    /// <param name="maxRadius">how far the point is allowed to be</param>
    /// <returns></returns>
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
    /// <summary>
    /// Transition to attack state
    /// </summary>
    public void EnterAttackState()
    {
        TransitionTo(NPCStateEnum.Attacking,0);
        Debug.Log("Is Attacking State");
    }

    /// <summary>
    /// //Get player list from search to update target to Attack
    /// Search at the end of transition from Attack>Aggressive or CountTime after x time without Attacking, 
    /// if all player escaped from the detection range,bear will go back to Active Mode
    /// </summary>
    public void ActiveSearchBox()
    {
        _searchBox.gameObject.SetActive(true);
    }

    //This one is called from Search Detection
    /// <summary>
    /// Find next victim or roam
    /// </summary>
    /// <param name="list">the list of players</param>
    public void SearchPlayerList(List<GameObject> list)
    {
        _targetPlayers = list;
        

        if (_targetPlayers == null || _targetPlayers.Count == 0)
        {
            TransitionTo(NPCStateEnum.Active, 0);
        }
    }

    /// <summary>
    /// add people to the wanted list based on sight
    /// </summary>
    /// <param name="target">people</param>
    public void AddTargetOnSight(GameObject target)
    {
        if (!_targetPlayers.Contains(target))
        { 
         _targetPlayers.Add(target);
        }
    }


    //Async and Countdown utilities

    //Delay X second
    /// <summary>
    /// wait
    /// </summary>
    /// <param name="sec">nb of sec to wait</param>
    /// <returns></returns>
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
    // state transition debug commands
    [ContextMenu("Debug: Stanby")]
    public void StanbyTest() => TransitionTo(NPCStateEnum.Idle, _idleTime);

    [ContextMenu("Debug: Patrol")]
    public void PatrolTest() => TransitionTo(NPCStateEnum.Active,0);

    [ContextMenu("Debug: Chase")]
    public void ChaseTest() => TransitionTo(NPCStateEnum.Aggressive,0);

    [ContextMenu("Debug: Attack")]
    public void AttackTest() => TransitionTo(NPCStateEnum.Attacking,0);
}
