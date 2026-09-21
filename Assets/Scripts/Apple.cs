using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Apple : NetworkBehaviour
{
    [Header("Apple")]
    [SerializeField]
    private int defaultAppleID;

    [Header("Interaction")]
    [SerializeField]
    private GameObject prompt;

    private readonly NetworkVariable<int> networkAppleID =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private PlayerInventory player;
    private bool pickupRequested;

    public int AppleID =>
        IsSpawned
            ? networkAppleID.Value
            : defaultAppleID;

    private void Awake()
    {
        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        player = null;
        pickupRequested = false;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsSpawned)
        {
            return;
        }

        if (player == null ||
            pickupRequested)
        {
            return;
        }

        UpdatePromptFacing();

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickUp();
        }
    }

    private void UpdatePromptFacing()
    {
        if (prompt == null ||
            !prompt.activeSelf)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        prompt.transform.LookAt(
            mainCamera.transform
        );

        prompt.transform.Rotate(
            0f,
            180f,
            0f
        );
    }

    private void TryPickUp()
    {
        if (player == null)
        {
            return;
        }

        if (!IsSpawned)
        {
            Debug.LogWarning(
                $"Apple '{name}' cannot be picked up " +
                "because it is not network-spawned."
            );

            return;
        }

        bool requestSent =
            player.PickUp(this);

        if (!requestSent)
        {
            return;
        }

        pickupRequested = true;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpawned)
        {
            return;
        }

        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null ||
            !inventory.IsOwner)
        {
            return;
        }

        player = inventory;
        pickupRequested = false;

        if (prompt != null)
        {
            prompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null ||
            inventory != player)
        {
            return;
        }

        player = null;
        pickupRequested = false;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public void ConfigureServer(int newAppleID)
    {
        /*
         * This can be called BEFORE NetworkObject.Spawn(),
         * so don't rely on this Apple already being spawned.
         */
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning(
                $"Apple.ConfigureServer({newAppleID}) " +
                "was called by something other than the server."
            );

            return;
        }

        defaultAppleID = newAppleID;
        networkAppleID.Value = newAppleID;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        player = null;
        pickupRequested = false;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }
}