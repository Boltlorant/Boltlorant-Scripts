using UnityEngine;

public class PlayerWeapons : Bolt.EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private Camera _cam = null;
    [SerializeField]
    private Weapon[] _weapons = null;
    private int _weaponIndex = 0;

    public int WeaponIndex { get => _weaponIndex; }
    public Camera Cam { get => _cam; }

    public void Init()
    {
        foreach (Weapon w in _weapons)
        {
            if (w)
            {
                w.Init(this);
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
            BoltNetwork.Instantiate(_weapons[_weaponIndex].WeaponStat.drop, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Cam.transform.forward));

            state.Weapons[_weaponIndex].ID = -1;
            Destroy(_weapons[_weaponIndex].gameObject);
            _weapons[_weaponIndex] = null;
        }
    }

    public void RemoveWeapon(int i)
    {
        if (_weapons[i])
            Destroy(_weapons[i].gameObject);

        _weapons[i] = null;
    }
}
