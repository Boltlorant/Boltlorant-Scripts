using System;
using UdpKit;
using UnityEngine;
using Bolt.Matchmaking;

public class NetworkManager : Bolt.GlobalEventListener
{
    [SerializeField]
    private UnityEngine.UI.Text feedback;

    public void FeedbackUser(string text)
    {
        feedback.text = text;
    }

    public void Connect()
    {
        FeedbackUser("Connnecting ...");
        BoltLauncher.StartClient();
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
