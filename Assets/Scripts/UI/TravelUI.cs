using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class managing the GUI elements during the ready and scene change
/// </summary>
public class TravelUI : MonoBehaviour
{
    /// <summary>
    /// static reference to this manager
    /// </summary>
    public static TravelUI Instance { get; private set; }

    [Header("UI References")]
    // the text related to the state currently ongoing
    [SerializeField] private TMP_Text _statusText;

    // the text shown right before the scene change
    [SerializeField] private TMP_Text _transitionText;

    /// <summary>
    /// So far I've been moving this on innit, but this time there is an awake and a start so I don't really dare
    /// </summary>
    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged +=
            OnActiveSceneChanged;
    }

    /// <summary>
    /// So far I've been moving this on innit, but this time there is an awake and a start so I don't really dare
    /// </summary>
    private void Start()
    {
        ClearStatus();
        HideTransition();
    }

    /// <summary>
    /// Monobehavior's Update()
    /// </summary>
    private void Update()
    {
        UpdateReadyStatus();
    }

    /// <summary>
    /// tries to keep things ready for the travel
    /// </summary>
    private void UpdateReadyStatus()
    {
        if (_statusText == null)
        {
            return;
        }

        string sceneName =
            SceneManager.GetActiveScene().name;

        int ready;
        int connected;

        if (sceneName == "AppleCity")
        {
            HubDepartureManager manager =
                HubDepartureManager.Instance;

            if (manager == null)
            {
                ClearStatus();
                return;
            }

            ready = manager._readyPlayers.Value;
            connected = manager._connectedPlayers.Value;
        }
        else if (sceneName == "AppleForest")
        {
            ForestExtractionManager manager =
                ForestExtractionManager.Instance;

            if (manager == null)
            {
                ClearStatus();
                return;
            }

            ready = manager._readyPlayers.Value;
            connected = manager._connectedPlayers.Value;
        }
        else
        {
            ClearStatus();
            return;
        }

        _statusText.text =
            $"Players Ready: {ready}/{connected}\n" +
            $"You: {GetLocalReadyState()}";
    }

    /// <summary>
    /// Get if this client is ready or not
    /// </summary>
    /// <returns>a ready state</returns>
    private string GetLocalReadyState()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            networkManager.LocalClient == null ||
            networkManager.LocalClient.PlayerObject == null)
        {
            return "NOT READY";
        }

        HubDeparturePlayer departurePlayer =
            networkManager.LocalClient.PlayerObject
                .GetComponent<HubDeparturePlayer>();

        if (departurePlayer == null)
        {
            return "NOT READY";
        }

        return departurePlayer._isReady.Value
            ? "READY"
            : "NOT READY";
    }

    /// <summary>
    /// handles the transition
    /// </summary>
    /// <param name="message">transition message to be displayed</param>
    public void ShowTransition(string message)
    {
        if (_transitionText == null)
        {
            return;
        }

        _transitionText.text = message;
        _transitionText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the transition once it's over
    /// </summary>
    public void HideTransition()
    {
        if (_transitionText == null)
        {
            return;
        }

        _transitionText.text = string.Empty;
        _transitionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// reset status text
    /// </summary>
    private void ClearStatus()
    {
        if (_statusText == null)
        {
            return;
        }

        _statusText.text = string.Empty;
    }

    /// <summary>
    /// Clear status when scene is changed
    /// </summary>
    /// <param name="previousScene">the scene before the change</param>
    /// <param name="newScene">the scene you are going to</param>
    private void OnActiveSceneChanged(
        Scene previousScene,
        Scene newScene)
    {
        /*
         * Never allow a transition message from the previous
         * scene to survive into the new scene.
         */
        HideTransition();

        if (newScene.name != "AppleCity")
        {
            ClearStatus();
        }

        Debug.Log(
            $"TravelUI: Entered '{newScene.name}'. " +
            "Transition UI cleared."
        );
    }

    /// <summary>
    /// Unsubscribe on OnDestroy()
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -=
            OnActiveSceneChanged;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}