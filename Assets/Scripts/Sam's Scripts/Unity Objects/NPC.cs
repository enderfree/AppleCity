using UnityEngine;
using UnityEngine.AI;

public class NPC: Character
{
    [Header("NPC Fields")]
    [SerializeField] private NPCStateEnum _npcState;
    [SerializeField] private Transform _target;
    [SerializeField] private NavMeshAgent _agent;

    // Getters and Setters
    public virtual NPCStateEnum NPCState
    {
        get
        {
            return _npcState;
        }
        set
        {
            _npcState = value;
        }
    }

    public virtual Transform Target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
        }
    }

    public virtual NavMeshAgent Agent
    {
        get
        {
            return _agent;
        }
        set
        {
            _agent = value;
        }
    }
}
