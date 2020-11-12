using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    [SerializeField]
    private Camera _cam = null;
    [SerializeField]
    private Weapon _weapons = null;

    public Camera Cam { get => _cam; }

    public void Init()
    {
        _weapons.Init(this);
    }

    public void ExecuteCommand(bool fire, bool aiming, bool reload, int seed)
    {
        _weapons.ExecuteCommand(fire, aiming, reload, seed);
    }

    public void FireEffect(int seed, float precision)
    {
        _weapons.FireEffect(seed, precision);
    }
}
