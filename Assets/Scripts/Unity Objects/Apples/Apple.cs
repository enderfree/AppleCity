using UnityEngine;

public class Apple : Item, IThrowable
{
    [Header("Apple Attributes")]
    [SerializeField] private AppleTypeEnum appleType;
    [SerializeField] private float damage;

    // Functions
    public virtual void ThrowItem()
    {
        // todo
    }

    public virtual void OnImpact()
    {
        // todo
    }

    // Getters and Setters
    public virtual AppleTypeEnum AppleType
    {
        get 
        {
            return appleType;
        }
        set
        {
            appleType = value;
        }
    }

    public virtual float Damage
    {
        get
        {
            return damage;
        }
        set
        {
            damage = value;
        }
    }
}
