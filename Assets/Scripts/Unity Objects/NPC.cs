using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Things common to all non-players
/// </summary>
public class NPC: Character
{
    [Header("NPC Fields")]
    // state of the npc
    [SerializeField] private NPCStateEnum _npcState;

    // NavMeshAgent target
    [SerializeField] private Transform _target;

    // NavMeshAgent
    [SerializeField] private NavMeshAgent _agent;

    // Getters and Setters
    /// <summary>
    /// just settuping my observers in case I need them later
    /// </summary>
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
