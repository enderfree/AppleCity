using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is the manager managing spawn points
/// </summary>
public class ScenePlayerSpawner : MonoBehaviour
{
    [Header("Spawn")]
    // this is the GameObject marking spawn points
    [SerializeField]
    private ScenePlayerSpawnPoint _spawnPoint;

    // players will spawn a bit above the spawn point by this amount
    [SerializeField]
    private float _spawnHeightOffset = 0.5f;

    /// <summary>
    /// Monobehavior's start... but again, IEnumerator?
    /// </summary>
    /// <returns>Innit() to get that stuff out of start()</returns>
    private IEnumerator Start()
    {
        return Innit();
    }

    private IEnumerator Innit()
    {
        /*
         * The dedicated server itself doesn't have a local player.
         * The owner-authoritative NetworkTransform will sync the
         * client's new position back through NGO.
         */
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient)
        {
            yield break;
        }

        /*
         * During a network scene transition the scene can finish
         * loading slightly before our local PlayerObject is ready.
         *
         * Wait for it instead of assuming it already exists.
         */
        while (NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        NetworkObject playerObject =
            NetworkManager.Singleton.LocalClient.PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError(
                "ScenePlayerSpawner: Local PlayerObject was not found."
            );

            yield break;
        }

        if (!playerObject.IsOwner)
        {
            yield break;
        }

        if (_spawnPoint == null)
        {
            _spawnPoint =
                FindFirstObjectByType<ScenePlayerSpawnPoint>();
        }

        if (_spawnPoint == null)
        {
            Debug.LogError(
                "ScenePlayerSpawner: No ScenePlayerSpawnPoint found."
            );

            yield break;
        }

        Transform playerTransform =
            playerObject.transform;

        Vector3 destination =
            _spawnPoint.transform.position +
            Vector3.up * _spawnHeightOffset;

        /*
         * If the player uses a CharacterController,
         * disable it briefly while teleporting.
         */
        CharacterController controller =
            playerObject.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        playerTransform.SetPositionAndRotation(
            destination,
            _spawnPoint.transform.rotation
        );

        if (TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats._respawnPoint = destination;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log(
            $"LOCAL PLAYER: Spawned at AppleForest spawn point. " +
            $"Position = {destination}"
        );
    }
}