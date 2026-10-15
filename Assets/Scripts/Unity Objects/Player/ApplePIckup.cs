using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The logic needed to pickup an apple
/// </summary>
public class ApplePIckup : NetworkBehaviour
{
    // placeholder for the inventory since we couldn't make light of Josh's
  [SerializeField] List<NetworkObject> _appleList = new List<NetworkObject>();




    /// <summary>
    /// Allow for apple pickup when in range
    /// </summary>
    /// <param name="other">the apple's collider</param>
    private void OnTriggerEnter(Collider other)
    {
        GetAppleToList(other);
    }

    /// <summary>
    /// Disallow for apple pickup when no longer in range
    /// </summary>
    /// <param name="other">the apple's collider</param>
    private void OnTriggerExit(Collider other)
    {
        RemoveAppleFromList(other);
    }

    /// <summary>
    /// put apple in inventory
    /// </summary>
    /// <param name="col">apple collider</param>
    private void GetAppleToList(Collider col)
    {
        if (col.gameObject.CompareTag("Apple"))
        {
            if (col.TryGetComponent<NetworkObject>(out var netApple))
            {
                if (!_appleList.Contains(netApple))
                {
                    _appleList.Add(netApple);
                }
            }
        }
    }

    /// <summary>
    /// remove apple from inventory
    /// </summary>
    /// <param name="col">apple collider</param>
    private void RemoveAppleFromList(Collider col)
    {
        if (col.gameObject.CompareTag("Apple"))
        {
            if (col.TryGetComponent<NetworkObject>(out var netApple))
            {
                _appleList.Remove(netApple);
            }
        }
    }



    /// <summary>
    /// Accessor for the temp inventory; although, I don't think it works like Jun think it does
    /// </summary>
    public virtual List<NetworkObject> AppleList
    {
        get {return _appleList;}
        set {_appleList = value;}
    }
}
