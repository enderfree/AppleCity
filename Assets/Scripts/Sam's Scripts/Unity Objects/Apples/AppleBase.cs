using UnityEngine;

public class AppleBase: Item, IThrowable
{
    [Header("Apple Attributes")]
    [SerializeField] private AppleTypeEnum _appleType;
    [SerializeField] private float _damage;

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
