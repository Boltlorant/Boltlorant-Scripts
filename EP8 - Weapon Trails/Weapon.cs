using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    protected Transform _camera;
    [SerializeField]
    protected WeaponStats _weaponStat = null;
    protected int _currentAmmo = 0;
    protected int _currentTotalAmmo = 0;
    protected bool _isReloading = false;
    protected PlayerWeapons _playerWeapons;
    protected PlayerMotor _playerMotor;
    protected PlayerCallback _playerCallback;

    protected int _fireFrame = 0;
    private Coroutine _reloadCrt = null;
    protected Dictionary<PlayerMotor, int> _dmgCounter;
    [SerializeField]
    private GameObject _renderer;
    [SerializeField]
    private Transform _muzzle;

    protected int _fireInterval
    {
        get
        {
            int rps = _weaponStat.rpm / 60;
            return BoltNetwork.FramesPerSecond / rps;
        }
    }

    public WeaponStats WeaponStat { get => _weaponStat; }
    public int CurrentAmmo { get => _currentAmmo; }
    public int TotalAmmo { get => _currentTotalAmmo; }

    public virtual void Init(PlayerWeapons pw)
    {
        _playerWeapons = pw;
        _playerMotor = pw.GetComponent<PlayerMotor>();
        _playerCallback = pw.GetComponent<PlayerCallback>();
        _camera = _playerWeapons.Cam.transform;

        if (!_playerMotor.entity.HasControl)
            _renderer.gameObject.layer = 0;

        _currentAmmo = _weaponStat.magazin;
        _currentTotalAmmo = _weaponStat.totalMagazin;
    }

    public virtual void ExecuteCommand(bool fire, bool aiming, bool reload, int seed)
    {
        if (!_isReloading)
        {
            if (reload && _currentAmmo != _weaponStat.magazin && _currentTotalAmmo > 0)
            {
                _Reload();
            }
            else
            {
                if (fire)
                {
                    _Fire(seed);
                }
            }
        }
    }

    protected virtual void _Fire(int seed)
    {
        if (_currentAmmo >= _weaponStat.ammoPerShot)
        {
            if (_fireFrame + _fireInterval <= BoltNetwork.ServerFrame)
            {
                int dmg = 0;
                _fireFrame = BoltNetwork.ServerFrame;

                if (_playerCallback.entity.IsOwner)
                    _playerCallback.FireEffect(WeaponStat.precision, seed);

                if (_playerCallback.entity.HasControl)
                    FireEffect(seed, WeaponStat.precision);

                _currentAmmo -= _weaponStat.ammoPerShot;
                Random.InitState(seed);

                _dmgCounter = new Dictionary<PlayerMotor, int>();
                for (int i = 0; i < _weaponStat.multiShot; i++)
                {
                    Vector2 rnd = Random.insideUnitCircle * WeaponStat.precision;
                    Ray r = new Ray(_camera.position, (_camera.forward * 10f) + (_camera.up * rnd.y) + (_camera.right * rnd.x));
                    RaycastHit rh;

                    if (Physics.Raycast(r, out rh, _weaponStat.maxRange))
                    {
                        PlayerMotor target = rh.transform.GetComponent<PlayerMotor>();
                        if (target != null)
                        {
                            if (target.IsHeadshot(rh.collider))
                                dmg = (int)(_weaponStat.dmg * 1.5f);
                            else
                                dmg = _weaponStat.dmg;

                            if (!_dmgCounter.ContainsKey(target))
                                _dmgCounter.Add(target, dmg);
                            else
                                _dmgCounter[target] += dmg;
                        }
                    }
                }

                foreach (PlayerMotor pm in _dmgCounter.Keys)
                    pm.Life(_playerMotor, -_dmgCounter[pm]);
            }
        }
        else if (_currentTotalAmmo > 0)
        {
            _Reload();
        }
    }

    public virtual void FireEffect(int seed, float precision)
    {
        Random.InitState(seed);

        for (int i = 0; i < _weaponStat.multiShot; i++)
        {
            Vector2 rnd = Random.insideUnitSphere * precision;
            Ray r = new Ray(_camera.position, _camera.forward + (_camera.up * rnd.y) + (_camera.right * rnd.x));
            RaycastHit rh;

            if (Physics.Raycast(r, out rh))
            {
                if (_weaponStat.impact)
                    Instantiate(_weaponStat.impact, rh.point, Quaternion.LookRotation(rh.normal));

                if (_weaponStat.decal)
                    if (!rh.rigidbody)
                        Instantiate(_weaponStat.decal, rh.point, Quaternion.LookRotation(rh.normal));

                if (_weaponStat.trail)
                {
                    var trailGo = Instantiate(_weaponStat.trail, _muzzle.position, Quaternion.identity);
                    var trail = trailGo.GetComponent<LineRenderer>();

                    trail.SetPosition(0, _muzzle.position);
                    trail.SetPosition(1, rh.point);
                }
            }
            else if (_weaponStat.trail)
            {
                var trailGo = Instantiate(_weaponStat.trail, _muzzle.position, Quaternion.identity);
                var trail = trailGo.GetComponent<LineRenderer>();

                trail.SetPosition(0, _muzzle.position);
                trail.SetPosition(1, r.direction * _weaponStat.maxRange + _camera.position);
            }
        }
    }

    protected void _Reload()
    {
        _reloadCrt = StartCoroutine(Reloading());
    }

    IEnumerator Reloading()
    {
        _isReloading = true;
        yield return new WaitForSeconds(_weaponStat.reloadTime);
        _currentTotalAmmo += _currentAmmo;
        int _ammo = Mathf.Min(_currentTotalAmmo, _weaponStat.magazin);
        _currentTotalAmmo -= _ammo;
        _currentAmmo = _ammo;
        _isReloading = false;
    }
}
