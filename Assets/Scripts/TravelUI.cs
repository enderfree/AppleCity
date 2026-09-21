using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TravelUI : MonoBehaviour
{
    public static TravelUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text transitionText;

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

    private void Start()
    {
        ClearStatus();
        HideTransition();
    }

    private void Update()
    {
        UpdateReadyStatus();
    }

    private void UpdateReadyStatus()
    {
        if (statusText == null)
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

            ready = manager.ReadyPlayers.Value;
            connected = manager.ConnectedPlayers.Value;
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

            ready = manager.ReadyPlayers.Value;
            connected = manager.ConnectedPlayers.Value;
        }
        else
        {
            ClearStatus();
            return;
        }

        statusText.text =
            $"Players Ready: {ready}/{connected}\n" +
            $"You: {GetLocalReadyState()}";
    }

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

        return departurePlayer.IsReady.Value
            ? "READY"
            : "NOT READY";
    }

    public void ShowTransition(string message)
    {
        if (transitionText == null)
        {
            return;
        }

        transitionText.text = message;
        transitionText.gameObject.SetActive(true);
    }

    public void HideTransition()
    {
        if (transitionText == null)
        {
            return;
        }

        transitionText.text = string.Empty;
        transitionText.gameObject.SetActive(false);
    }

    private void ClearStatus()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = string.Empty;
    }

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