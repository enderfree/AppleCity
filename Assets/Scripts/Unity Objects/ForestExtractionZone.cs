using UnityEngine;

/// <summary>
/// This is the zone where characters must remain to ready
/// </summary>
public class ForestExtractionZone : MonoBehaviour
{
    // the local player's departure stats
    private HubDeparturePlayer _localPlayer;

    /// <summary>
    /// register that the player is in the departure zone if they are what collided
    /// </summary>
    /// <param name="other">what collided with this</param>
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

        _localPlayer = player;

        player.EnterDepartureZone();

        Debug.Log(
            "PRIMARY EXTRACTION: " +
            "Player entered extraction zone."
        );
    }

    /// <summary>
    /// I'm actually not sure why this spams EnterDepartureZone and I can't ask Josh...
    /// </summary>
    /// <param name="other">what is colliding with this</param>
    private void OnTriggerStay(Collider other)
    {
        if (_localPlayer != null)
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

        _localPlayer = player;

        player.EnterDepartureZone();
    }

    /// <summary>
    /// register that the player is no longer in the departure zone if they are what no longer touch the collider
    /// </summary>
    /// <param name="other">what no longer touches this</param>
    private void OnTriggerExit(Collider other)
    {
        HubDeparturePlayer player =
            other.GetComponentInParent<HubDeparturePlayer>();

        if (player == null ||
            !player.IsOwner)
        {
            return;
        }

        if (player != _localPlayer)
        {
            return;
        }

        player.ExitDepartureZone();

        _localPlayer = null;

        Debug.Log(
            "PRIMARY EXTRACTION: " +
            "Player left extraction zone."
        );
    }

    /// <summary>
    /// Makes so players are not in the departure zone if there are no more departure zone
    /// </summary>
    private void OnDisable()
    {
        if (_localPlayer != null)
        {
            _localPlayer.ExitDepartureZone();
        }

        _localPlayer = null;

        InteractionTooltipUI.Instance?.Hide();
    }
}