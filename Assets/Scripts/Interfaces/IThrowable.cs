using UnityEngine;

/// <summary>
/// Interface telling that this is something that can be thrown...
/// Due to time constraints, Throw is player-side only rn, but we did plan to have thrown items handle their trajectory
/// in order to do stupid things like when you throw a crabapple it goes sideways
/// </summary>
public interface IThrowable
{
    /// <summary>
    /// How to throw the thing
    /// </summary>
    public void ThrowItem();

    /// <summary>
    /// How the thing behaves when hitting something
    /// </summary>
    /// <param name="collision">The collision from the impact</param>
    public void OnImpact(Collision collision);

    /// <summary>
    /// Damage value of the thrown item (since throwing is our main mean of defense)
    /// </summary>
    public float Damage { get; set; }
}
