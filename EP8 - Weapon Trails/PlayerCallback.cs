using UnityEngine;
using Bolt;

public class PlayerCallback : EntityEventListener<IPlayerState>
{
    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
    }

    public override void Attached()
    {
        state.AddCallback("LifePoints", UpdatePlayerLife);
        state.AddCallback("Pitch", _playerMotor.SetPitch);

        if (entity.IsOwner)
        {
            state.LifePoints = _playerMotor.TotalLife;
        }
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
}
