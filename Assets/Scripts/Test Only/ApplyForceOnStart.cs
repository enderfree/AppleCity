using UnityEngine;

/// <summary>
/// Made to throw apples before players could, applies force in a direction of your choice on its GameObject 
/// Requires a Rigidbody
/// Not used outside of test scenes
/// </summary>
public class ApplyForceOnStart : MonoBehaviour
{
    // the force to apply on the object
    [SerializeField] private Vector3 _force;

    // the Rigidbody component of the GameObject
    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Innit();
        ApplyForce();
    }

    /// <summary>
    /// Get the Rigidbody
    /// </summary>
    private void Innit()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Apply the force on the Rigidbody
    /// </summary>
    private void ApplyForce()
    {
        _rb.AddForce(_force, ForceMode.Impulse);
    }
}
