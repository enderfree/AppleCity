using UnityEngine;

public class SettingManager : MonoBehaviour
{
    /// <summary>
    /// static reference to this manager
    /// </summary>
    public static SettingManager Instance { get; private set; }

    /// <summary>
    /// Make a singleton on Awake()
    /// </summary>
    private void Awake()
    {
        IsSingletonComponent();
    }

    /// <summary>
    /// keep this as a singleton
    /// </summary>
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
