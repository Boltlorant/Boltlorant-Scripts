using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GUI_Controller : MonoBehaviour
{
    #region Singleton
    private static GUI_Controller _instance = null;

    public static GUI_Controller Current
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GUI_Controller>();

            return _instance;
        }
    }
    #endregion

    [SerializeField]
    private UI_HealthBar _healthBar = null;

    [SerializeField]
    private UI_AmmoPanel _ammoPanel = null;

    [SerializeField]
    private UI_Cooldown _skill = null;
    [SerializeField]
    private UI_Cooldown _grenade = null;

    [SerializeField]
    private Text _energyCount = null;

    public UI_Cooldown Skill { get => _skill; }
    public UI_Cooldown Grenade { get => _grenade; }
    public Team GuiTeam { set => _guiTeam = value; }

    [SerializeField]
    private Image _blindMask = null;
    Coroutine blind;

    [SerializeField]
    private Text _money = null;

    [SerializeField]
    private UI_PlayerPlate[] _allayPlates = null;
    [SerializeField]
    private UI_PlayerPlate[] _enemyPlates = null;
    [SerializeField]
    private Sprite[] _icons = null;

    Team _guiTeam = Team.AT;
    [SerializeField]
    private Text _allayScore = null;
    [SerializeField]
    private Text _enemyScore = null;

    [SerializeField]
    private UI_Timer _timer = null;

    [SerializeField]
    private Image _bombBar = null;
    [SerializeField]
    private Image _bombPlanted = null;

    [SerializeField]
    private GameObject _scope = null;

    [SerializeField]
    private UI_Shop _shop = null;
    public UI_Shop shop { get => _shop; }

    private void Start()
    {
        Show(false);
    }

    public void Show(bool active)
    {
        _healthBar.gameObject.SetActive(active);
        _ammoPanel.gameObject.SetActive(active);
        _skill.gameObject.SetActive(active);
        _grenade.gameObject.SetActive(active);
        _energyCount.transform.parent.gameObject.SetActive(active);
        _money.transform.parent.gameObject.SetActive(active);

        if (_bombBar.transform.parent.gameObject.activeSelf)
            _bombBar.transform.parent.gameObject.SetActive(active);

        if (_bombPlanted.gameObject.activeSelf)
            _bombPlanted.gameObject.SetActive(active);

        if (_scope.gameObject.activeSelf)
            _scope.gameObject.SetActive(active);

        if (_shop.gameObject.activeSelf)
            _shop.gameObject.SetActive(active);
    }

    public void UpdateLife(int current, int total)
    {
        _healthBar.UpdateLife(current, total);
    }

    public void UpdateAmmo(int current, int total)
    {
        if (!_ammoPanel.gameObject.activeSelf)
            _ammoPanel.gameObject.SetActive(true);

        _ammoPanel.UpdateAmmo(current, total);
    }

    public void HideAmmo()
    {
        _ammoPanel.gameObject.SetActive(false);
    }

    public void UpdateAbilityView(int i)
    {
        _energyCount.text = i.ToString();
        _skill.UpdateCost(i);
        _grenade.UpdateCost(i);
    }

    public void Flash()
    {
        if (blind != null)
            StopCoroutine(blind);
        blind = StartCoroutine(CRT_Blind(3f));
    }

    IEnumerator CRT_Blind(float f)
    {
        float startTime = Time.time;
        while (startTime + f > Time.time)
        {
            _blindMask.color = new Color(1, 1, 1, 1);
            yield return null;
            while (startTime + f - 1 < Time.time && startTime + f > Time.time)
            {
                _blindMask.color = new Color(1, 1, 1, -(Time.time - (startTime + f)));
                yield return null;
            }
        }
        _blindMask.color = new Color(1, 1, 1, 0);
    }

    public void UpdatePlayersPlate(GameObject[] players, GameObject localPlayer)
    {
        PlayerMotor pm;
        PlayerToken pt;

        if (localPlayer != null)
        {
            pm = localPlayer.GetComponent<PlayerMotor>();
            pt = (PlayerToken)pm.entity.AttachToken;

            _allayPlates[(int)pt.playerSquadID].Init(_icons[(int)pt.characterClass]);
            _allayPlates[(int)pt.playerSquadID].Death(pm.state.IsDead);
        }

        foreach (GameObject p in players)
        {
            pm = p.GetComponent<PlayerMotor>();
            pt = (PlayerToken)pm.entity.AttachToken;

            if (pm.IsEnemy)
            {
                _enemyPlates[(int)pt.playerSquadID].Init(_icons[(int)pt.characterClass]);
                _enemyPlates[(int)pt.playerSquadID].Death(pm.state.IsDead);
            }
            else
            {
                _allayPlates[(int)pt.playerSquadID].Init(_icons[(int)pt.characterClass]);
                _allayPlates[(int)pt.playerSquadID].Death(pm.state.IsDead);
            }
        }
    }

    public void UpdatePoints(int AT, int TT)
    {
        if (_guiTeam == Team.AT)
        {
            _allayScore.text = AT.ToString();
            _enemyScore.text = TT.ToString();
        }
        else
        {
            _allayScore.text = TT.ToString();
            _enemyScore.text = AT.ToString();
        }
    }

    public void UpdateTimer(float f)
    {
        _timer.Init(f);
    }

    public void PlantingProgressShow(bool b)
    {
        _bombBar.transform.parent.gameObject.SetActive(b);
    }

    public void PlantingProgress(float b)
    {
        _bombBar.fillAmount = b;
    }

    public void Planted(bool b)
    {
        _bombPlanted.gameObject.SetActive(b);
    }
    public void UpdateMoney(int i)
    {
        _money.text = "$ " + i;
    }

    public void ShowScope(bool show)
    {
        _scope.SetActive(show);
    }

    public void ShowShop(bool show)
    {
        _shop.gameObject.SetActive(show);
    }

    public void UpdateShop(int e, int m)
    {
        _shop.UpdateView(e, m);
    }
}
