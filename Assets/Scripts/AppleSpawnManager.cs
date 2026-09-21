using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AppleSpawnManager : MonoBehaviour
{
    [Header("Apple")]
    [SerializeField]
    private Apple applePrefab;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform spawnPointsRoot;

    [Header("Startup")]
    [SerializeField]
    private float initialSpawnDelay = 0.25f;

    private bool hasSpawned;

    private IEnumerator Start()
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

        if (initialSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(
                initialSpawnDelay
            );
        }

        SpawnApples();
    }

    private void SpawnApples()
    {
        if (hasSpawned)
        {
            return;
        }

        if (applePrefab == null)
        {
            Debug.LogError(
                "AppleSpawnManager: Apple prefab is not assigned."
            );

            return;
        }

        if (spawnPointsRoot == null)
        {
            Debug.LogError(
                "AppleSpawnManager: Spawn Points Root is not assigned."
            );

            return;
        }

        AppleSpawnPoint[] spawnPoints =
            spawnPointsRoot.GetComponentsInChildren<AppleSpawnPoint>(
                true
            );

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "AppleSpawnManager: No AppleSpawnPoints found."
            );

            return;
        }

        hasSpawned = true;

        foreach (AppleSpawnPoint spawnPoint in spawnPoints)
        {
            SpawnApple(spawnPoint);
        }

        Debug.Log(
            $"SERVER: Spawned {spawnPoints.Length} apples."
        );
    }

    private void SpawnApple(
        AppleSpawnPoint spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        Apple apple =
            Instantiate(
                applePrefab,
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