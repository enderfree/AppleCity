using UnityEngine;

/// <summary>
/// This classes handles the main menu
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // this enum should contain all main menu tabs
     public enum mainMenuTabs {mainTab, settingsTab }

    // which tab is currently selected
    [SerializeField] private mainMenuTabs _tabState;

    // the game title in the main menu
    [SerializeField] private GameObject _gameTitle;

    // the main panel in the menu
    [SerializeField] private GameObject _mainPanel;

    // the pannel on which settings are
    [SerializeField] private GameObject _settingsPanel;

    [Header("Scene")]
    // the name of the starting scene
    [SerializeField] private string _startSceneName;

    /// <summary>
    /// on start
    /// </summary>
    void Start()
    {
        Innit();
        ChangeTabState();
    }

    /// <summary>
    /// Initialize variables
    /// </summary>
    private void Innit()
    {
        _tabState = mainMenuTabs.mainTab;
    }

    //Function call with buttons
    
    /// <summary>
    /// What happens when the Game Start button is pressed
    /// </summary>
    public void GameStartButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_startSceneName);
    }

    /// <summary>
    /// What happens when the Setting button is pressed
    /// </summary>
    public void SettingButton()
    { 
      _tabState = mainMenuTabs.settingsTab;
        ChangeTabState();
    }

    /// <summary>
    /// What happens when the Back button is pressed
    /// </summary>
    public void BackButton()
    {
        _tabState = mainMenuTabs.mainTab;
        ChangeTabState();
    }

    /// <summary>
    /// What happens when the Exit button is pressed
    /// </summary>
    public void ExitButton()
    { 
    Application.Quit();
    }

    /// <summary>
    /// Going from one tab to the other
    /// </summary>
    // Using enum to manage the state of the MainMenu, to show and hide the right elements
    public void ChangeTabState()
    {
        if (_gameTitle != null && _mainPanel != null && _settingsPanel != null)
        {
            if (_tabState == mainMenuTabs.mainTab)
            {
                _mainPanel.SetActive(true);
                _gameTitle.SetActive(true);
                _settingsPanel.SetActive(false);
            }
            else if (_tabState == mainMenuTabs.settingsTab)
            {
                _mainPanel.SetActive(false);
                _gameTitle.SetActive(false);
                _settingsPanel.SetActive(true);
            }
        }
    }
}
