using System.Collections;
using UnityEngine;
using Bolt;

public class WeaponDrop : EntityBehaviour<IPhysicState>
{
    private NetworkRigidbody _networkRigidbody = null;
    private bool _inited = false;

    private void Awake()
    {
        _networkRigidbody = GetComponent<NetworkRigidbody>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        _inited = true;
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            if (transform.rotation == Quaternion.identity)
            {
                _networkRigidbody.MoveVelocity = Random.onUnitSphere * 10f;
                transform.eulerAngles = Random.insideUnitSphere * 360f;
            }
            else
            {
                _networkRigidbody.MoveVelocity = transform.forward * 10f;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (entity.IsAttached)
            if (entity.IsOwner && (_inited || !collision.gameObject.GetComponent<PlayerMotor>()))
                _networkRigidbody.MoveVelocity *= 0.5f;
    }
}
