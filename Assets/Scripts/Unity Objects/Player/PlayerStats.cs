using System;
using UnityEngine;

public class PlayerStats : Character
{
    // where the character should end up after death
    [NonSerialized] public Vector3 _respawnPoint;

    // event handling HP changes (mostly to warn the UI)
    public delegate void HPChanged(float newHPValue);
    public static event HPChanged OnHPChanged;

    /// <summary>
    /// what happens when the player dies
    /// </summary>
    protected void Kill()
    {
        transform.position = _respawnPoint;
        HP = MaxHP;
    }

    /// <summary>
    /// trigger the OnHPChanged event when hp is changed and die if you are killed
    /// </summary>
    public override float HP
    {
        // can't override only the setter
        get
        {
            return base.HP;
        }
        set 
        {
            base.HP = value;
            OnHPChanged(HP);

            if (HP <= 0f)
            {
                Kill();
            }
        }
    }
}
