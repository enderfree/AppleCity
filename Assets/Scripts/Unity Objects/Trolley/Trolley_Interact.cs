using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Script handling trolley interractions
/// </summary>
public class Trolley_Interact : NetworkBehaviour
{
    // driver
    [SerializeField] private GameObject _detectedPlayer;

    // if the trolley is currently under someone's control
    private bool _havePlayer;

    // Public accessor for _havePlayer
    public bool HavePlayer => _havePlayer;

    //Trolley Ref
    // the trolley prefab
    [SerializeField] private GameObject _trolleyPrefab;

    // the trolley script
    [SerializeField] private Trolley_Script _trolleyScript;

    // the position of the player when pushing the trolley
    [SerializeField] private Transform _pilotPosition;

    //Player Ref
    // the current player state
    [SerializeField] private Player_State _playerState;


    // copy this block to a new script
    /// <summary>
    /// purposefully uncleared!
    /// </summary>
    private void FixedUpdate()
    {
        DelayClearInfo();
    }

    /// <summary>
    /// Check who's the detected player
    /// </summary>
    /// <param name="other">The triggering player</param>
    private void OnTriggerEnter(Collider other)
    {
        _detectedPlayer = other.gameObject;
    }

    /// <summary>
    /// keep detecting the player while they keep triggering you
    /// </summary>
    /// <param name="other">The triggering player</param>
    private void OnTriggerStay(Collider other)
    {
        DetectPlayer(other);
    }

    /// <summary>
    /// stop detecting the player when they leave you alone
    /// </summary>
    /// <param name="other">the no longer triggreing player</param>
    private void OnTriggerExit(Collider other)
    {
        _detectedPlayer = null;
    }

    /// <summary>
    /// the player detection
    /// </summary>
    /// <param name="other">a collision</param>
    private void DetectPlayer(Collider other)
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
                _playerState.GetTrolleyRef(_trolleyPrefab, _trolleyScript, this, _pilotPosition);
            }
        }
        else
        {
            _havePlayer = false;

        }

    }

    /// <summary>
    /// stops the info from being cleared yet
    /// </summary>
    private void DelayClearInfo()
    {
        if (_detectedPlayer == null && _playerState != null)
        {
            
            if (_playerState.readCurrentState == PlayerState.Normal_State)
           
               _playerState.GetTrolleyRef(null, null, null, null);
        }
    }



}
