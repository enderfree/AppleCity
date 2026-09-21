using UnityEngine;

public interface IHitable
{
    /// <summary>
    /// What happens when something gets hit
    /// </summary>
    /// <param name="hit">the ammount for which the thing was hit</param>
    public void OnHit(float hit);
}
