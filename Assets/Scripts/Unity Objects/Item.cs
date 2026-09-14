using UnityEngine;

public class Item: MonoBehaviour, IHitable
{
    [Header("Item Attributes")]
    [SerializeField] private string itemName;
    [SerializeField] private ItemTypeEnum itemType;
    [SerializeField] private string description;
    [SerializeField] private float maxDurability;
    [SerializeField] private float durability;

    // Interface member
    // XML comment is usually directly provided in the interface itself
    public virtual void OnBeforeSerialize()
    {
        // needed
    }

    public virtual void OnAfterDeserialize()
    {
        // needed
    }

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
            return itemName;
        } 
        set
        {
            itemName = value;
        }
    }

    public virtual ItemTypeEnum ItemType
    {
        get
        {
            return itemType;
        }
        set
        {
            itemType = value;
        }
    }

    public virtual string Description
    {
        get
        {
            return description;
        }
        set
        {
            description = value;
        }
    }

    public virtual float MaxDurability
    {
        get
        {
            return maxDurability;
        }
        set
        {
            maxDurability = value;
        }
    }

    public virtual float Durability
    {
        get
        {
            return durability;
        }
        set
        {
            durability = value;
        }
    }

}
