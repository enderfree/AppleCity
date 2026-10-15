using TMPro;
using UnityEngine;

/// <summary>
/// The tooltip used to pick up apples
/// </summary>
public class InteractionTooltipUI : MonoBehaviour
{
    /// <summary>
    /// Static reference to this manager
    /// </summary>
    public static InteractionTooltipUI Instance { get; private set; }

    [Header("Tooltip")]
    // GameObject that represents the position of the tooltip
    [SerializeField] private GameObject tooltipRoot;

    [Header("Text")]
    // label that tells players to press
    [SerializeField] private TMP_Text labelPress;

    // label telling players which button to press
    [SerializeField] private TMP_Text inputKeyButton;

    // decorative label
    [SerializeField] private TMP_Text labelAction;

    /// <summary>
    /// Innit on Awake to not clutter Awake()
    /// </summary>
    private void Awake()
    {
        Innit();
    }

    /// <summary>
    /// initial setup
    /// </summary>
    private void Innit()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        /*
         * Keep this UI alive when NGO moves us between
         * AppleCity and AppleForest.
         */
        DontDestroyOnLoad(gameObject);

        Hide();
    }

    /// <summary>
    /// Show the tooltip
    /// </summary>
    /// <param name="action">action label</param>
    /// <param name="key">key label</param>
    /// <param name="pressLabel">press label</param>
    public void Show(
        string action,
        string key = "E",
        string pressLabel = "Press")
    {
        if (labelPress != null)
        {
            labelPress.text = pressLabel;
        }

        if (inputKeyButton != null)
        {
            inputKeyButton.text = key;
        }

        if (labelAction != null)
        {
            labelAction.text = action;
        }

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }
    }

    // Hide the looltip
    public void Hide()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    /// <summary>
    /// update the instance if this is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}