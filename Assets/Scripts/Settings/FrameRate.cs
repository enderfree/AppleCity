using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The refresh rate mode options of our game
/// </summary>
public enum RefreshRateMode 
{ 
    CustomFrameRate, 
    VSync
}

/// <summary>
/// A framerate optimisation script Jun found online
/// </summary>
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


    // the refresh rate mode to use
    [SerializeField] private RefreshRateMode _mode;

    // the target framerate of our game
    [SerializeField] private int _frameRate = 120;

    /// <summary>
    /// Monobehavior's Awake()
    /// </summary>
    private void Awake()
    {
        InitiateRefreshMode();
    }

    /// <summary>
    /// setup the game based on the desired refresh mode
    /// </summary>
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

    /// <summary>
    /// Context menu option that let us call InitiateRefreshMode() while testing stuff
    /// </summary>
    [ContextMenu("Debug: Update Changes")]
    public void DebugModeOnChanged() => InitiateRefreshMode();

}
