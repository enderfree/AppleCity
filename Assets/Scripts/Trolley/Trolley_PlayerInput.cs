using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Trolley_PlayerInput : NetworkBehaviour
{
    [SerializeField] private float _horizontalInput, _verticalInput;
    public float readHorizon => _horizontalInput;
    public float readVertical => _verticalInput;

    [SerializeField] private Trolley_Script _trolleyScript;

    private void Update()
    {
        PlayerInput();
    }

    private void PlayerInput()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        _trolleyScript.SendPlayerInputRpc(_verticalInput, _horizontalInput);
    }

    public void GetTrolleyScript(Trolley_Script script)
    {
        _trolleyScript = script;
    }
}
