using UnityEngine;
using Unity.Netcode;


public class Character: NetworkBehaviour, IHitable
{
    [Header("Character Attributes")]
    [SerializeField] private string _charName;
    [SerializeField] private float _maxHP;
    [SerializeField] private float _hp;
    [SerializeField] private float _weightCapacity;
    [SerializeField] private float _carriedWeight;
    // removed _equipment as we now have a script dedicated to the inventory
    [Header("Sound")]
    [SerializeField] private AudioSource _player;
    [SerializeField] private AudioClip _hitSoundClip;
    [SerializeField] private float _volume = 0.2f;

    // Interface Members
    public virtual void OnHit(float damage)
    {
        HP -= damage;
        PlaySound(_hitSoundClip);
    }

    public virtual void PlaySound(AudioClip clip)
    {
        this.GetComponent<AudioSource>().PlayOneShot(clip, _volume);
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

            if (_hp < 0f)
            {
                _hp = 0f;
            }
            else if (_hp > MaxHP)
            {
                _hp = MaxHP;
            }
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
