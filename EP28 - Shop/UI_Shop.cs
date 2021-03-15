using UnityEngine;
using UnityEngine.UI;


public class UI_Shop : MonoBehaviour
{
    [SerializeField]
    private ShopItem[] shopItems;
    [SerializeField]
    private Text _energyCount;
    [SerializeField]
    private UI_ShopItem _energyButton;
    private int[] _energyCost = { 300, 600, 900, 1200, 2000, 2000, 2000 };
    private string[] _energyView = { "", "*", "**", "***", "****", "*****", "******" };
    private int _currentEnergy = 0;

    void Start()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i].button.Init(shopItems[i].cost);
        }
    }

    public void BuyWeapon(int index)
    {
        GameController.Current.localPlayer.GetComponent<PlayerCallback>().RaiseBuyWeaponEvent(index);
    }

    public void BuyEnergy()
    {
        GameController.Current.localPlayer.GetComponent<PlayerCallback>().RaiseBuyEnergyEvent();
    }

    public int ItemCost(int index)
    {
        return shopItems[index].cost;
    }

    public WeaponID ItemID(int index)
    {
        return shopItems[index].ID;
    }

    public int EnergyCost()
    {
        return _energyCost[_currentEnergy];
    }

    public void UpdateView(int energy, int money)
    {
        _currentEnergy = energy;

        _energyCount.text = _energyView[energy];
        _energyButton.Init(_energyCost[energy]);
        _energyButton.Interactable(_energyCost[energy] <= money);

        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i].button.Interactable(shopItems[i].cost <= money);
        }
    }
}

[System.Serializable]
public struct ShopItem
{
    public UI_ShopItem button;
    public WeaponID ID;
    public int cost;
}