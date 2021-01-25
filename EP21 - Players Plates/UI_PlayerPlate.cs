using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerPlate : MonoBehaviour
{
    private Color _color;
    private Image _background;
    private Image _icon;

    private void Start()
    {
        _background = GetComponent<Image>();
        _icon = transform.GetChild(0).GetComponent<Image>();
        _color = _background.color;
    }

    public void Init(Sprite s)
    {
        _icon.sprite = s;
        if (_icon.sprite != null)
            Active(true);
    }

    public void Active(bool t)
    {
        _background.enabled = t;
        _icon.enabled = t;
    }

    public void Death(bool b)
    {
        if (b)
        {
            _background.color = new Color(_color.r, _color.g, _color.b, 0.5f);
            _icon.color = new Color(1f, 1f, 1f, 0.2f);
        }
        else
        {
            _background.color = _color;
            _icon.color = Color.white;
        }
    }
}
