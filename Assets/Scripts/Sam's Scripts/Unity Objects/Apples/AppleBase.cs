using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class AppleBase: Item, IThrowable
{
    [Header("Apple Attributes")]
    [SerializeField] private GameObject _prompt;
    // _defaultAppleID should likely be moved to Item and changed to _defaultItemID, but Josh's inventory only tolerates apples and I don't have the time to rewrite it
    [SerializeField] private int _defaultAppleID;
    [SerializeField] private AppleTypeEnum _appleType;
    [SerializeField] private float _damage;

    // I don't like initializing outside of Awake, but took it from Josh and it works so I wont look further
    private readonly NetworkVariable<int> networkAppleID =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    private PlayerInventory player;
    private bool pickupRequested;

    // Unity
    protected void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(collision.gameObject.name); // The number of bounces is low so not checking velocity should be fine
        OnImpact(collision);
    }

    // Josh
    private void Awake()
    {
        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        player = null;
        pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
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
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryPickUp();
        }
    }

    private void UpdatePromptFacing()
    {
        if (_prompt == null ||
            !_prompt.activeSelf)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        _prompt.transform.LookAt(
            mainCamera.transform
        );

        _prompt.transform.Rotate(
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

        if (_prompt != null)
        {
            _prompt.SetActive(false);
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

        if (_prompt != null)
        {
            _prompt.SetActive(true);
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

        if (_prompt != null)
        {
            _prompt.SetActive(false);
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

        _defaultAppleID = newAppleID;
        networkAppleID.Value = newAppleID;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        player = null;
        pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    // Functions
    public virtual void ThrowItem()
    {
        // todo
    }

    public virtual void OnImpact(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
        {
            hitable.OnHit(_damage);
        }

        OnHit(1f);
    }

    // Getters and Setters
    public virtual int AppleID
    {
        get
        {
            return IsSpawned ? networkAppleID.Value : _defaultAppleID;
        }
    }

    public virtual AppleTypeEnum AppleType
    {
        get
        {
            return _appleType;
        }
        set
        {
            _appleType = value;
        }
    }

    public virtual float Damage
    {
        get
        {
            return _damage;
        }
        set
        {
            _damage = value;
        }
    }
}
