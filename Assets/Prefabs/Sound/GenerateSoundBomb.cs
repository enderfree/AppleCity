using UnityEngine;

public class GenerateSoundBomb : MonoBehaviour
{
    [SerializeField] private GameObject _soundPrefab;
    private void OnEnable()
    {
        Instantiate(_soundPrefab, gameObject.transform.position, gameObject.transform.rotation);
    }
}
