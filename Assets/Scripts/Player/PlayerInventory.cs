using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    private readonly List<int> apples = new List<int>();

    public int AppleCount => apples.Count;

    public bool PickUp(Apple apple)
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

        Apple apple =
            appleNetworkObject.GetComponent<Apple>();

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

    [Rpc(SendTo.Owner)]
    private void ConfirmApplePickupRpc(int appleID)
    {
        apples.Add(appleID);

        Debug.Log(
            $"Apple collected successfully! " +
            $"Apple ID: {appleID} | " +
            $"Apples: {apples.Count}"
        );
    }
}