using UnityEngine;

public class FrameRate : MonoBehaviour
{
    public enum RefreshRateMode {VSync, CustomFrameRate }
    [SerializeField] private RefreshRateMode mode;
    [SerializeField] private int _frameRate = 60;

    private void Awake()
    {
        InitiateRefreshMode();
    }

    private void InitiateRefreshMode()
    {
        if (mode == RefreshRateMode.VSync) 
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _frameRate;
        }

    }
}
