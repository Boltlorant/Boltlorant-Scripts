using UnityEngine;
using UnityEngine.UI;

public class UI_AmmoPanel : MonoBehaviour
{
    [SerializeField]
    private Text _current = null;
    [SerializeField]
    private Text _total = null;
    public void UpdateAmmo(int current, int total)
    {
        _current.text = current.ToString();
        _total.text = total.ToString();
    }
}
