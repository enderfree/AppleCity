using UnityEngine;

public class NPC: Character
{
    [Header("NPC Fields")]
    [SerializeField] private NPCStateEnum _npcState;
    [SerializeField] private Vector3 _target;

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

    public virtual Vector3 Target
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
}
