using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// That's a normal UI manager that does not mess with the online too much unlike the other ones!
/// </summary>
public class UiManager : MonoBehaviour
{
    /// <summary>
    /// static reference to the 
    /// </summary>
    public static UiManager Instance { get; private set; }

    /// <summary>
    /// UI states represents what we currently are trying to display
    /// </summary>
    public enum UIstate { PauseMenu, Settings, Playing, Inventory }

    // which tab is currently selected
    [SerializeField] private UIstate _tabState;

    // the main menu panel
    [SerializeField] private GameObject _mainPanel;

    // the setting menu panel
    [SerializeField] private GameObject _settings;

    // the inventory panel
    [SerializeField] private GameObject _inventoryPanel;

    // the char UI
    [SerializeField] private GameObject _gameplayUI;

    // the background image of the menu
    [SerializeField] private GameObject _backgroundIMG;

    // the hp bar
    [SerializeField] private Slider _hpBar;

    // the text written on the hp bar
    [SerializeField] private TMP_Text _hpText;

    /// <summary>
    /// Check if singleton on awake
    /// </summary>
    private void Awake()
    {
        IsSingletonComponent();
    }

    /// <summary>
    /// Setup the initial tab state
    /// </summary>
    void Start()
    {
        ChangeTabState();
    }

    /// <summary>
    /// Subscribe to OnHPChanged() on OnEnable()
    /// </summary>
    private void OnEnable()
    {
        PlayerStats.OnHPChanged += OnHPChanged;
    }

    /// <summary>
    /// Unsubscribe to OnHPChanged on OnDisable
    /// </summary>
    private void OnDisable()
    {
        PlayerStats.OnHPChanged -= OnHPChanged;
    }

    /// <summary>
    /// inputs are still a bit weird and all over that place, Joshua style~
    /// </summary>
    private void Update()
    {
        GetInputs();
    }

    /// <summary>
    /// Checking for the menu inputs
    /// </summary>
    private void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPausingMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryButton();
        }
    }

    /// <summary>
    /// Pause Menu logic
    /// </summary>
    private void OnPausingMenu()
    {
        if (_tabState == UIstate.Playing || _tabState == UIstate.Inventory)
        {
            _tabState = UIstate.PauseMenu;
        }
        else if (_tabState == UIstate.PauseMenu || _tabState == UIstate.Settings) 
        {
            _tabState = UIstate.Playing;
        }
        ChangeTabState();
    }

    /// <summary>
    /// Initiate game UI
    /// </summary>
    public void PlayerInitiateUI()
    {
        _tabState = UIstate.Playing;
        ChangeTabState();
    }

    /// <summary>
    /// Switch state... I don't think we saw Switch Cases yet
    /// </summary>
    private void ChangeTabState()
    {
        if (_mainPanel != null)
        {
            if (_tabState == UIstate.PauseMenu)
            {
               
                PanelOption(CursorLockMode.None, true,false,true,false,false);

            }
            else if (_tabState == UIstate.Settings)
            {
                
                PanelOption(CursorLockMode.None,true, false, false, true, false);

            }
            else if (_tabState == UIstate.Playing)
            {
                
                PanelOption(CursorLockMode.Locked, false, true, false, false, false);

            }
            else if (_tabState == UIstate.Inventory)
            {

                PanelOption(CursorLockMode.Locked,false, true, false, false, true);
            }
        }
    }

    /// <summary>
    /// handles which panel is displayed
    /// </summary>
    /// <param name="cursor">how the cursor should behave</param>
    /// <param name="background">if the background image should be displayed</param>
    /// <param name="playUI">if the player UI should be displayed</param>
    /// <param name="mainP">if the main menu panel should be displayed</param>
    /// <param name="setting">if the setting pannel should be displayed</param>
    /// <param name="inventory">if the inventory should be displayed</param>
    private void PanelOption(CursorLockMode cursor,bool background, bool playUI, bool mainP,bool setting, bool inventory )
    {
        Cursor.lockState = cursor;
        _backgroundIMG.SetActive(background);
        _gameplayUI.SetActive(playUI);
        _mainPanel.SetActive(mainP);
        _settings.SetActive(setting);
        _inventoryPanel.SetActive(inventory);
        
    }

    /// <summary>
    /// what happens graphically when the hp is changed
    /// </summary>
    /// <param name="hpValue">the new hp value</param>
    private void OnHPChanged(float hpValue)
    {
        _hpBar.value = hpValue;
        _hpText.text = hpValue.ToString();
    }

    //Functions for UI buttons
    /// <summary>
    /// Back to game button logic
    /// </summary>
    private void BackToPlaying()
    {
        _tabState = UIstate.Playing;
        ChangeTabState();
    }

    /// <summary>
    /// Setting button logic
    /// </summary>
    private void SettingsButton()
    {
        _tabState = UIstate.Settings;
        ChangeTabState();
    }

    /// <summary>
    /// Back button logic
    /// </summary>
    private void BackButton()
    {
        _tabState = UIstate.PauseMenu;
        ChangeTabState();
    }

    /// <summary>
    /// Inventory button logic
    /// </summary>
    private void InventoryButton()
    {
        if (_tabState == UIstate.Playing)
        {
            _tabState = UIstate.Inventory;
        }
        else if (_tabState == UIstate.Inventory)
        {
            _tabState = UIstate.Playing;
        }
        ChangeTabState();
    }

    /// <summary>
    /// back to main menu button logic
    /// </summary>
    private void BacktoMainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Makes sure this is a singleton
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
