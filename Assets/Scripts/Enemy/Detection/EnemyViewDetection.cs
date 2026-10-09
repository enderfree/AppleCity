using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyViewDetection : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _getPlayerList = new List<GameObject>();
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private GameObject _head;
    [SerializeField] private NetworkVariable<bool> _seePlayer = new NetworkVariable<bool>(false);
    public bool _readSawPlayer => _seePlayer.Value;



    private void OnTriggerEnter(Collider col)
    {
        if (!IsServer) return;

        GetPlayerToList(col);
    }

    private void OnTriggerExit(Collider col)
    {
        if (!IsServer) return;

        RemovePlayerToList(col);
    }

    

    private void OnTriggerStay(Collider col)
    {
        if (!IsServer) return;

        DetectPlayer();
    }

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
