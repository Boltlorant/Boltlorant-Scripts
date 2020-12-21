using System.Collections;
using UnityEngine;
using Bolt;

public class Grenade : EntityBehaviour<IPhysicState>
{
    private NetworkRigidbody _networkRigidbody = null;
    [SerializeField]
    private float _timer = 5f;
    private float _bouceThreshold = 0.2f;
    private float _repulsionForce = 0.5f;

    [SerializeField]
    private GameObject _explosion;

    protected PlayerMotor _launcher = null;
    public PlayerMotor laucher { set => _launcher = value; }

    private IEnumerator Start()
    {
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        yield return new WaitForSeconds(_timer);
        if (entity.IsOwner)
            BoltNetwork.Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 velocity = _networkRigidbody.MoveVelocity;

        float x = collision.contacts[0].normal.x;
        float y = collision.contacts[0].normal.y;
        float z = collision.contacts[0].normal.z;
        x = (Mathf.Abs(x) < _bouceThreshold) ? 1f : -Mathf.Abs(x);
        y = (Mathf.Abs(y) < _bouceThreshold) ? 1f : -Mathf.Abs(y);
        z = (Mathf.Abs(z) < _bouceThreshold) ? 1f : -Mathf.Abs(z);
        Vector3 repulsionVector = new Vector3(x, y, z);
        _networkRigidbody.MoveVelocity = Vector3.Scale(velocity, repulsionVector * _repulsionForce);
    }

    public override void Detached()
    {
        if (!entity.IsOwner)
            GameObject.Instantiate(_explosion, transform.position, Quaternion.identity);
    }
}


