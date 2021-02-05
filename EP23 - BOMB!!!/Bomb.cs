using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Weapon
{
    private GameController _gameController = null;

    public override void Init(PlayerWeapons pw, int index)
    {
        base.Init(pw, 3);
        if (_gameController == null)
            _gameController = FindObjectOfType<GameController>();
    }

    private void OnEnable()
    {
        if (_playerMotor)
            if (_playerMotor.entity.HasControl)
                GUI_Controller.Current.HideAmmo();
    }

    protected override void _Fire(int seed)
    {

    }
}
