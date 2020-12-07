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
}
