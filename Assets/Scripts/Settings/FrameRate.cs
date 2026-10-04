using Unity.Netcode;
using UnityEngine;

public enum RefreshRateMode 
{ 
    CustomFrameRate, 
    VSync
}

public class FrameRate : MonoBehaviour
{
    /// <summary>
    /// Tooltips:
    /// 1.Drag this script into a Empty GameObject in the Scene
    /// 2.Choice the refresh rate mode in Inspector
    /// 
    /// Feel free to use it in your game to save PC performance
    /// This script doesn't persist on Load, Place it under a Singleton gameObject, such as Game or Setting Managers, if you want it to persist on Load.
    /// </summary>


    
    [SerializeField] private RefreshRateMode _mode;
    [SerializeField] private int _frameRate = 120;

    private void Awake()
    {
        InitiateRefreshMode();
    }

    private void InitiateRefreshMode()
    {
        if (_mode == RefreshRateMode.VSync) 
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _frameRate;
        }

    }

    [ContextMenu("Debug: Update Changes")]
    public void DebugModeOnChanged() => InitiateRefreshMode();

}
