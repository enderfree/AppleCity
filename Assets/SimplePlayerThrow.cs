using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerThrow : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Apple applePrefab;
    [SerializeField] private float throwForce = 10f;

    private PlayerInventory inventory;

    private int pendingAppleID = -1;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartThrow();
        }
    }

    private void StartThrow()
    {
        if (pendingAppleID != -1)
            return;

        if (!inventory.TakeApple(out int appleID))
            return;

        pendingAppleID = appleID;

        PlayThrowRpc(appleID);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayThrowRpc(int appleID)
    {
        animator.SetInteger("ThrowType", appleID);
        animator.SetTrigger("Throw");
    }

    public void ReleaseApple()
    {
        if (!IsOwner || pendingAppleID == -1)
            return;

        ThrowAppleRpc(
            pendingAppleID,
            throwPoint.position,
            transform.forward
        );

        pendingAppleID = -1;
    }

    [Rpc(SendTo.Server)]
    private void ThrowAppleRpc(
        int appleID,
        Vector3 position,
        Vector3 direction)
    {
        Apple apple =
            Instantiate(
                applePrefab,
                position,
                Quaternion.identity
            );

        apple.ConfigureServer(appleID);

        Rigidbody rigidbody =
            apple.GetComponent<Rigidbody>();

        rigidbody.isKinematic = false;

        apple.NetworkObject.Spawn(true);

        rigidbody.AddForce(
            direction * throwForce,
            ForceMode.Impulse
        );
    }
}