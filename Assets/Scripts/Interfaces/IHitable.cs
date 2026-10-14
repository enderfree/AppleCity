using UnityEngine;

/// <summary>
/// Interface telling the system that something should happen when its member gets hit. 
/// </summary>
public interface IHitable
{
    /// <summary>
    /// What happens when something gets hit
    /// </summary>
    /// <param name="hit">the ammount for which the thing was hit</param>
    public void OnHit(float hit);
}
