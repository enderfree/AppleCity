using UnityEngine;

public class Character: MonoBehaviour, IHitable
{
    [Header("Character Attributes")]
    [SerializeField] private string _charName;
    [SerializeField] private float _maxHP;
    [SerializeField] private float _hp;
    [SerializeField] private float _weightCapacity;
    [SerializeField] private float _carriedWeight;
    // removed _equipment as we now have a script dedicated to the inventory

    // Interface Members
    public virtual void OnHit(float damage)
    {
        HP -= damage;
    }

    // Getters and Setters
    public virtual string CharName
    {
        get
        {
            return _charName;
        }
        set
        {
            _charName = value; 
        }
    }

    public virtual float MaxHP
    {
        get
        {
            return _maxHP;
        }
        set
        {
            _maxHP = value;
        }
    }

    public virtual float HP
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
        }
    }

    public virtual float WeightCapacity
    {
        get
        {
            return _weightCapacity;
        }
        set
        {
            _weightCapacity = value;
        }
    }

    public virtual float CarriedWeight
    {
        get
        {
            return _carriedWeight;
        }
        set
        {
            _carriedWeight = value;
        }
    }
}
