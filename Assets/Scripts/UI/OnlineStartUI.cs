using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class OnlineStartUI : MonoBehaviour
{
    private string joinCode = "";
    private string hostCode = "";
    private bool ready;
    private bool joined;

    private GUIStyle _fontStyle;
    [SerializeField] private int _fontSize = 24;

    [Header("Ready")]
    [SerializeField] private float ready_uiPosition_x = 10;
    [SerializeField] private float ready_uiPosition_y = 10; 
    [SerializeField] private float ready_uiWidth = 200;
    [SerializeField] private float ready_uiHeight = 30;
    [Header("HostCode/Joined")]
    [SerializeField] private float joined_uiPosition_x = 10;
    [SerializeField] private float joined_uiPosition_y = 10;
    [SerializeField] private float joined_uiWidth = 200;
    [SerializeField] private float joined_uiHeight = 30;

    [Header("Host/Join UI normal")]
    [SerializeField] private float _uiPosition_x = 880;
    [SerializeField] private float _uiPosition_y = 400;
    [SerializeField] private float _uiWidth = 220;
    [SerializeField] private float _uiHeight = 80;
    [Header("Host Button")]
    [SerializeField] private float host_uiPosition_x = 10;
    [SerializeField] private float host_uiPosition_y = 10;
    [SerializeField] private float host_uiWidth = 120;
    [SerializeField] private float host_uiHeight = 40;
    [Header("Code")]
    [SerializeField] private float code_uiPosition_x = 10;
    [SerializeField] private float code_uiPosition_y = 60;
    [SerializeField] private float code_uiWidth = 120;
    [SerializeField] private float code_uiHeight = 30;
    [Header("Join Button")]
    [SerializeField] private float join_uiPosition_x = 10;
    [SerializeField] private float join_uiPosition_y = 100;
    [SerializeField] private float join_uiWidth = 120;
    [SerializeField] private float join_uiHeight = 40;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        ready = true;


    }

    private void OnGUI()
    {

        GUI.skin.label.fontSize = _fontSize;
        GUI.skin.button.fontSize = _fontSize;
        GUI.skin.textField.fontSize = _fontSize;

        if (!ready)
        {
            GUI.Label(new Rect(ready_uiPosition_x, ready_uiPosition_y, ready_uiWidth, ready_uiHeight), "Connecting...");
            return;
        }

        if (hostCode != "")
        {
            GUI.Label(new Rect(joined_uiPosition_x, joined_uiPosition_y, joined_uiWidth, joined_uiHeight),
            "Join Code: " + hostCode);
            return;
        }

        if (joined)
        {
            GUI.Label(new Rect(_uiPosition_x + joined_uiPosition_x, _uiPosition_y + joined_uiPosition_y, _uiWidth + joined_uiWidth, _uiHeight + joined_uiHeight), "Connected!");
            return;
        }

        if (GUI.Button(new Rect(host_uiPosition_x + _uiPosition_x, _uiPosition_y + host_uiPosition_y, _uiWidth + host_uiWidth, _uiHeight + host_uiHeight), "Host"))
        {
            Host();
        }
            

        joinCode = GUI.TextField(new Rect(_uiPosition_x + code_uiPosition_x, _uiPosition_y + code_uiPosition_y, _uiWidth + code_uiWidth, _uiHeight + code_uiHeight), joinCode);

        if (GUI.Button(new Rect(_uiPosition_x + join_uiPosition_x, _uiPosition_y + join_uiPosition_y, _uiWidth + join_uiWidth, _uiHeight + join_uiHeight), "Join"))
        {
            Join();
        }  
    }

    private async void Host()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 12
        }
        .WithRelayNetwork()
        .WithNetworkOptions(new NetworkOptions
        {
            RelayProtocol = RelayProtocol.WSS
        });

        var session =
            await MultiplayerService.Instance.CreateSessionAsync(options);

        hostCode = session.Code;
    }

    private async void Join()
    {
        var options = new JoinSessionOptions()
            .WithNetworkOptions(new NetworkOptions
            {
                RelayProtocol = RelayProtocol.WSS
            });

        await MultiplayerService.Instance.JoinSessionByCodeAsync(
            joinCode,
            options
        );

        joined = true;
    }
}
