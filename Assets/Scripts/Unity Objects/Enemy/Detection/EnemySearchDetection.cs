using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class EnemySearchDetection : NetworkBehaviour
{
    // how long is this active
    [SerializeField] private float _activeTime =1f;

    // player list
    [SerializeField] private List<GameObject> _getPlayerList = new List<GameObject>();

    // layer being looked for
    [SerializeField] private LayerMask _hitLayer;

    //ref
    // enemy logic
    [SerializeField] private EnemyBehavior _behavior;
    

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
    /// OnEnable() set how long it is enabled for
    /// </summary>
    private void OnEnable()
    {
        ActiveTime();
    }

    /// <summary>
    /// OnDisable(), clear the player list
    /// </summary>
    private void OnDisable()
    {
        _getPlayerList.Clear();
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
            if (col.gameObject.CompareTag("Player"))
            {
                var list = new List<GameObject>(_getPlayerList);

                list.Remove(col.gameObject);
                _getPlayerList = list;
            }
    }

    /// <summary>
    /// set the time the enemy is looking for players
    /// </summary>
    private async void ActiveTime()
    {
        await Delay(_activeTime);

        //Send player list to Behavior and deactivate self
        _behavior.SearchPlayerList(new List<GameObject>(_getPlayerList));
        Debug.Log("Search player send result");

        await Delay(0.05f);
         gameObject.SetActive(false);
    }

    /// <summary>
    /// wait
    /// </summary>
    /// <param name="sec">nb of seconds to wait for</param>
    /// <returns></returns>
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
