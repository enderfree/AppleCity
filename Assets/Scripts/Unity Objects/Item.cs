using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles what all items have in common. Right now we only have apples, but the plan was to have upgrades too
/// </summary>
public class Item: NetworkBehaviour, IHitable
{
    [Header("Item Attributes")]
    // name of the item
    [SerializeField] private string _itemName;

    // type of the item
    [SerializeField] private ItemTypeEnum _itemType;

    // description of the item
    [SerializeField] private string _description;

    // weigth of the item
    [SerializeField] private float _weigth;

    // max hp of the item
    [SerializeField] private float _maxDurability;

    // current hp of the item
    [SerializeField] private float _durability;

    // Interface member
    // XML comment is usually directly provided in the interface itself
    /// <summary>
    /// What happen when the item takes a hit
    /// </summary>
    /// <param name="hit">how much damage does the item take</param>
    public virtual void OnHit(float hit)
    {
        Durability -= hit;

        if (Durability <= 0)
        {
            Break();
        }
    }

    // Functions

    /// <summary>
    /// Destroy this, and its frioritures
    /// </summary>
    public virtual void Break()
    {
        // we may want to put sound and extra graphics
        Destroy(gameObject);
    }

    // Getters and Setters
    /// <summary>
    /// just settuping my observers in case I need them later
    /// </summary>
    public virtual string ItemName
    {
        get
        {
            return _itemName;
        }
        set
        {
            _itemName = value;
        }
    }

    public virtual ItemTypeEnum ItemType
    {
        get
        {
            return _itemType;
        }
        set
        {
            _itemType = value;
        }
    }

    public virtual string Description
    {
        get
        {
            return _description;
        }
        set
        {
            _description = value;
        }
    }

    public virtual float Weigth
    {
        get
        {
            return _weigth;
        }
        set
        {
            _weigth = value;
        }
    }

    public virtual float MaxDurability
    {
        get
        {
            return _maxDurability;
        }
        set
        {
            _maxDurability = value;
        }
    }

    public virtual float Durability
    {
        get
        {
            return _durability;
        }
        set
        {
            _durability = value;
        }
    }
}
