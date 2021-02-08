using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiteController : MonoBehaviour
{
    List<PlayerWeapons> _playersIn = new List<PlayerWeapons>();
    private bool _isPlayerIn = false;
    public bool IsPlayerIn { get => _isPlayerIn; }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<PlayerWeapons>())
        {
            if (!_playersIn.Contains(col.GetComponent<PlayerWeapons>()) && (col.gameObject.tag == "Player" || col.gameObject.tag == "LocalPlayer"))
            {
                _playersIn.Add(col.GetComponent<PlayerWeapons>());
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<PlayerWeapons>())
        {
            if (_playersIn.Contains(col.GetComponent<PlayerWeapons>()))
            {
                _playersIn.Remove(col.GetComponent<PlayerWeapons>());
            }
        }
    }

    void FixedUpdate()
    {
        _isPlayerIn = false;

        foreach (PlayerWeapons pw in _playersIn)
        {
            if (pw.HasBomb)
                _isPlayerIn = true;
        }
    }

    public void RoundReset()
    {
        _playersIn = new List<PlayerWeapons>();
    }
}
