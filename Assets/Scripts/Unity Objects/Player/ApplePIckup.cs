using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ApplePIckup : NetworkBehaviour
{
  [SerializeField] List<NetworkObject> _appleList = new List<NetworkObject>();





    private void OnTriggerEnter(Collider other)
    {
        GetAppleToList(other);
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveAppleFromList(other);
    }

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




    public virtual List<NetworkObject> AppleList
    {
        get {return _appleList;}
        set {_appleList = value;}
    }
}
