
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private GameObject noteUI;

    private void Start()
    {
        noteUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            noteUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            noteUI.SetActive(false);
        }
    }
}
