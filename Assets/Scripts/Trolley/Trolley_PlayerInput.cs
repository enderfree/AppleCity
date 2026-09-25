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

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _trolleyScript = null;
    }
    private void PlayerInput()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        _trolleyScript.SendPlayerInputServerRPC(_verticalInput, _horizontalInput);

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Pressed F trying to Exit");
            _trolleyScript.QuitPilot();
        }
    }

    public void GetTrolleyScript(Trolley_Script script)
    { 
      _trolleyScript = script;
    }
}
