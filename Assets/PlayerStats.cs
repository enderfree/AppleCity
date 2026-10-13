using System;
using UnityEngine;

public class PlayerStats : Character
{
    [NonSerialized] public Vector3 _respawnPoint;

    public delegate void HPChanged(float newHPValue);
    public static event HPChanged OnHPChanged;

    protected void Kill()
    {
        transform.position = _respawnPoint;
        HP = MaxHP;
    }

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
