using UnityEngine;

public class AppleBase: Item, IThrowable
{
    [Header("Apple Attributes")]
    [SerializeField] private AppleTypeEnum _appleType;
    [SerializeField] private float _damage;

    // Unity
    protected void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(collision.gameObject.name); // The number of bounces is low so not checking velocity should be fine
        OnImpact(collision);
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
