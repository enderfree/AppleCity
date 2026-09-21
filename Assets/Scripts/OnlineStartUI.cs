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

    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        ready = true;
    }

    private void OnGUI()
    {
        if (!ready)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "Connecting...");
            return;
        }

        if (hostCode != "")
        {
            GUI.Label(new Rect(10, 10, 200, 30),
                "Join Code: " + hostCode);
            return;
        }

        if (joined)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "Connected!");
            return;
        }

        if (GUI.Button(new Rect(10, 10, 120, 40), "Host"))
            Host();

        joinCode = GUI.TextField(
            new Rect(10, 60, 120, 30),
            joinCode
        );

        if (GUI.Button(new Rect(10, 100, 120, 40), "Join"))
            Join();
    }

    private async void Host()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 10
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
