using UnityEngine;

public class ApplyForceOnStart : MonoBehaviour
{
    [SerializeField] private Vector3 _force;

    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Innit();
        ApplyForce();
    }

    private void Innit()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    private void ApplyForce()
    {
        _rb.AddForce(_force, ForceMode.Impulse);
    }
}
