using UnityEngine;
using Bolt;
using System.Collections;

public class PlayerCallback : EntityEventListener<IPlayerState>
{
    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private PlayerController _playerController;
    private PlayerRenderer _playerRenderer;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
        _playerController = GetComponent<PlayerController>();
        _playerRenderer = GetComponent<PlayerRenderer>();
    }

    public override void Attached()
    {
        state.AddCallback("LifePoints", UpdatePlayerLife);
        state.AddCallback("Pitch", _playerMotor.SetPitch);
        state.AddCallback("Energy", UpdateEnergy);
        state.AddCallback("IsDead", UpdateDeathState);
        state.AddCallback("WeaponIndex", UpdateWeaponIndex);
        state.AddCallback("Weapons[].ID", UpdateWeaponList);
        state.AddCallback("Weapons[].CurrentAmmo", UpdateWeaponAmmo);
        state.AddCallback("Weapons[].TotalAmmo", UpdateWeaponAmmo);


        if (entity.IsOwner)
        {
            state.IsDead = false;
            state.LifePoints = _playerMotor.TotalLife;
            state.Energy = 6;
            GameController.Current.UpdateGameState();
            GameController.Current.state.AlivePlayers++;
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

    public void RaiseFlashEvent()
    {
        FlashEvent evnt = FlashEvent.Create(entity, EntityTargets.OnlyController);
        evnt.Send();
    }

    public override void OnEvent(FlashEvent evnt)
    {
        GUI_Controller.Current.Flash();
    }

    private void UpdateDeathState()
    {
        if (entity.IsOwner)
        {
            if (state.IsDead)
                GameController.Current.state.AlivePlayers--;
            else
                GameController.Current.state.AlivePlayers++;
        }

        if (entity.HasControl)
            GUI_Controller.Current.Show(!state.IsDead);

        _playerMotor.OnDeath(state.IsDead);
        _playerRenderer.OnDeath(state.IsDead);
        _playerWeapons.OnDeath(state.IsDead);
    }

    public void RoundReset(Team winner)
    {
        if (entity.IsOwner)
        {
            if (GameController.Current.CurrentPhase != GamePhase.Starting)
            {
                if (state.IsDead == true)
                {
                    state.IsDead = false;
                    if (GameController.Current.CurrentPhase == GamePhase.WaitForPlayers)
                    {
                        state.Energy = 1;

                        state.LifePoints = _playerMotor.TotalLife;
                        state.SetTeleport(state.Transform);
                        PlayerToken token = (PlayerToken)entity.AttachToken;
                        transform.position = FindObjectOfType<PlayerSetupController>().GetSpawnPoint(token.team);
                    }
                }

                if (GameController.Current.CurrentPhase == GamePhase.StartRound)
                {
                    if (state.Energy < 4)
                        state.Energy += 1;

                    PlayerToken token = (PlayerToken)entity.AttachToken;

                    state.LifePoints = _playerMotor.TotalLife;
                    state.SetTeleport(state.Transform);
                    transform.position = FindObjectOfType<PlayerSetupController>().GetSpawnPoint(token.team);
                }
            }
            else
            {
                state.Energy = 0;
            }
        }
    }
}
