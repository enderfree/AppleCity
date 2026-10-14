using UnityEngine;

public class HubDepartureZone : MonoBehaviour
{
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