using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

    private void Awake()
    {
        IsSingletonComponent();
    }

    private void IsSingletonComponent()
    {
        // Keep the first GameManager alive and destroy the object whenever there is a second one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
