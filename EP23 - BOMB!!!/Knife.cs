using UnityEngine;

public class Knife : Weapon
{
    private static float SPEED_MULTIPLIER = 1.5f;

    private static int DAMAGE_MULTIPLIER = 2;
    private static float BACK_ANGLE_THRESHOLD = 60f;

    private void OnEnable()
    {
        if (_playerMotor)
        {
            _playerMotor.Speed = _playerMotor.SpeedBase * SPEED_MULTIPLIER;
            if (_playerMotor.entity.HasControl)
                GUI_Controller.Current.HideAmmo();
        }
    }

    private void OnDisable()
    {
        _playerMotor.Speed = _playerMotor.SpeedBase;
    }

    protected override void _Fire(int seed)
    {
        if (_fireFrame + _fireInterval <= BoltNetwork.ServerFrame)
        {
            _fireFrame = BoltNetwork.ServerFrame;
            if (_playerCallback.entity.IsOwner)
                _playerCallback.FireEffect(seed, 0);
            FireEffect(seed, 0);

            Ray r = new Ray(_camera.position, _camera.forward);
            RaycastHit rh;

            if (Physics.Raycast(r, out rh, _weaponStat.maxRange))
            {
                PlayerMotor target = rh.transform.GetComponent<PlayerMotor>();
                if (target != null)
                {
                    int dmg = _weaponStat.dmg;
                    if (Vector3.Angle(Vector3.Scale(_camera.forward, new Vector3(1, 0, 1)).normalized, target.transform.forward) < BACK_ANGLE_THRESHOLD)
                        dmg *= DAMAGE_MULTIPLIER;
                    target.Life(_playerMotor, -dmg);
                }
            }
        }
    }
}
