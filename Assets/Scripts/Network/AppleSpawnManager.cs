using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Code that spawns the apples for the network... NGO has its own defenition of spawn...
/// NB. This is one of Josh's scripts. I modified it a bit to be able to make it work with our new apple, 
/// but I can't pretend I understand it at 100%. 
/// </summary>
public class AppleSpawnManager : MonoBehaviour
{
    [Header("Apple")]
    // NGO accepts NetworkBehavior as a prefab...
    [SerializeField]
    private AppleBase _applePrefab;

    [Header("Spawn Points")]
    // the places where apple spawns. The location itself is marked by a GameObjects.
    [SerializeField]
    private Transform _spawnPointsRoot;

    [Header("Startup")]
    // A small wait time before spawning the apples... I am not sure why and Joshua is not here to explain it to me...
    [SerializeField]
    private float _initialSpawnDelay = 0.25f;

    // If the apple has spawned or not... I'm pretty sure this should not be global, but I don't have time to reformat everything and test rn...
    private bool _hasSpawned;

    /// <summary>
    /// MonoBehavior's Start()? I think... Why is it IEnumerator?
    /// </summary>
    /// <returns>Innit() to take the initialization out of Start()</returns>
    private IEnumerator Start()
    {
        return Innit();
    }

    /// <summary>
    /// ok... I'm not exactly sure what's happening in there, but from Josh's comments, 
    /// I believes it waits for NGO before checking if this is server or client and then make apples?
    /// </summary>
    /// <returns>a delay</returns>
    private IEnumerator Innit()
    {
        /*
         * Wait until NGO is actually running.
         */
        while (NetworkManager.Singleton == null ||
               !NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }

        /*
         * Only the dedicated server creates apples.
         *
         * Clients receive them automatically through NGO.
         */
        if (!NetworkManager.Singleton.IsServer)
        {
            yield break;
        }

        if (_initialSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(
                _initialSpawnDelay
            );
        }

        SpawnApples();
    }

    /// <summary>
    /// Spawns apples at the spawn points
    /// </summary>
    private void SpawnApples()
    {
        if (_hasSpawned)
        {
            return;
        }

        if (_applePrefab == null)
        {
            Debug.LogError(
                "AppleSpawnManager: Apple prefab is not assigned."
            );

            return;
        }

        if (_spawnPointsRoot == null)
        {
            Debug.LogError(
                "AppleSpawnManager: Spawn Points Root is not assigned."
            );

            return;
        }

        AppleSpawnPoint[] spawnPoints =
            _spawnPointsRoot.GetComponentsInChildren<AppleSpawnPoint>(
                true
            );

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "AppleSpawnManager: No AppleSpawnPoints found."
            );

            return;
        }

        _hasSpawned = true;

        foreach (AppleSpawnPoint spawnPoint in spawnPoints)
        {
            SpawnApple(spawnPoint);
        }

        Debug.Log(
            $"SERVER: Spawned {spawnPoints.Length} apples."
        );
    }

    /// <summary>
    /// This is called in a loop by SpawnApples to spawn singular apples where they belong. 
    /// </summary>
    /// <param name="spawnPoint">The GameObject thet will carry the apple</param>
    private void SpawnApple(
        AppleSpawnPoint spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        AppleBase apple =
            Instantiate(
                _applePrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );

        if (apple == null)
        {
            return;
        }

        NetworkObject networkObject =
            apple.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                "AppleSpawnManager: Apple prefab has no NetworkObject.",
                apple
            );

            Destroy(apple.gameObject);
            return;
        }

        /*
         * Configure server-side data before spawning.
         */
        apple.ConfigureServer(
            spawnPoint.AppleID
        );

        /*
         * true = destroy this NetworkObject when
         * AppleForest unloads.
         *
         * Returning to AppleForest later creates a new
         * fresh set of apples.
         */
        networkObject.Spawn(true);

        Debug.Log(
            $"SERVER: Spawned Apple ID {spawnPoint.AppleID} | " +
            $"NetworkObjectId={networkObject.NetworkObjectId}"
        );
    }
}