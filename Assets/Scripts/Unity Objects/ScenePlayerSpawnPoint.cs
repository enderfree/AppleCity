using UnityEngine;

public class ScenePlayerSpawnPoint : MonoBehaviour
{
    public static ScenePlayerSpawnPoint Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            0.5f
        );

        Gizmos.DrawRay(
            transform.position,
            transform.forward * 2f
        );
    }
}