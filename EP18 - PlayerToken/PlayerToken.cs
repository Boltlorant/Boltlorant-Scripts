public class PlayerToken : Bolt.IProtocolToken
{
    public string name;
    public Team team;
    public CharacterClass characterClass;
    public PlayerSquadID playerSquadID;


    public void Write(UdpKit.UdpPacket packet)
    {
        packet.WriteString(name);
        packet.WriteShort((short)team);
        packet.WriteShort((short)characterClass);
        packet.WriteShort((short)playerSquadID);
    }

    public void Read(UdpKit.UdpPacket packet)
    {
        name = packet.ReadString();
        team = (Team)packet.ReadShort();
        characterClass = (CharacterClass)packet.ReadShort();
        playerSquadID = (PlayerSquadID)packet.ReadShort();
    }
}

public enum PlayerSquadID
{
    Orange,
    Blue,
    Green,
    Purple,
    Pink
}

public enum CharacterClass
{
    Soldier,
    Medic,
    Heavy
}

public enum Team
{
    TT,
    AT,
    None
}
