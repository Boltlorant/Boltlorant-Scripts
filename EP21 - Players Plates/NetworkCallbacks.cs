[BoltGlobalBehaviour]
public class NetworkCallbacks : Bolt.GlobalEventListener
{
    public override void BoltStartBegin()
    {
        BoltNetwork.RegisterTokenClass<Bolt.Photon.PhotonRoomProperties>();
        BoltNetwork.RegisterTokenClass<WeaponDropToken>();
        BoltNetwork.RegisterTokenClass<PlayerToken>();
    }

    public override void SceneLoadLocalDone(string scene, Bolt.IProtocolToken token)
    {
        if (BoltNetwork.IsServer)
        {
            if (scene == HeadlessServerManager.Map())
            {
                if (!GameController.Current)
                    BoltNetwork.Instantiate(BoltPrefabs.GameController);
            }
        }
    }
}
