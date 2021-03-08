using UnityEngine;

public class LauncherWeapon : Weapon
{
    [SerializeField]
    private GameObject _rocket = null;

    protected override void _Fire(int seed)
    {
        if (_currentAmmo > 0)
        {
            if (_fireFrame + _fireInterval <= BoltNetwork.ServerFrame)
            {
                _fireFrame = BoltNetwork.ServerFrame;

                if (_playerCallback.entity.IsOwner)
                    _playerCallback.FireEffect(WeaponStat.precision, seed);

                if (_playerCallback.entity.HasControl)
                    FireEffect(seed, WeaponStat.precision);

                CurrentAmmo -= _weaponStat.ammoPerShot;

                if (_playerWeapons.entity.IsOwner)
                {
                    GameObject g = BoltNetwork.Instantiate(_rocket, _camera.position + _camera.forward, _camera.rotation);
                    g.GetComponent<Rocket>().Init(_playerMotor);
                }
            }
        }
        else if (_currentTotalAmmo > 0)
        {
            base._Reload();
        }
    }

    public override void FireEffect(int seed, float precision)
    {

    }
}
