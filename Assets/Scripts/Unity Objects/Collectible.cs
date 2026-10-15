
using UnityEngine;

/// <summary>
/// Script handling the lore
/// </summary>
public class Collectible : MonoBehaviour
{
    [SerializeField] private GameObject _noteUI;

    private void Start()
    {
        _noteUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _noteUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _noteUI.SetActive(false);
        }
    }
}
