using System;
using UdpKit;
using UnityEngine;
using Bolt.Matchmaking;

public class NetworkManager : Bolt.GlobalEventListener
{
    [SerializeField]
    private UnityEngine.UI.Text feedback;
    [SerializeField]
    private UnityEngine.UI.InputField username;

    private void Awake()
    {
        username.text = AppManager.Current.Username;
    }

    public void FeedbackUser(string text)
    {
        feedback.text = text;
    }

    public void Connect()
    {
        if (username.text != "")
        {
            AppManager.Current.Username = username.text;
            BoltLauncher.StartClient();
            FeedbackUser("Connnecting ...");
        }
        else
            FeedbackUser("Enter a valid name");
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        FeedbackUser("Searching ...");
        BoltMatchmaking.JoinSession(HeadlessServerManager.RoomID());
    }

    public override void Connected(BoltConnection connection)
    {
        FeedbackUser("Connected !");
    }
}
