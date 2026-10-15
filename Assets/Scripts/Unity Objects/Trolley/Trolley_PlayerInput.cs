using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Handles the player inputs while the player is driving a trolley
/// </summary>
public class Trolley_PlayerInput : NetworkBehaviour
{
    // horizontal and vertical inputs (to see in the editor)
    [SerializeField] private float _horizontalInput, _verticalInput;

    // horizontal input public accessor
    public float readHorizon => _horizontalInput;

    // vertical input public accessor
    public float readVertical => _verticalInput;

    // the trolley script
    [SerializeField] private Trolley_Script _trolleyScript;

    /// <summary>
    /// read inputs on Update()
    /// </summary>
    private void Update()
    {
        PlayerInput();
    }

    /// <summary>
    /// Reads player inputs
    /// </summary>
    private void PlayerInput()
    {
        if (!IsOwner) return;

        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        _trolleyScript.SendPlayerInputRpc(_verticalInput, _horizontalInput);
    }

    /// <summary>
    /// Get the trolley script
    /// </summary>
    /// <param name="script">the trolley script</param>
    public void GetTrolleyScript(Trolley_Script script)
    {
        _trolleyScript = script;
    }
}
