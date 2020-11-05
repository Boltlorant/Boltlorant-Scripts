using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField]
    private Gradient _gradient = null;

    [SerializeField]
    private Image _bg = null;
    [SerializeField]
    private Image _bar = null;
    [SerializeField]
    private Text _text = null;

    public void UpdateLife(int hp, int totalHp)
    {
        float f = (float)hp / (float)totalHp;
        _bar.fillAmount = f;
        Color c = _gradient.Evaluate(f);
        _bg.color = new Color(c.r, c.g, c.b, _bg.color.a);
        _bar.color = c;
        _text.text = hp.ToString();
    }
}
