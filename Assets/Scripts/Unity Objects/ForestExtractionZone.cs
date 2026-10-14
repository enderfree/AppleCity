using UnityEngine;

public class ForestExtractionZone : MonoBehaviour
{
    private HubDeparturePlayer localPlayer;

    private void OnTriggerEnter(Collider other)
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

        localPlayer = player;

        player.EnterDepartureZone();

        Debug.Log(
            "PRIMARY EXTRACTION: " +
            "Player entered extraction zone."
        );
    }

    private void OnTriggerStay(Collider other)
    {
        if (localPlayer != null)
        {
            return;
        }

        HubDeparturePlayer player =
            other.GetComponentInParent<HubDeparturePlayer>();

        if (player == null ||
            !player.IsOwner)
        {
            return;
        }

        localPlayer = player;

        player.EnterDepartureZone();
    }

    private void OnTriggerExit(Collider other)
    {
        HubDeparturePlayer player =
            other.GetComponentInParent<HubDeparturePlayer>();

        if (player == null ||
            !player.IsOwner)
        {
            return;
        }

        if (player != localPlayer)
        {
            return;
        }

        player.ExitDepartureZone();

        localPlayer = null;

        Debug.Log(
            "PRIMARY EXTRACTION: " +
            "Player left extraction zone."
        );
    }

    private void OnDisable()
    {
        if (localPlayer != null)
        {
            localPlayer.ExitDepartureZone();
        }

        localPlayer = null;

        InteractionTooltipUI.Instance?.Hide();
    }
}