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
    protected PlayerController _playerController;

    protected int _fireFrame = 0;
    private Coroutine _reloadCrt = null;
    protected Dictionary<PlayerMotor, int> _dmgCounter;
    [SerializeField]
    private GameObject _renderer;
    [SerializeField]
    private Transform _muzzle;

    protected float _basePrecision = 0;
    protected float _precision = 0;
    private bool _scoping = false;
    private float _baseSensitivity;
    private float _scopeSensitivity;

    protected int _fireInterval
    {
        get
        {
            int rps = _weaponStat.rpm / 60;
            return BoltNetwork.FramesPerSecond / rps;
        }
    }

    public WeaponStats WeaponStat { get => _weaponStat; }

    public int CurrentAmmo
    {
        get => _currentAmmo;
        set
        {
            if (_playerMotor.entity.IsOwner)
                _playerMotor.state.Weapons[_playerWeapons.WeaponIndex].CurrentAmmo = value;
            _currentAmmo = value;
        }
    }

    public int TotalAmmo
    {
        get => _currentTotalAmmo;
        set
        {
            if (_playerMotor.entity.IsOwner)
                _playerMotor.state.Weapons[_playerWeapons.WeaponIndex].TotalAmmo = value;
            _currentTotalAmmo = value;
        }
    }

    public virtual void Init(PlayerWeapons pw, int index)
    {
        _playerWeapons = pw;
        _playerMotor = pw.GetComponent<PlayerMotor>();
        _playerCallback = pw.GetComponent<PlayerCallback>();
        _playerController = pw.GetComponent<PlayerController>();
        _camera = _playerWeapons.Cam.transform;

        _baseSensitivity = _playerController.mouseSensitivity;
        _scopeSensitivity = _baseSensitivity * _weaponStat.scopeSensitivity;

        _precision = WeaponStat.precision;

        if (_playerMotor.state.Weapons[index].CurrentAmmo != -1)
        {
            _currentAmmo = _playerMotor.state.Weapons[index].CurrentAmmo;
            _currentTotalAmmo = _playerMotor.state.Weapons[index].TotalAmmo;
        }
        else
        {
            _currentAmmo = _weaponStat.magazin;
            _currentTotalAmmo = _weaponStat.totalMagazin;
            if (_playerMotor.entity.IsOwner)
            {
                _playerMotor.state.Weapons[index].CurrentAmmo = _currentAmmo;
                _playerMotor.state.Weapons[index].TotalAmmo = _currentTotalAmmo;
            }
        }

        if (!_playerMotor.entity.HasControl)
            _renderer.gameObject.layer = 0;
    }

    private void OnEnable()
    {
        if (_playerWeapons)
        {
            if (_playerWeapons.entity.IsControllerOrOwner)
            {
                if (CurrentAmmo == 0)
                    _reloadCrt = StartCoroutine(Reloading());
            }

            if (_playerWeapons.entity.HasControl)
            {
                GUI_Controller.Current.UpdateAmmo(CurrentAmmo, TotalAmmo);
            }
        }
    }

    private void OnDisable()
    {
        if (_isReloading)
        {
            _isReloading = false;
            StopCoroutine(_reloadCrt);
        }
    }

    public virtual void ExecuteCommand(bool fire, bool aiming, bool reload, int seed)
    {
        if (!_isReloading)
        {
            if (reload && CurrentAmmo != _weaponStat.magazin && TotalAmmo > 0)
            {
                _Reload();
            }
            else
            {
                if (fire)
                {
                    _Fire(seed);
                }

                if (_weaponStat.canScope)
                {
                    if (_scoping != aiming)
                    {
                        _precision = _weaponStat.precision * (aiming ? _weaponStat.scopePrecision : 1f);
                        _scoping = aiming;
                        _Aiming(_scoping);
                    }
                }
            }
        }
    }

    protected virtual void _Fire(int seed)
    {
        if (CurrentAmmo >= _weaponStat.ammoPerShot)
        {
            if (_fireFrame + _fireInterval <= BoltNetwork.ServerFrame)
            {
                int dmg = 0;
                _fireFrame = BoltNetwork.ServerFrame;

                if (_playerCallback.entity.IsOwner)
                    _playerCallback.FireEffect(_precision, seed);

                if (_playerCallback.entity.HasControl)
                    FireEffect(seed, _precision);

                CurrentAmmo -= _weaponStat.ammoPerShot;
                Random.InitState(seed);

                _dmgCounter = new Dictionary<PlayerMotor, int>();
                for (int i = 0; i < _weaponStat.multiShot; i++)
                {
                    Vector2 rnd = Random.insideUnitCircle * _precision;
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
        else if (TotalAmmo > 0)
        {
            _Reload();
        }
    }

    private void _Aiming(bool aim)
    {
        if (_playerWeapons.entity.HasControl)
        {
            GUI_Controller.Current.ShowScope(aim);
            _renderer.SetActive(!aim);

            if (aim)
            {
                _camera.GetComponent<Camera>().fieldOfView = 40;
                _playerController.mouseSensitivity = _scopeSensitivity;
            }
            else
            {
                _camera.GetComponent<Camera>().fieldOfView = 75;
                _playerController.mouseSensitivity = _baseSensitivity;
            }
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

    public virtual void InitAmmo(int current, int total)
    {
        _currentAmmo = current;
        _currentTotalAmmo = total;
        if (_playerCallback.entity.HasControl)
            GUI_Controller.Current.UpdateAmmo(current, total);
    }

    protected void _Reload()
    {
        _reloadCrt = StartCoroutine(Reloading());
    }

    IEnumerator Reloading()
    {
        _isReloading = true;
        yield return new WaitForSeconds(_weaponStat.reloadTime);
        TotalAmmo += CurrentAmmo;
        int _ammo = Mathf.Min(TotalAmmo, _weaponStat.magazin);
        TotalAmmo -= _ammo;
        CurrentAmmo = _ammo;
        _isReloading = false;
    }
}
