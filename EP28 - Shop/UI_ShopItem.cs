using UnityEngine;
using UnityEngine.UI;

public class UI_ShopItem : MonoBehaviour
{
    [SerializeField]
    private Text _title;
    [SerializeField]
    private Text _Textcost;
    [SerializeField]
    private Image _icon;

    public void Init(int cost)
    {
        _Textcost.text = "$ " + cost;
    }

    public void Interactable(bool b)
    {
        GetComponent<Button>().interactable = b;

        if (!b)
        {
            if (_icon)
                _icon.color = Color.white;
            _title.color = Color.white;
            _Textcost.color = Color.red;
        }
        else
        {
            if (_icon)
                _icon.color = Color.black;
            _title.color = Color.black;
            _Textcost.color = Color.green;
        }
    }
}
