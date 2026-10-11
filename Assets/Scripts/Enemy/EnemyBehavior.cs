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

    [Header("Enemy ref")]
    [SerializeField] private Animator _animator;

    [Header("Navmesh/Navigation AI")]
    [SerializeField] private float _IdleSpeed = 0f;
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 30f;
    [SerializeField] private float _pursuitRange = 100f;
    [SerializeField] private float _idleStopDis = 0f;
    [SerializeField] private float _aggressiveStopDis = 2f;
    [SerializeField] private float _normalAngularSpeed = 500f;
    [SerializeField] private float _attackAngularSpeed = 250f;
    [SerializeField] private float _idleTime = 2f;

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

    [Header("Attack Parameters")]
    [SerializeField] private float _dttackCooldown = 2f;
    [SerializeField] private float _dashAttackSpped = 60f;
    [SerializeField] private float _dashAngularSpeed = 30f;
    [SerializeField] private List<GameObject> _hitboxes;


    [Header("Sound")]
    [SerializeField] private AudioSource _audioPlayer;
    [SerializeField] private AudioClip _bearNoise;


    private void Awake()
    {
        _vision = _visionBox.GetComponent<EnemyViewDetection>();
    }

    private void OnEnable()
    {
        TransitionTo(NPCStateEnum.Idle);
        _stateActiveTime = _stateActiveTimeLimit;

    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        BehaviourLogic();
    }

    

    private async void BehaviourLogic()
    {
        // Bear see you
        if (_seePlayer)
        {
            await Delay(1f);
            TransitionTo(NPCStateEnum.Aggressive);
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
            TransitionTo(NPCStateEnum.Idle);
        }

    }

    //Enemy State

    private void TransitionTo(NPCStateEnum next)
    {
        NPCState = next;

        switch (next)
        {
            case NPCStateEnum.Idle: IdleState(); break;
            case NPCStateEnum.Active: ActiveState(); break;
            case NPCStateEnum.Aggressive: AggressiveState(); break;
            case NPCStateEnum.Attacking: AttackingState(); break;
            case NPCStateEnum.Death: DeathState(); break;
        }
    }


    private async void IdleState()
    {
        //Set state parameters
        Agent.speed = _IdleSpeed;
        Agent.stoppingDistance = _idleStopDis;
        _stateActiveTime = _stateActiveTimeLimit;
        Agent.angularSpeed = _normalAngularSpeed;
        StopNavAgent();

        //Bear rest time
        await Delay(_idleTime);

        RestartNavAgent();
        
        //Bear saw no one, start walking around
        if (!_seePlayer || _targetPlayers.Count == 0)
        {
            TransitionTo(NPCStateEnum.Active);
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
        //disable unnecessary detection, vision can be heavy
        //set parameter
        _visionBox.SetActive(false);

        if (Agent.isActiveAndEnabled) 
        {
            Agent.stoppingDistance = _aggressiveStopDis;
            Agent.speed = _runSpeed;
            Agent.angularSpeed = _normalAngularSpeed;

            //Old method is still useful, but use the new one, since it doens't search the player constantly
             GetPlayerTargetAndChase();
        }
    }

    //Navemesh Functions
    public void GoToPoint(Vector3 point)
    {
        if (Agent.isActiveAndEnabled)
            Agent.SetDestination(point);
    }

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
        if (Target != null && Agent.isActiveAndEnabled)
        {
            Agent.SetDestination(Target.position);
        }
    }

    private void GetPlayerTargetAndChase()
    {
        if (Target == null || Vector3.Distance(transform.position, Target.position) > PursuitRange)
        {
            Transform newTarget = _targetPlayers[0].gameObject.transform;

            Target = newTarget;
        }

        if (Target != null)
        {
            Agent.SetDestination(Target.position);
        }
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

    private void CheckPatrolProgress()
    {
        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
        {
            TransitionTo(NPCStateEnum.Idle);
        }
    }

    private void AttackingState()
    {
        
    }

    private void DeathState()
    {
        if (!IsServer) return;


         NetworkObject.Despawn();
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
            TransitionTo(NPCStateEnum.Active);
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



    




    //Debug

    [ContextMenu("Debug: Stanby")]
    public void StanbyTest() => TransitionTo(NPCStateEnum.Idle);

    [ContextMenu("Debug: Patrol")]
    public void PatrolTest() => TransitionTo(NPCStateEnum.Active);

    [ContextMenu("Debug: Chase")]
    public void ChaseTest() => TransitionTo(NPCStateEnum.Aggressive);
}
