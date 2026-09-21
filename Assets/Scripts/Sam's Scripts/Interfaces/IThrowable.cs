using UnityEngine;

public interface IThrowable
{
    /// <summary>
    /// How to throw the thing
    /// </summary>
    public void ThrowItem();

    /// <summary>
    /// How the thing behaves when hitting something midair
    /// </summary>
    public void OnImpact();

    /// <summary>
    /// Damage value of the thrown item (since throwing is our main mean of defense)
    /// </summary>
    public float Damage { get; set; }
}
