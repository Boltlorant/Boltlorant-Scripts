using UnityEngine;
using System.Collections;

public class PlayerWeapons : Bolt.EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private Camera _cam = null;
    [SerializeField]
    private Weapon[] _weapons = null;
    private int _weaponIndex = 1;

    [SerializeField]
    private WeaponID _primairyWeapon = WeaponID.None;
    [SerializeField]
    private WeaponID _secondairyWeapon = WeaponID.None;

    private WeaponID _primairy = WeaponID.None;
    private WeaponID _secondary = WeaponID.None;

    public int WeaponIndex { get => _weaponIndex; }
    public Camera Cam { get => _cam; }

    [SerializeField]
    private Transform _weaponsTransform = null;
    [SerializeField]
    private GameObject[] _weaponPrefabs = null;

    public bool HasBomb { get => _weapons[3] != null; }

    public void Init()
    {
        if (entity.IsOwner)
        {
            for (int i = 0; i < 4; i++)
            {
                state.Weapons[i].CurrentAmmo = -1;
            }

            AddWeaponEvent(_primairyWeapon);
            AddWeaponEvent(_secondairyWeapon);

            for (int i = 1; i < 4; i++)
            {
                if (state.Weapons[i].ID == 0)
                    state.Weapons[i].ID = -1;
            }

            state.WeaponIndex = 1;
        }

        StartCoroutine(SetWeapon());
        _weapons[0].Init(this,0);
    }

    IEnumerator SetWeapon()
    {
        while (_weapons[_weaponIndex] == null)
            yield return new WaitForEndOfFrame();
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
        if (_weapons[i] && i != 0)
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
                DropWeapon(_weapons[_weaponIndex].WeaponStat.ID, false);

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

    public void DropWeapon(WeaponID toRemove, bool random)
    {
        if (toRemove == WeaponID.None || toRemove == WeaponID.Knife)
            return;

        if (toRemove == _primairy)
        {
            if (entity.IsOwner)
            {
                WeaponDropToken token = new WeaponDropToken();
                token.ID = toRemove;
                token.currentAmmo = _weapons[2].CurrentAmmo;
                token.totalAmmo = _weapons[2].TotalAmmo;
                token.networkId = entity.NetworkId;

                if (random)
                    BoltNetwork.Instantiate(_weapons[2].WeaponStat.drop, token, Cam.transform.position, Quaternion.identity);
                else
                    BoltNetwork.Instantiate(_weapons[2].WeaponStat.drop, token, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Cam.transform.forward));

                state.Weapons[2].ID = -1;
            }

            Destroy(_weapons[2].gameObject);
            _weapons[2] = null;
            _primairy = WeaponID.None;
        }
        else if (toRemove == _secondary)
        {
            if (entity.IsOwner)
            {
                WeaponDropToken token = new WeaponDropToken();
                token.ID = toRemove;
                token.currentAmmo = _weapons[1].CurrentAmmo;
                token.totalAmmo = _weapons[1].TotalAmmo;
                token.networkId = entity.NetworkId;

                if (random)
                    BoltNetwork.Instantiate(_weapons[1].WeaponStat.drop, token, Cam.transform.position, Quaternion.identity);
                else
                    BoltNetwork.Instantiate(_weapons[1].WeaponStat.drop, token, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Random.onUnitSphere));

                state.Weapons[1].ID = -1;
            }

            Destroy(_weapons[1].gameObject);
            _weapons[1] = null;
            _secondary = WeaponID.None;
        }
        else if (HasBomb)
        {
            if (entity.IsOwner)
            {
                WeaponDropToken token = new WeaponDropToken();
                token.ID = toRemove;
                token.networkId = entity.NetworkId;

                GameObject g = null;

                if (random)
                    g = BoltNetwork.Instantiate(_weapons[3].WeaponStat.drop, token, Cam.transform.position, Quaternion.identity);
                else
                    g = BoltNetwork.Instantiate(_weapons[3].WeaponStat.drop, token, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Random.onUnitSphere));

                state.Weapons[3].ID = -1;
            }

            Destroy(_weapons[3].gameObject);
            _weapons[3] = null;
        }
    }

    public void RemoveWeapon(int i)
    {
        if (_weapons[i])
            Destroy(_weapons[i].gameObject);

        _weapons[i] = null;

        if (i == 2)
            _primairy = WeaponID.None;
        else if (i == 1)
            _secondary = WeaponID.None;
    }

    public void RemoveBomb()
    {
        if (entity.IsOwner)
        {
            state.Weapons[3].ID = -1;
            Destroy(_weapons[3].gameObject);
            _weapons[3] = null;
            state.WeaponIndex = CalculateIndex(1);
        }
    }

    public bool CanAddWeapon(WeaponID toAdd)
    {
        PlayerToken pt = (PlayerToken)entity.AttachToken;
        if (pt.team == Team.AT && toAdd == WeaponID.Bomb)
            return false;

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
        else if (i != (int)WeaponID.Bomb)
        {
            state.Weapons[2].ID = i;
            state.Weapons[2].CurrentAmmo = ca;
            state.Weapons[2].TotalAmmo = ta;
        }
        else
        {
            state.Weapons[3].ID = i;
            state.Weapons[3].CurrentAmmo = ca;
            state.Weapons[3].TotalAmmo = ta;
        }
    }

    void DropOnBuy(WeaponID id, int w)
    {
        if (entity.IsOwner)
        {
            WeaponDropToken token = new WeaponDropToken();
            token.ID = id;
            token.currentAmmo = _weapons[w].CurrentAmmo;
            token.totalAmmo = _weapons[w].TotalAmmo;
            token.networkId = entity.NetworkId;
            BoltNetwork.Instantiate(_weapons[w].WeaponStat.drop, token, Cam.transform.position + Cam.transform.forward, Quaternion.LookRotation(Cam.transform.forward));
        }

        Destroy(_weapons[w].gameObject);
    }

    public void AddWeapon(WeaponID id)
    {
        if (id == WeaponID.None)
            return;

        GameObject prefab = null;
        foreach (GameObject w in _weaponPrefabs)
        {
            if (w.GetComponent<Weapon>().WeaponStat.ID == id)
            {
                prefab = w;
                break;
            }
        }

        prefab = Instantiate(prefab, _weaponsTransform.position, Quaternion.LookRotation(_weaponsTransform.forward), _weaponsTransform);

        if (id < WeaponID.SecondaryEnd)
        {
            if (_secondary != WeaponID.None)
                DropOnBuy(id, 1);

            _secondary = id;
            _weapons[1] = prefab.GetComponent<Weapon>();
            prefab.GetComponent<Weapon>().Init(this, 1);
        }
        else if (id != WeaponID.Bomb)
        {
            if (_primairy != WeaponID.None)
                DropOnBuy(id, 2);

            _primairy = id;
            _weapons[2] = prefab.GetComponent<Weapon>();
            prefab.GetComponent<Weapon>().Init(this, 2);
        }
        else
        {
            _weapons[3] = prefab.GetComponent<Weapon>();
            prefab.GetComponent<Weapon>().Init(this, 3);
        }
    }

    public void AddWeaponEvent(WeaponID id)
    {
        if (id == WeaponID.None)
            return;

        int i = (id < WeaponID.SecondaryEnd) ? 1 : 2;

        if (id == WeaponID.Bomb)
            i = 3;

        state.Weapons[i].ID = (int)id;
    }

    public void RefillWeapon(WeaponID id)
    {
        Weapon prefab = null;

        foreach (GameObject w in _weaponPrefabs)
        {
            if (w.GetComponent<Weapon>().WeaponStat.ID == id)
            {
                prefab = w.GetComponent<Weapon>();
                break;
            }
        }

        int i = 0;

        if (id <= WeaponID.SecondaryEnd)
        {
            i = 1;
        }
        else
        {
            i = 2;
        }

        state.Weapons[i].CurrentAmmo = prefab.WeaponStat.magazin;
        state.Weapons[i].TotalAmmo = prefab.WeaponStat.totalMagazin;
    }

    public void OnDeath(bool b)
    {
        if (b)
        {
            if (entity.IsControllerOrOwner)
            {
                DropWeapon(_primairy, true);
                DropWeapon(_secondary, true);
                DropWeapon(WeaponID.Bomb, true);
            }

            if (entity.IsOwner)
                state.WeaponIndex = 0;
        }
        else
        {
            if (entity.IsOwner)
            {
                if (_secondary == WeaponID.None)
                    AddWeaponEvent(WeaponID.Glock);
                else
                    RefillWeapon(_secondary);

                if (_primairy != WeaponID.None)
                    RefillWeapon(_primairy);

                state.WeaponIndex = 1;
            }
        }
    }
}
