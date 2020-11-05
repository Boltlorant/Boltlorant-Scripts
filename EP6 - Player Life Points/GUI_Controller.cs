using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        Show(false);
    }

    public void Show(bool active)
    {
        _healthBar.gameObject.SetActive(active);
    }

    public void UpdateLife(int current, int total)
    {
        _healthBar.UpdateLife(current, total);
    }
}
