using UnityEngine;
using UnityEngine.InputSystem;
public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }
    public enum UIstate { PauseMenu, Settings, Playing, Inventory }
    [SerializeField] private UIstate tabState;

    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _gameplayUI;
    [SerializeField] private GameObject _backgroundIMG;


    private void Awake()
    {
        IsSingletonComponent();
    }
    void Start()
    {
        ChangeTabState();
    }

    private void Update()
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
    private void OnPausingMenu()
    {
        if (tabState == UIstate.Playing || tabState == UIstate.Inventory) { tabState = UIstate.PauseMenu; }
        else if (tabState == UIstate.PauseMenu || tabState == UIstate.Settings) { tabState = UIstate.Playing; }
        ChangeTabState();
    }

    public void PlayerInitiateUI()
    {
        tabState = UIstate.Playing;
        ChangeTabState();
    }

    private void ChangeTabState()
    {
        if (_mainPanel != null)
        {
            if (tabState == UIstate.PauseMenu)
            {
               
                PanelOption(CursorLockMode.None, true,false,true,false,false);

            }
            else if (tabState == UIstate.Settings)
            {
                
                PanelOption(CursorLockMode.None,true, false, false, true, false);

            }
            else if (tabState == UIstate.Playing)
            {
                
                PanelOption(CursorLockMode.Locked, false, true, false, false, false);

            }
            else if (tabState == UIstate.Inventory)
            {

                PanelOption(CursorLockMode.Locked,false, true, false, false, true);
            }
        }
    }

    private void PanelOption(CursorLockMode cursor,bool background, bool playUI, bool mainP,bool setting, bool inventory )
    {
        Cursor.lockState = cursor;
        _backgroundIMG.SetActive(background);
        _gameplayUI.SetActive(playUI);
        _mainPanel.SetActive(mainP);
        _settings.SetActive(setting);
        _inventoryPanel.SetActive(inventory);
        
    }

    //Functions for UI buttons
    private void BackToPlaying()
    {
        tabState = UIstate.Playing;
        ChangeTabState();
    }

    private void SettingsButton()
    {
        tabState = UIstate.Settings;
        ChangeTabState();
    }

    private void BackButton()
    {
        tabState = UIstate.PauseMenu;
        ChangeTabState();
    }

    private void InventoryButton()
    {
        tabState = UIstate.Inventory;
        ChangeTabState();
    }

    private void BacktoMainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

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
