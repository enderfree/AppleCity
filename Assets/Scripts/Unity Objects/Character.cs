using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Characters includes all in comon between player and NPC
/// </summary>
public class Character: NetworkBehaviour, IHitable
{
    [Header("Character Attributes")]
    // name of the character
    [SerializeField] private string _charName;

    // char's max hp
    [SerializeField] private float _maxHP;

    // current hp
    [SerializeField] private float _hp;

    // max carry capacity (currently not in used)
    [SerializeField] private float _weightCapacity;

    // current weigh carried (currently not in use)
    [SerializeField] private float _carriedWeight;

    // removed _equipment as we now have a script dedicated to the inventory

    [Header("Sound")]
    // the audio player of the character
    [SerializeField] private AudioSource _player;

    // the sound the character makes when hit
    [SerializeField] private AudioClip _hitSoundClip;

    // the volume at which the sound is player
    [SerializeField] private float _volume = 0.2f;

    // Interface Members
    /// <summary>
    /// what happens when char is hit
    /// </summary>
    /// <param name="damage">the amount of damage taken</param>
    public virtual void OnHit(float damage)
    {
        HP -= damage;
        PlaySound(_hitSoundClip);
    }

    /// <summary>
    /// play the sound provided
    /// </summary>
    /// <param name="clip">the sound to play</param>
    public virtual void PlaySound(AudioClip clip)
    {
        this.GetComponent<AudioSource>().PlayOneShot(clip, _volume);
    }

    // Getters and Setters
    /// <summary>
    /// just settuping my observers in case I need them later
    /// </summary>
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

    /// <summary>
    /// Here my observer is needed~
    /// I'm making sure the hp stays between 0 and MaxHP
    /// </summary>
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
