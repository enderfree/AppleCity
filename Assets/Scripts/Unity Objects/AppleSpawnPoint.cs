using UnityEngine;

/// <summary>
/// This is where te server spawns apples
/// </summary>
public class AppleSpawnPoint : MonoBehaviour
{
    [Header("Apple")]
    // I have no idea why we have an apple id here
    [SerializeField]
    private int _appleID = 1;

    /// <summary>
    /// Original getter?
    /// </summary>
    public int AppleID => _appleID;

    /// <summary>
    /// Show the spawn point in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            0.25f
        );

        Gizmos.DrawRay(
            transform.position,
            transform.up * 0.75f
        );
    }
}