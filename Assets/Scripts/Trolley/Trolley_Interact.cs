using UnityEngine;
using Unity.Netcode;

public class Trolley_Interact : NetworkBehaviour
{
    [SerializeField] private Trolley_Script script;
    private bool _isControlling => script.isControl_m;

   // copy this block to a new script
   private void OnTriggerStay(Collider other)
    {
        //if (!other.gameObject.CompareTag("Player")) return;
        if (_isControlling) return;

        //Press F to Enter pilot mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            script.PlayerInteract();
        }
    }
}
