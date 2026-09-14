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
    public void OnInpact();
}
