using UnityEngine;

public class Item: MonoBehaviour, IHitable
{
    [Header("Item Attributes")]
    [SerializeField] private string _itemName;
    [SerializeField] private ItemTypeEnum _itemType;
    [SerializeField] private string _description;
    [SerializeField] private float _weigth;
    [SerializeField] private float _maxDurability;
    [SerializeField] private float _durability;

    // Interface member
    // XML comment is usually directly provided in the interface itself
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
