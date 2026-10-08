using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : NPC
{
    //HP and other parameters are handled by parent classes
    //Changes: 1. HP parameter should be handled with SyncVar 2. Character GrandParent should be networkBehavior to make everything online

    [Header("Enemy ref")]
    [SerializeField] private Animator _animator;

    [Header("Player Detection")]
    [SerializeField] private GameObject _visionBox;
    [SerializeField] private GameObject _searchBox;
    private EnemyViewDetection _vision;
    

    private bool _seePlayer => _vision._readSawPlayer;
    private List<GameObject> _targetPlayers = new List<GameObject>();

    [Header("Attack Parameters")]
    [SerializeField] private float _damageOutput;
    [SerializeField] private float _baseAttackPower = 20;
    [SerializeField] private float _weakAtkMultiplier = 1;
    [SerializeField] private float _MedAtkMultiplier = 1.2f;
    [SerializeField] private float _StrongAtkMultiplier = 1.5f;
    [SerializeField] private float _startupTime = 0.5f;
    [SerializeField] private float _activeTime = 1.5f;
    [SerializeField] private float _recoveryTime = 0.7f;

    [SerializeField] private List<GameObject> _hitboxes;

    [Header("Navmesh/Navigation AI")]
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float _IdleSpeed = 1f;
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 30f;
    [SerializeField] private float _pursuitRange = 100f;
    [SerializeField] private float _normalAngularSpeed = 500f;
    [SerializeField] private float _attackAngularSpeed = 250f;

    [Header("Sound")]
    [SerializeField] private AudioSource _audioPlayer;
    [SerializeField] private AudioClip _bearNoise;


    private void Awake()
    {
        _vision = _visionBox.GetComponent<EnemyViewDetection>();
    }

    private void FixedUpdate()
    {
        //if (!isServer) return;

        BehaviourLogic();
    }

    

    private void BehaviourLogic()
    { 
    
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


    private void IdleState()
    {
        if (_seePlayer) TransitionTo(NPCStateEnum.Aggressive);

        _currentSpeed = _IdleSpeed;
    }
    private void ActiveState()
    {
        if (_seePlayer) TransitionTo(NPCStateEnum.Aggressive);

        _currentSpeed = _walkSpeed;
    }
    private void AggressiveState()
    {

        _currentSpeed = _runSpeed;
    }
    private void AttackingState()
    {
        
    }
    private void DeathState()
    {
        //if (!IsServer) return;
        // NetworkObject.Despawn();
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



    //example to trigger hitable, make different script for actual hitbox and on/off from here
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
            {
               // hitable.OnHit(ContactDamage);
            }
        }
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

        if (_targetPlayers == null)
        {
            TransitionTo(NPCStateEnum.Active);
        }
    }

    private void GetPlayerTargetAndChase()
    {
        if (Target != null)
        {
            Agent.SetDestination(Target.position);
        }
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



    //Old methods for references


    private void OldGetPlayerTargetAndChase()
    {
        if (Target == null || Vector3.Distance(transform.position, Target.position) > PursuitRange)
        {
            Transform newTarget = null;

            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (newTarget == null ||
                    Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, newTarget.position))
                {
                    newTarget = player.transform;
                }
            }

            Target = newTarget;
        }

        if (Target != null)
        {
            Agent.SetDestination(Target.position);
        }
    }
}
