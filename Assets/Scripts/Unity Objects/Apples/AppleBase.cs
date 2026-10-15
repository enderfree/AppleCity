using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// the default apple. Used to be called Apple, but we had 3 of those so I renamed mine and in the end, this is the one we ended up with...
/// I didn't dare to rename it in case we still have old refs lying around that could cause a problem
/// </summary>
public class AppleBase: Item, IThrowable
{
    [Header("Apple Attributes")]
    // the pickup prompt
    [SerializeField] private GameObject _prompt;

    // _defaultAppleID should likely be moved to Item and changed to _defaultItemID, but Josh's inventory only tolerates apples and I don't have the time to rewrite it
    [SerializeField] private int _defaultAppleID;

    // the type of apple (this class was made to be overriden by all types of apple)
    [SerializeField] private AppleTypeEnum _appleType;

    // how badly does it shoo the doctor
    [SerializeField] private float _damage;

    // things the apple doesn't touch
    [SerializeField] private List<LayerMask> _contactExclusionLayers;

    // the apple's rigid body
    private Rigidbody _rb;

    // I don't like initializing outside of Awake, but took it from Josh and it works so I wont look further
    private readonly NetworkVariable<int> networkAppleID =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // the player's inventory
    private PlayerInventory _player;

    // if someone is trying to pickup this apple or not
    private bool _pickupRequested;

    // Unity
    /// <summary>
    /// what happens when this apple collides with something
    /// </summary>
    /// <param name="collision">the thing it's colliding with</param>
    protected void OnCollisionEnter(Collision collision)
    {
        OnImpact(collision);
    }

    // Josh
    /// <summary>
    /// Initialize on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// get rigid body and disable the prompt
    /// </summary>
    private void Innit()
    {
        _rb = GetComponent<Rigidbody>();

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    /// <summary>
    /// Initialize network things
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _player = null;
        _pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    /// <summary>
    /// I really was not kidding when I said that inputs were all over the place
    /// </summary>
    private void Update()
    {
        CheckInputs();
    }

    /// <summary>
    /// Handle player's attempts at picking apples
    /// </summary>
    private void CheckInputs()
    {
        if (!IsSpawned)
        {
            return;
        }

        if (_player == null ||
            _pickupRequested)
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

    /// <summary>
    /// Make prompts always face players
    /// </summary>
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

    /// <summary>
    /// Pickup the apple
    /// </summary>
    private void TryPickUp()
    {
        if (_player == null)
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
            _player.PickUp(this);

        if (!requestSent)
        {
            return;
        }

        _pickupRequested = true;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    /// <summary>
    /// I'm not following that one
    /// </summary>
    /// <param name="other">the thing triggering the apple</param>
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

        _player = inventory;
        _pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(true);
        }
    }

    /// <summary>
    /// I am still confused
    /// </summary>
    /// <param name="other">the thing no longer triggering the apple</param>
    private void OnTriggerExit(Collider other)
    {
        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null ||
            inventory != _player)
        {
            return;
        }

        _player = null;
        _pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    /// <summary>
    /// configure the apple on the server
    /// </summary>
    /// <param name="newAppleID">server id for the apple</param>
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

    /// <summary>
    /// handles the dispawning of apples
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _player = null;
        _pickupRequested = false;

        if (_prompt != null)
        {
            _prompt.SetActive(false);
        }
    }

    // Functions
    /// <summary>
    /// The thrown mechanic was supposed to be handled in the apple, but due to time constraints, it is currently in player
    /// This would have let us do apples that are thrown weird
    /// </summary>
    public virtual void ThrowItem()
    {
        // todo
    }

    /// <summary>
    /// What happens when the âpple gets hit
    /// </summary>
    /// <param name="collision">what hit the apple</param>
    public virtual void OnImpact(Collision collision)
    {
        if (_contactExclusionLayers.Contains(LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer))))
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        else
        {
            if (collision.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
            {
                hitable.OnHit(_damage);
            }

            OnHit(1f);
        }
    }

    // Getters and Setters
    /// <summary>
    /// Handles the apple id
    /// </summary>
    public virtual int AppleID
    {
        get
        {
            return IsSpawned ? networkAppleID.Value : _defaultAppleID;
        }
    }

    /// <summary>
    /// just settuping my observers in case I need them later
    /// </summary>
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
