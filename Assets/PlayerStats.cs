using UnityEngine;

public class PlayerStats : Character
{
    public delegate void HPChanged(float newHPValue);
    public static event HPChanged OnHPChanged;

    public override float HP
    {
        // can't override only the setter and base works weird in getters and setters
        get
        {
            return base.HP;
        }
        set 
        {
            base.HP = value;
            OnHPChanged(HP);
        }
    }
}
