
using UnityEngine;

/// <summary>
/// Script handling the lore
/// </summary>
public class Collectible : MonoBehaviour
{
    [SerializeField] private GameObject _noteUI;

    /// <summary>
    /// Innit on Start()
    /// </summary>
    private void Start()
    {
        Innit();
    }

    /// <summary>
    /// Innitialize default state
    /// </summary>
    private void Innit()
    {
        _noteUI.SetActive(false);
    }

    /// <summary>
    /// Show note when player is in trigger range
    /// </summary>
    /// <param name="other">collider triggering</param>
    private void OnTriggerEnter(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _noteUI.SetActive(true);
        }
    }

    /// <summary>
    /// Stop showing when player leaves trigger zone
    /// </summary>
    /// <param name="other">collider no longer triggering</param>
    private void OnTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _noteUI.SetActive(false);
        }
    }
}
