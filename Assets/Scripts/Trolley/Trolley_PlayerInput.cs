using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Trolley_PlayerInput : NetworkBehaviour
{
    [SerializeField] private float _horizontalInput, _verticalInput;
    public float readHorizon => _horizontalInput;
    public float readVertical => _verticalInput;

    private void Update()
    {
        ReadPlayerInput();
    }

    private void ReadPlayerInput()
    {
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");
    }
}
