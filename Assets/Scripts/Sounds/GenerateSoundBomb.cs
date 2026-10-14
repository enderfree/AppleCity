using UnityEngine;

public class GenerateSoundBomb : MonoBehaviour
{
    // the sound to play
    [SerializeField] private GameObject _soundPrefab;

    // play the sound when the hitbox is reenabled
    private void OnEnable()
    {
        Instantiate(_soundPrefab, gameObject.transform.position, gameObject.transform.rotation);
    }
}
