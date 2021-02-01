using UnityEngine;
using Bolt;

public class GameController : EntityEventListener<IGameModeState>
{
    #region Singleton
    private static GameController _instance = null;

    public static GameController Current
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameController>();

            return _instance;
        }
    }
    #endregion

    GamePhase _currentPhase = GamePhase.WaitForPlayers;
    int _playerCountTarget = 2;
    float _nextEvent = 0;
    GameObject _walls;
    Team _roundWinner = Team.None;
    int _winningRoundAmmount = 10;

    public GamePhase CurrentPhase { get => _currentPhase; }

    private void Start()
    {
        _walls = GameObject.Find("__Walls");
    }

    public override void Attached()
    {
        state.AddCallback("AlivePlayers", UpdatePlayersAlive);
        state.AddCallback("TTPoints", UpdatePoints);
        state.AddCallback("ATPoints", UpdatePoints);
        state.AddCallback("Timer", UpdateTime);
    }

    public void UpdatePoints()
    {
        GUI_Controller.Current.UpdatePoints(state.ATPoints, state.TTPoints);

        if (_winningRoundAmmount == state.ATPoints || _winningRoundAmmount == state.TTPoints)
        {
            _currentPhase = GamePhase.EndGame;
            UpdateGameState();
        }
    }

    public void UpdateTime()
    {
        GUI_Controller.Current.UpdateTimer(state.Timer);
    }


    public void SetWalls(bool b)
    {
        SetWallsEvent evnt = SetWallsEvent.Create(entity);
        evnt.Set = b;
        evnt.Send();
    }

    public override void OnEvent(SetWallsEvent evnt)
    {
        for (int i = 0; i < _walls.transform.childCount; i++)
        {
            _walls.transform.GetChild(i).gameObject.SetActive(evnt.Set);
        }
    }

    public void UpdatePlayersAlive()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (entity.IsOwner)
        {
            int ATCount = 0;
            int TTCount = 0;

            foreach (GameObject player in players)
            {
                PlayerToken pt = (PlayerToken)player.GetComponent<PlayerMotor>().entity.AttachToken;
                if (!player.GetComponent<PlayerMotor>().state.IsDead)
                {
                    if (pt.team == Team.AT)
                        ATCount++;
                    else
                        TTCount++;
                }
            }
            _roundWinner = Team.None;

            if (_currentPhase == GamePhase.AT_Defending)
            {
                if (ATCount == 0)
                {
                    state.TTPoints++;
                    _nextEvent = BoltNetwork.ServerTime + 10f;
                    state.Timer = 10f;
                    _currentPhase = GamePhase.EndRound;
                    _roundWinner = Team.TT;
                }

                if (TTCount == 0)
                {
                    state.ATPoints++;
                    _nextEvent = BoltNetwork.ServerTime + 10f;
                    state.Timer = 10f;
                    _currentPhase = GamePhase.EndRound;
                    _roundWinner = Team.AT;
                }
            }

            if (GamePhase.WaitForPlayers == _currentPhase)
            {
                foreach (GameObject player in players)
                {
                    player.GetComponent<PlayerCallback>().RoundReset(Team.None);
                }
            }

            if (_currentPhase == GamePhase.TT_Planted)
            {
                //TODO
            }
        }

        GameObject lp = GameObject.FindGameObjectWithTag("LocalPlayer");
        GUI_Controller.Current.UpdatePlayersPlate(players, lp);
    }

    public void UpdateGameState()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        switch (_currentPhase)
        {
            case GamePhase.WaitForPlayers:
                if (_playerCountTarget == players.Length)
                {
                    _currentPhase = GamePhase.Starting;
                    _nextEvent = BoltNetwork.ServerTime + 10f;
                    state.Timer = 10f;
                }
                break;
            case GamePhase.Starting:
                break;
            case GamePhase.StartRound:
                SetWalls(true);
                GameObject[] drops = GameObject.FindGameObjectsWithTag("Drop");

                foreach (GameObject drop in drops)
                {
                    BoltNetwork.Destroy(drop.GetComponent<BoltEntity>());
                }

                foreach (GameObject player in players)
                {
                    player.GetComponent<PlayerCallback>().RoundReset(_roundWinner);
                }
                _nextEvent = BoltNetwork.ServerTime + 10f;
                state.Timer = 10f;
                break;
            case GamePhase.AT_Defending:
                SetWalls(false);
                break;
            case GamePhase.TT_Planted:
                break;
            case GamePhase.EndRound:
                break;
            case GamePhase.EndGame:
                //TODO WINNER
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        switch (_currentPhase)
        {
            case GamePhase.WaitForPlayers:
                break;
            case GamePhase.Starting:
                if (_nextEvent < BoltNetwork.ServerTime)
                {
                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                    foreach (GameObject player in players)
                    {
                        player.GetComponent<PlayerCallback>().RoundReset(Team.None);
                    }

                    _nextEvent = BoltNetwork.ServerTime + 15f;
                    state.Timer = 15f;
                    _currentPhase = GamePhase.StartRound;
                    UpdateGameState();
                }
                break;
            case GamePhase.StartRound:
                if (_nextEvent < BoltNetwork.ServerTime)
                {
                    _nextEvent = BoltNetwork.ServerTime + 180f;
                    state.Timer = 180f;
                    _currentPhase = GamePhase.AT_Defending;
                    UpdateGameState();
                }
                break;
            case GamePhase.AT_Defending:
                if (_nextEvent < BoltNetwork.ServerTime)
                {
                    _nextEvent = BoltNetwork.ServerTime + 10f;
                    state.Timer = 10f;
                    _currentPhase = GamePhase.EndRound;
                    state.ATPoints++;
                    _roundWinner = Team.AT;
                }
                break;
            case GamePhase.TT_Planted:
                //TODO
                break;
            case GamePhase.EndRound:
                if (_nextEvent < BoltNetwork.ServerTime)
                {
                    _nextEvent = BoltNetwork.ServerTime + 15f;
                    state.Timer = 15f;
                    _currentPhase = GamePhase.StartRound;
                    UpdateGameState();
                }
                break;
            case GamePhase.EndGame:
                break;
            default:
                break;
        }

    }
}

public enum GamePhase
{
    WaitForPlayers,
    Starting,
    StartRound,
    AT_Defending,
    TT_Planted,
    EndRound,
    EndGame
}