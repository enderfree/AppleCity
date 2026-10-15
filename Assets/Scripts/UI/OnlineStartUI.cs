using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

/// <summary>
/// The join and host buttons and related
/// </summary>
public class OnlineStartUI : MonoBehaviour
{
    // the code from the text box to join a game
    private string _joinCode = "";

    // the code that allows people to connect to your game
    private string _hostCode = "";

    // is the player ready to connect
    private bool _ready;

    // has the player joined
    private bool _joined;

    // the size of the font
    [SerializeField] private int _fontSize = 24;

    [Header("Ready")]
    // x position of the ready window
    [SerializeField] private float _ready_uiPosition_x = 10;

    // y position of the ready window
    [SerializeField] private float _ready_uiPosition_y = 10;

    // width of the ready window
    [SerializeField] private float _ready_uiWidth = 200;

    // height of the ready window
    [SerializeField] private float _ready_uiHeight = 30;

    [Header("HostCode/Joined")]
    // x position of the joined window
    [SerializeField] private float _joined_uiPosition_x = 10;

    // y position of the joined window
    [SerializeField] private float _joined_uiPosition_y = 10;

    // width of the joined window
    [SerializeField] private float _joined_uiWidth = 200;

    // height of the joined window
    [SerializeField] private float _joined_uiHeight = 30;

    [Header("Host/Join UI normal")]
    // x position of the host/join window
    [SerializeField] private float _uiPosition_x = 880;

    // y position of the host/join window
    [SerializeField] private float _uiPosition_y = 400;

    // width of the host/join window
    [SerializeField] private float _uiWidth = 220;

    // height of the host/join window
    [SerializeField] private float _uiHeight = 80;

    [Header("Host Button")]
    // x position of the Host button
    [SerializeField] private float _host_uiPosition_x = 10;

    // y position of the Host button
    [SerializeField] private float _host_uiPosition_y = 10;

    // width of the Host button
    [SerializeField] private float _host_uiWidth = 120;

    // height of the Host button
    [SerializeField] private float _host_uiHeight = 40;

    [Header("Code")]
    // x position of the join code field
    [SerializeField] private float _code_uiPosition_x = 10;

    // y position of the join code field
    [SerializeField] private float _code_uiPosition_y = 60;

    // width of the join code field
    [SerializeField] private float _code_uiWidth = 120;

    // height of the join code field
    [SerializeField] private float _code_uiHeight = 30;

    [Header("Join Button")]
    // x position of the join button
    [SerializeField] private float _join_uiPosition_x = 10;

    // y position of the join button
    [SerializeField] private float _join_uiPosition_y = 100;

    // width of the join button
    [SerializeField] private float _join_uiWidth = 120;

    // height of the join button
    [SerializeField] private float _join_uiHeight = 40;

    /// <summary>
    /// Look at that! Monobehavior's Start works async too!
    /// </summary>
    private async void Start()
    {
        Innit();


    }

    /// <summary>
    /// Initialization
    /// </summary>
    private async void Innit()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _ready = true;
    }

    /// <summary>
    /// For some reasons Jun does GUI through code
    /// </summary>
    private void OnGUI()
    {
        DrawGUI();
        
    }

    /// <summary>
    /// Draw the online start GUI
    /// </summary>
    private async void DrawGUI()
    {
        GUI.skin.label.fontSize = _fontSize;
        GUI.skin.button.fontSize = _fontSize;
        GUI.skin.textField.fontSize = _fontSize;

        if (!_ready)
        {
            GUI.Label(new Rect(_ready_uiPosition_x, _ready_uiPosition_y, _ready_uiWidth, _ready_uiHeight), "Connecting...");
            return;
        }

        if (_hostCode != "")
        {
            GUI.Label(new Rect(_joined_uiPosition_x, _joined_uiPosition_y, _joined_uiWidth, _joined_uiHeight),
            "Join Code: " + _hostCode);
            return;
        }

        if (_joined)
        {
            GUI.Label(new Rect(_uiPosition_x + _joined_uiPosition_x, _uiPosition_y + _joined_uiPosition_y, _uiWidth + _joined_uiWidth, _uiHeight + _joined_uiHeight), "Connected!");
            return;
        }

        if (GUI.Button(new Rect(_host_uiPosition_x + _uiPosition_x, _uiPosition_y + _host_uiPosition_y, _uiWidth + _host_uiWidth, _uiHeight + _host_uiHeight), "Host"))
        {
            Host();
        }


        _joinCode = GUI.TextField(new Rect(_uiPosition_x + _code_uiPosition_x, _uiPosition_y + _code_uiPosition_y, _uiWidth + _code_uiWidth, _uiHeight + _code_uiHeight), _joinCode);

        if (GUI.Button(new Rect(_uiPosition_x + _join_uiPosition_x, _uiPosition_y + _join_uiPosition_y, _uiWidth + _join_uiWidth, _uiHeight + _join_uiHeight), "Join"))
        {
            Join();
        }
    }

    /// <summary>
    /// Host button logic
    /// </summary>
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

        _hostCode = session.Code;
    }

    /// <summary>
    /// join button logic
    /// </summary>
    private async void Join()
    {
        var options = new JoinSessionOptions()
            .WithNetworkOptions(new NetworkOptions
            {
                RelayProtocol = RelayProtocol.WSS
            });

        await MultiplayerService.Instance.JoinSessionByCodeAsync(
            _joinCode,
            options
        );

        _joined = true;
    }
}
