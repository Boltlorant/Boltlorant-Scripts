using UnityEngine;
using Bolt;

public class PlayerCallback : EntityBehaviour<IPlayerState>
{
    private PlayerMotor _playerMotor;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
    }

    public override void Attached()
    {
        state.AddCallback("LifePoints", UpdatePlayerLife);

        if (entity.IsOwner)
        {
            state.LifePoints = _playerMotor.TotalLife;
        }
    }

    public void UpdatePlayerLife()
    {
        if(entity.HasControl)
            GUI_Controller.Current.UpdateLife(state.LifePoints, _playerMotor.TotalLife);
    }
}
