using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class Trolley_Interact : NetworkBehaviour
{
    
    [SerializeField] private GameObject _detectedPlayer;
    private bool _havePlayer;
    public bool _playerBool => _havePlayer;

    //Trolley Ref

    [SerializeField] private GameObject _trolleyPrefab;
    [SerializeField] private Trolley_Script _trolleyScript;

    //Player Ref
    [SerializeField] private Player_State _playerState;


    // copy this block to a new script

    private void FixedUpdate()
    {
        if (_detectedPlayer == null && _playerState !=null)
        {
            _playerState.GetTrolleyRef(null, null, null);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        _detectedPlayer = other.gameObject;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _detectedPlayer != null)
        {
            _havePlayer = true;

            if (_detectedPlayer.TryGetComponent<Player_State>(out Player_State script))
            {
                _playerState = script;
            }

            if (_playerState != null)
            {
                _playerState.GetTrolleyRef(_trolleyPrefab, _trolleyScript, this);
            }
        }
        else
        {
            _havePlayer = false;

        } 
    }

    private void OnTriggerExit(Collider other)
    {
        _detectedPlayer = null;
    }


}
