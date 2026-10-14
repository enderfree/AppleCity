using UnityEngine;

public class AppleSpawnPoint : MonoBehaviour
{
    [Header("Apple")]
    [SerializeField]
    private int appleID = 1;

    public int AppleID => appleID;

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