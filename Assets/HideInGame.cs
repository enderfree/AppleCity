using UnityEngine;

public class HideInGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hide();
    }

    private void Hide()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
