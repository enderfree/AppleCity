using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Detection logic of the enemy when not searching
/// </summary>
public class EnemyViewDetection : NetworkBehaviour
{
    // list of players
    [SerializeField] private List<GameObject> _getPlayerList = new List<GameObject>();

    // detection layer
    [SerializeField] private LayerMask _hitLayer;

    // enemy head
    [SerializeField] private GameObject _head;

    // if the enemy sees the player
    [SerializeField] private NetworkVariable<bool> _seePlayer = new NetworkVariable<bool>(false);

    // if the agent saw the player
    public bool _readSawPlayer => _seePlayer.Value;

    //ref
    // the enemy logic
    [SerializeField] private EnemyBehavior _behaviour;

    /// <summary>
    /// what happens on trigger
    /// </summary>
    /// <param name="col">the collision object triggering</param>
    private void OnTriggerEnter(Collider col)
    {
        if (!IsServer) return;

        GetPlayerToList(col);
    }

    /// <summary>
    /// what happens when no longer triggering
    /// </summary>
    /// <param name="col">the collision object no longer triggering</param>
    private void OnTriggerExit(Collider col)
    {
        if (!IsServer) return;

        RemovePlayerToList(col);
    }

    
    /// <summary>
    /// detect players when they are triggering you
    /// </summary>
    /// <param name="col">the collision triggering you</param>
    private void OnTriggerStay(Collider col)
    {
        if (!IsServer) return;

        DetectPlayer();
    }

    /// <summary>
    /// player detection logic
    /// </summary>
    public void DetectPlayer()
    {
        if (_seePlayer.Value) return;
        if (_getPlayerList == null) return;
        var players = new List<GameObject>(_getPlayerList);

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player = players[i];
            if (player == null) continue;

            Vector3 target = player.GetComponent<Collider>().bounds.center;
            Vector3 direction = (target - _head.transform.position).normalized;
            float distance = Vector3.Distance(_head.transform.position, target);

            if (Physics.Raycast(_head.transform.position, direction, out RaycastHit hit, distance, _hitLayer))
            {
                if (hit.collider.gameObject == player)
                {
                    
                    Debug.Log("saw the player ");
                    _seePlayer.Value = true;
                    _behaviour.AddTargetOnSight(player);

                }
                else
                {
                    if (!_seePlayer.Value) return;
                    Debug.Log("saw no one ");
                    _seePlayer.Value = false;
                }
            }

            Debug.DrawLine(_head.transform.position, target, Color.red);
        }
    }

    /// <summary>
    /// add new players to list
    /// </summary>
    /// <param name="col">collision object</param>
    private void GetPlayerToList(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            var list = new List<GameObject>(_getPlayerList);

            if (!list.Contains(col.gameObject))
            {
                list.Add(col.gameObject);
                _getPlayerList = list;
            }
        }
    }

    /// <summary>
    /// remove player from list
    /// </summary>
    /// <param name="col">collision object</param>
    private void RemovePlayerToList(Collider col)
    {
        _seePlayer.Value = false;
        if (col.gameObject.CompareTag("Player"))
        {
            if (col.gameObject.CompareTag("Player"))
            {
                var list = new List<GameObject>(_getPlayerList);

                list.Remove(col.gameObject);
                _getPlayerList = list;
            }
        }
    }


    //Get Set
    public virtual List<GameObject> GetPlayerList
    {
        get{ return _getPlayerList; }
        set{ _getPlayerList = value; }
    }
}
