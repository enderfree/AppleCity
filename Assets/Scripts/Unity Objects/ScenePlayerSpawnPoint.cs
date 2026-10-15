using UnityEngine;

/// <summary>
/// GameObject representing the player spawn position
/// </summary>
public class ScenePlayerSpawnPoint : MonoBehaviour
{
    /// <summary>
    /// static reference to this
    /// </summary>
    public static ScenePlayerSpawnPoint Instance { get; private set; }

    /// <summary>
    /// Initialize on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// Initialize the instance
    /// </summary>
    private void Innit()
    {
        Instance = this;
    }

    /// <summary>
    /// if this is destroyed, we no longuer need the instance
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// make this visible in the editor
    /// </summary>
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