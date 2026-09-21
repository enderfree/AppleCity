using TMPro;
using UnityEngine;

public class InteractionTooltipUI : MonoBehaviour
{
    public static InteractionTooltipUI Instance { get; private set; }

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text labelPress;
    [SerializeField] private TMP_Text inputKeyButton;
    [SerializeField] private TMP_Text labelAction;

    private void Awake()
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

    public void Hide()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}