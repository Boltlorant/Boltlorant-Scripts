using System.Collections;
using UnityEngine;
using Bolt;

public class Rocket : EntityBehaviour<IPhysicState>
{
    private static float _FORCE = 30f;
    private static float _LIFETIME = 10f;
    private static float _RANGE = 5f;
    private static int _DAMAGE = 60;

    private Coroutine _life;
    private bool _inited = false;

    [SerializeField]
    private GameObject _explosion = null;
    private NetworkRigidbody _networkRigidbody = null;
    private PlayerMotor _playerMotor = null;

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
            GetComponent<NetworkRigidbody>().MoveVelocity = transform.forward * _FORCE;
            _life = StartCoroutine(_WaitForExplosion());
        }
    }

    public void Init(PlayerMotor pm)
    {
        _playerMotor = pm;
    }

    private IEnumerator _WaitForExplosion()
    {
        yield return new WaitForSeconds(_LIFETIME);
        _Explode();
    }
    public override void SimulateOwner()
    {
        transform.rotation = Quaternion.LookRotation(_networkRigidbody.MoveVelocity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (entity.IsOwner && (_inited || !collision.gameObject.GetComponent<PlayerMotor>()))
            _Explode();
    }

    private void _Explode()
    {
        if (_life != null)
            StopCoroutine(_life);

        Collider[] colliders = Physics.OverlapSphere(transform.position, _RANGE);
        foreach (Collider col in colliders)
        {
            if (col.GetComponent<PlayerMotor>())
                col.GetComponent<PlayerMotor>().Life(_playerMotor, -_DAMAGE);
        }

        BoltNetwork.Destroy(gameObject);
    }

    public override void Detached()
    {
        GameObject.Instantiate(_explosion, transform.position, transform.rotation);
    }
}
