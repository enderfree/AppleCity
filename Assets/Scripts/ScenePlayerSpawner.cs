using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ScenePlayerSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField]
    private ScenePlayerSpawnPoint spawnPoint;

    [SerializeField]
    private float spawnHeightOffset = 0.5f;

    private IEnumerator Start()
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

        if (spawnPoint == null)
        {
            spawnPoint =
                FindFirstObjectByType<ScenePlayerSpawnPoint>();
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "ScenePlayerSpawner: No ScenePlayerSpawnPoint found."
            );

            yield break;
        }

        Transform playerTransform =
            playerObject.transform;

        Vector3 destination =
            spawnPoint.transform.position +
            Vector3.up * spawnHeightOffset;

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
            spawnPoint.transform.rotation
        );

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