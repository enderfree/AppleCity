using UnityEngine;

/// <summary>
/// hide the object in game, but not in the inspector
/// </summary>
public class HideInGame : MonoBehaviour
{
    /// <summary>
    /// Hide on Start()
    /// </summary>
    void Start()
    {
        Hide();
    }

    /// <summary>
    /// Disable the renderer
    /// </summary>
    private void Hide()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
