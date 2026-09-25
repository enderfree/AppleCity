using UnityEngine;
using Unity.Netcode;

public class Trolley_Interact : NetworkBehaviour
{
    [SerializeField] private Trolley_Script _script;
    [SerializeField] private GameObject _detectedPlayer;

    private bool _isControlling => _script.isControl_m;
    public GameObject _playerRef => _detectedPlayer;

    // copy this block to a new script
    private void OnTriggerEnter(Collider other)
    {
        _detectedPlayer = other.gameObject;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (_isControlling) return;

        //Press F to Enter pilot mode
        if (Input.GetKeyUp(KeyCode.F))
        {   
            _script.PlayerInteractEnter(_detectedPlayer);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _detectedPlayer = null;
    }
}
