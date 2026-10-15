using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Joshua's inventory code. I made it work, but I don't think others did so we're just using an apple list in the end. 
/// There are a lot of things to fix in there too, but it was working with my patches, we just didn't have time to teach it's quirks to everyone. 
/// </summary>
public class PlayerInventory : NetworkBehaviour
{
    // technically, this is the inventory, it holds the apple IDs used by the server
    private readonly List<int> _apples = new List<int>();

    // I'm not sure why he has an accessor for that, but I saw this in other's code too
    public int AppleCount => _apples.Count;

    /// <summary>
    ///Get if the player trying to pick up an apple is allowed to
    /// </summary>
    /// <param name="apple">the apple being picked up</param>
    /// <returns> If the player trying to pick up an apple is allowed to</returns>
    public bool PickUp(AppleBase apple)
    {
        if (!IsOwner)
        {
            Debug.LogWarning(
                "PlayerInventory: Only the owning player can request a pickup."
            );

            return false;
        }

        if (apple == null)
        {
            Debug.LogWarning(
                "PlayerInventory: Tried to pick up a null Apple."
            );

            return false;
        }

        NetworkObject appleNetworkObject = apple.NetworkObject;

        if (appleNetworkObject == null)
        {
            Debug.LogError(
                $"PlayerInventory: {apple.name} has no NetworkObject."
            );

            return false;
        }

        /*
         * Extremely important.
         *
         * A NetworkObjectReference can only be created from an object
         * that NGO currently considers spawned.
         */
        if (!appleNetworkObject.IsSpawned)
        {
            Debug.LogError(
                $"APPLE PICKUP FAILED: {apple.name} is not network-spawned. " +
                $"NetworkObjectId = {appleNetworkObject.NetworkObjectId}. " +
                "Make sure the dedicated server and client are using the " +
                "same current scene/build."
            );

            return false;
        }

        Debug.Log(
            $"Requesting pickup: {apple.name} | " +
            $"AppleID={apple.AppleID} | " +
            $"NetworkObjectId={appleNetworkObject.NetworkObjectId}"
        );

        NetworkObjectReference appleReference =
            new NetworkObjectReference(appleNetworkObject);

        PickUpAppleRpc(appleReference);

        return true;
    }

    /// <summary>
    /// If the apple can be picked up, make sure the server aggrees
    /// </summary>
    /// <param name="appleReference">a network reference to the apple</param>
    [Rpc(SendTo.Server)]
    private void PickUpAppleRpc(
        NetworkObjectReference appleReference)
    {
        /*
         * Resolve the network reference on the SERVER.
         */
        if (!appleReference.TryGet(
                out NetworkObject appleNetworkObject))
        {
            Debug.LogWarning(
                "SERVER: Apple pickup failed because the " +
                "NetworkObjectReference could not be resolved."
            );

            return;
        }

        if (!appleNetworkObject.IsSpawned)
        {
            Debug.LogWarning(
                "SERVER: Apple pickup failed because the apple " +
                "is no longer spawned."
            );

            return;
        }

        AppleBase apple =
            appleNetworkObject.GetComponent<AppleBase>();

        if (apple == null)
        {
            Debug.LogWarning(
                "SERVER: NetworkObject does not contain an Apple component."
            );

            return;
        }

        int collectedAppleID = apple.AppleID;

        Debug.Log(
            $"SERVER: Picking up {apple.name} | " +
            $"AppleID={collectedAppleID} | " +
            $"NetworkObjectId={appleNetworkObject.NetworkObjectId}"
        );

        /*
         * Tell the owner that the pickup succeeded.
         *
         * We do this instead of adding it locally before the
         * server has validated the apple.
         */
        ConfirmApplePickupRpc(collectedAppleID);

        /*
         * The SERVER despawns the apple.
         *
         * NGO will then remove it for all connected clients.
         */
        appleNetworkObject.Despawn();
    }

    /// <summary>
    /// Confirm the apple was properly picked up through the server
    /// </summary>
    /// <param name="appleID">the id of the picked apple</param>
    [Rpc(SendTo.Owner)]
    private void ConfirmApplePickupRpc(int appleID)
    {
        _apples.Add(appleID);

        Debug.Log(
            $"Apple collected successfully! " +
            $"Apple ID: {appleID} | " +
            $"Apples: {_apples.Count}"
        );
    }
}