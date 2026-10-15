using UnityEngine;

/// <summary>
/// Zone where players must remain to ready
/// </summary>
public class HubDepartureZone : MonoBehaviour
{
    /// <summary>
    /// register that the player is in the departure zone if they are what collided
    /// </summary>
    /// <param name="other">what collided with this</param>
    private void OnTriggerEnter(
        Collider other)
    {
        HubDeparturePlayer player =
            other.GetComponentInParent<HubDeparturePlayer>();

        if (player == null)
        {
            return;
        }

        if (!player.IsOwner)
        {
            return;
        }

        player.EnterDepartureZone();
    }

    /// <summary>
    /// register that the player is no longer in the departure zone if they are what no longer touch the collider
    /// </summary>
    /// <param name="other">what no longer touches this</param>
    private void OnTriggerExit(
        Collider other)
    {
        HubDeparturePlayer player =
            other.GetComponentInParent<HubDeparturePlayer>();

        if (player == null)
        {
            return;
        }

        if (!player.IsOwner)
        {
            return;
        }

        player.ExitDepartureZone();
    }
}