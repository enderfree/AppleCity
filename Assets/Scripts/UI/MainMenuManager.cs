using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

     public enum mainMenuTabs {mainTab, settingsTab }
    [SerializeField] private mainMenuTabs tabState;
    [SerializeField] private GameObject _gameTitle;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Scene")]
    [SerializeField] private string _startSceneName;


    void Start()
    {
            tabState = mainMenuTabs.mainTab;
            ChangeTabState();
    }

    //Function call with buttons
    public void GameStartButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_startSceneName);
    }
    public void SettingButton()
    { 
      tabState = mainMenuTabs.settingsTab;
        ChangeTabState();
    }

    public void BackButton()
    {
        tabState = mainMenuTabs.mainTab;
        ChangeTabState();
    }

    public void ExitButton()
    { 
    Application.Quit();
    }

    // Using enum to manage the state of the MainMenu, to show and hide the right elements
    public void ChangeTabState()
    {
        if (_gameTitle != null && _mainPanel != null && _settingsPanel != null)
        {
            if (tabState == mainMenuTabs.mainTab)
            {
                _mainPanel.SetActive(true);
                _gameTitle.SetActive(true);
                _settingsPanel.SetActive(false);
            }
            else if (tabState == mainMenuTabs.settingsTab)
            {
                _mainPanel.SetActive(false);
                _gameTitle.SetActive(false);
                _settingsPanel.SetActive(true);
            }
        }
    }
}
