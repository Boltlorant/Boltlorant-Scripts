using UnityEngine;
using Bolt;

public class PlayerCallback : EntityEventListener<IPlayerState>
{
    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
        _playerController = GetComponent<PlayerController>();
    }

    public override void Attached()
    {
        state.AddCallback("LifePoints", UpdatePlayerLife);
        state.AddCallback("Pitch", _playerMotor.SetPitch);
        state.AddCallback("Energy", UpdateEnergy);
        state.AddCallback("WeaponIndex", UpdateWeaponIndex);
        state.AddCallback("Weapons[].ID", UpdateWeaponList);
        state.AddCallback("Weapons[].CurrentAmmo", UpdateWeaponAmmo);
        state.AddCallback("Weapons[].TotalAmmo", UpdateWeaponAmmo);


        if (entity.IsOwner)
        {
            state.LifePoints = _playerMotor.TotalLife;
            state.Energy = 6;
        }
    }

    public void UpdateWeaponList(IState state, string propertyPath, ArrayIndices arrayIndices)
    {
        int index = arrayIndices[0];
        IPlayerState s = (IPlayerState)state;
        if (s.Weapons[index].ID == -1)
            _playerWeapons.RemoveWeapon(index);
        else
            _playerWeapons.AddWeapon((WeaponID)s.Weapons[index].ID);
    }

    public void UpdateWeaponAmmo(IState state, string propertyPath, ArrayIndices arrayIndices)
    {
        int index = arrayIndices[0];
        IPlayerState s = (IPlayerState)state;
        _playerWeapons.InitAmmo(index, s.Weapons[index].CurrentAmmo, s.Weapons[index].TotalAmmo);
    }

    public void UpdateWeaponIndex()
    {
        _playerController.Wheel = state.WeaponIndex;
        _playerWeapons.SetWeapon(state.WeaponIndex);
    }

    public void FireEffect(float precision, int seed)
    {
        FireEffectEvent evnt = FireEffectEvent.Create(entity, EntityTargets.EveryoneExceptOwnerAndController);
        evnt.Precision = precision;
        evnt.Seed = seed;
        evnt.Send();
    }

    public override void OnEvent(FireEffectEvent evnt)
    {
        _playerWeapons.FireEffect(evnt.Seed, evnt.Precision);
    }

    public void UpdatePlayerLife()
    {
        if(entity.HasControl)
            GUI_Controller.Current.UpdateLife(state.LifePoints, _playerMotor.TotalLife);
    }

    public void UpdateEnergy()
    {
        if (entity.HasControl)
        {
            GUI_Controller.Current.UpdateAbilityView(state.Energy);
        }
    }
}
