using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class EnemySearchDetection : NetworkBehaviour
{
    [SerializeField] private float _activeTime =2f;
    [SerializeField] private List<GameObject> _getPlayerList = new List<GameObject>();
    [SerializeField] private LayerMask _hitLayer;

    //ref
    private EnemyBehavior _behavior;


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

    private void OnEnable()
    {
        ActiveTime();
    }

    private void OnDisable()
    {
        GetPlayerList.Clear();
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

    private async void ActiveTime()
    {
        await Delay(_activeTime);

        //Send player list to Behavior and deactivate self
        _behavior.SearchPlayerList(GetPlayerList);
        gameObject.SetActive(false);
    }

    private async Task Delay(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }

    //Get Set
    public virtual List<GameObject> GetPlayerList
    {

        get{ return _getPlayerList; }
        set{ _getPlayerList = value; }
    }
}
