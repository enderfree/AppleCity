using UnityEngine;

/// <summary>
/// player camera
/// </summary>
public class PersistentMainCamera : MonoBehaviour
{
    // static reference to this camera
    private static PersistentMainCamera _instance;

    /// <summary>
    /// Innitalize on Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// Innitialize the cam
    /// </summary>
    private void Innit()
    {
        if (_instance != null &&
            _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// destrow this instance is the cemera GameObject is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}