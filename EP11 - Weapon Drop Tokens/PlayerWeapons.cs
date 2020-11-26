using UnityEngine;

public class PlayerWeapons : Bolt.EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private Camera _cam = null;
    [SerializeField]
    private Weapon[] _weapons = null;
    private int _weaponIndex = 0;

    private WeaponID _primairy = WeaponID.None;
    private WeaponID _secondary = WeaponID.None;

    public int WeaponIndex { get => _weaponIndex; }
    public Camera Cam { get => _cam; }

    public void Init()
    {
        foreach (Weapon w in _weapons)
        {
            if (w)
            {
                w.Init(this);
                if (_weapons[_weaponIndex].WeaponStat.ID < WeaponID.SecondaryEnd)
                    _secondary = _weapons[_weaponIndex].WeaponStat.ID;
                else
                    _primairy = _weapons[_weaponIndex].WeaponStat.ID;
                _weaponIndex++;
            }
        }
        _weaponIndex = 0;

        SetWeapon(_weaponIndex);
    }

    public void ExecuteCommand(bool fire, bool aiming, bool reload, int wheel, int seed, bool drop)
    {
        if (wheel != state.WeaponIndex)
        {
            if (_weapons[wheel] != null)
                if (entity.IsOwner)
                    state.WeaponIndex = wheel;
        }

        if (_weapons[_weaponIndex])
            _weapons[_weaponIndex].ExecuteCommand(fire, aiming, reload, seed);

        DropCurrent(drop);
    }

    public void FireEffect(int seed, float precision)
    {
        _weapons[_weaponIndex].FireEffect(seed, precision);
    }

    public void InitAmmo(int i, int current, int total)
    {
        _weapons[i].InitAmmo(current, total);
    }

    public void SetWeapon(int index)
    {
        _weaponIndex = index;

        for (int i = 0; i < _weapons.Length; i++)
            if (_weapons[i] != null)
                _weapons[i].gameObject.SetActive(false);

        _weapons[_weaponIndex].gameObject.SetActive(true);
    }

    public int CalculateIndex(float valueToAdd)
    {
        int i = _weaponIndex;
        int factor = 0;

        if (valueToAdd > 0)
            factor = 1;
        else if (valueToAdd < 0)
            factor = -1;

        i += factor;

        if (i == -1)
            i = _weapons.Length - 1;

        if (i == _weapons.Length)
            i = 0;

        while (_weapons[i] == null)
        {
            i += factor;
            i = i % _weapons.Length;
        }

        return i;
    }

    bool _dropPressed = false;

    public void DropCurrent(bool drop)
    {
        if (drop)
        {
            if (_dropPressed == false)
            {
                _dropPressed = true;
                DropWeapon();

                if (entity.IsOwner)
                    state.WeaponIndex = CalculateIndex(1);
            }
        }
        else
        {
            if (_dropPressed)
            {
                _dropPressed = false;
            }
        }
    }

    public void DropWeapon()
    {
        if (entity.IsOwner)
        {
            if (_weaponIndex != 0)
            {
                WeaponDropToken token = new WeaponDropToken();
                token.currentAmmo = _weapons[_weaponIndex].CurrentAmmo;
                token.totalAmmo = _weapons[_weaponIndex].TotalAmmo;
                token.ID = _weapons[_weaponIndex].WeaponStat.ID;
                token.networkId = entity.NetworkId;

                BoltNetwork.Instantiate(_weapons[_weaponIndex].WeaponStat.drop, token, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Cam.transform.forward));

                if (_weapons[_weaponIndex].WeaponStat.ID < WeaponID.SecondaryEnd)
                    _secondary = WeaponID.None;
                else
                    _primairy = WeaponID.None;

                state.Weapons[_weaponIndex].ID = -1;
                Destroy(_weapons[_weaponIndex].gameObject);
                _weapons[_weaponIndex] = null;
            }
        }
    }

    public void RemoveWeapon(int i)
    {
        if (_weapons[i])
            Destroy(_weapons[i].gameObject);

        _weapons[i] = null;
    }

    public bool CanAddWeapon(WeaponID toAdd)
    {
        if (toAdd < WeaponID.SecondaryEnd)
        {
            if (_secondary == WeaponID.None)
                return true;
        }
        else
        {
            if (_primairy == WeaponID.None)
                return true;
        }
        return false;
    }

    public void AddWeaponEvent(int i, int ca, int ta)
    {
        if (i < (int)WeaponID.SecondaryEnd)
        {
            state.Weapons[1].ID = i;
            state.Weapons[1].CurrentAmmo = ca;
            state.Weapons[1].TotalAmmo = ta;
        }
        else
        {
            state.Weapons[2].ID = i;
            state.Weapons[2].CurrentAmmo = ca;
            state.Weapons[2].TotalAmmo = ta;
        }
    }
}
