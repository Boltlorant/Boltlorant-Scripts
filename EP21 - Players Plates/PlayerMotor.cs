using UnityEngine;
using Bolt;

public class PlayerMotor : EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private Camera _cam = null;
    private NetworkRigidbody _networkRigidbody = null;

    private float _speed = 7f;
    private float _speedBase = 7f;

    private Vector3 _lastServerPos = Vector3.zero;
    private bool _firstState = true;

    private bool _jumpPressed = false;
    private float _jumpForce = 9f;

    private bool _isGrounded = false;
    private float _maxAngle = 45f;

    [SerializeField]
    private int _totalLife = 250;

    SphereCollider _headCollider;
    CapsuleCollider _capsuleCollider;

    [SerializeField]
    private Ability _skill = null;
    [SerializeField]
    private Ability _grenade = null;

    private bool _isEnemy = true;

    public int TotalLife { get => _totalLife; }
    public float Speed { get => _speed; set => _speed = value; }
    public float SpeedBase { get => _speedBase; }
    public bool IsEnemy { get => _isEnemy; }

    private void Awake()
    {
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        _headCollider = GetComponent<SphereCollider>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void Init(bool isMine)
    {
        if (isMine)
        {
            tag = "LocalPlayer";
            GUI_Controller.Current.UpdateLife(_totalLife, _totalLife);
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject go in players)
            {
                go.GetComponent<PlayerMotor>().TeamCheck();
                go.GetComponent<PlayerRenderer>().Init();
            }
        }

        TeamCheck();
    }

    public void TeamCheck()
    {
        GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
        Team t = Team.AT;
        PlayerToken pt = (PlayerToken)entity.AttachToken;

        if (localPlayer)
        {
            PlayerToken lpt = (PlayerToken)localPlayer.GetComponent<PlayerMotor>().entity.AttachToken;
            t = lpt.team;
        }

        if (pt.team == t)
            _isEnemy = false;
        else
            _isEnemy = true;
    }

    public State ExecuteCommand(bool forward, bool backward, bool left, bool right, bool jump, float yaw, float pitch , bool ability1, bool ability2)
    {
        if (!state.IsDead)
        {
            Vector3 movingDir = Vector3.zero;
            if (forward ^ backward)
            {
                movingDir += forward ? transform.forward : -transform.forward;
            }
            if (left ^ right)
            {
                movingDir += right ? transform.right : -transform.right;
            }

            if (jump)
            {
                if (_jumpPressed == false && _isGrounded)
                {
                    _isGrounded = false;
                    _jumpPressed = true;
                    _networkRigidbody.MoveVelocity += Vector3.up * _jumpForce;
                }
            }
            else
            {
                if (_jumpPressed)
                    _jumpPressed = false;
            }

            movingDir.Normalize();
            movingDir *= _speed;
            _networkRigidbody.MoveVelocity = new Vector3(movingDir.x, _networkRigidbody.MoveVelocity.y, movingDir.z);

            _cam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            transform.rotation = Quaternion.Euler(0, yaw, 0);

            if (entity.IsOwner)
                state.Pitch = (int)pitch;

            if (_skill)
                _skill.UpdateAbility(ability1);
            if (_grenade)
                _grenade.UpdateAbility(ability2);
        }

        State stateMotor = new State();
        stateMotor.position = transform.position;
        stateMotor.rotation = yaw;

        return stateMotor;
    }

    public void SetPitch()
    {
        if (!entity.IsControllerOrOwner)
            _cam.transform.localEulerAngles = new Vector3(state.Pitch, 0f, 0f);
    }


    private void FixedUpdate()
    {
        if (entity.IsAttached)
        {
            if (entity.IsControllerOrOwner)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.3f))
                {
                    float slopeNormal = Mathf.Abs(Vector3.Angle(hit.normal, new Vector3(hit.normal.x, 0, hit.normal.z)) - 90) % 90;

                    if (_networkRigidbody.MoveVelocity.y < 0)
                        _networkRigidbody.MoveVelocity = Vector3.Scale(_networkRigidbody.MoveVelocity, new Vector3(1, 0, 1));

                    if (!_isGrounded && slopeNormal <= _maxAngle)
                    {
                        _isGrounded = true;
                    }
                }
                else
                {
                    if (_isGrounded)
                    {
                        _isGrounded = false;
                    }
                }
            }
        }
    }

    public void SetState(Vector3 position, float rotation)
    {
        if (Mathf.Abs(rotation - transform.rotation.y) > 5f)
            transform.rotation = Quaternion.Euler(0, rotation, 0);

        if (_firstState)
        {
            if (position != Vector3.zero)
            {
                transform.position = position;
                _firstState = false;
                _lastServerPos = Vector3.zero;
            }
        }
        else
        {
            if (position != Vector3.zero)
            {
                _lastServerPos = position;
            }

            transform.position += (_lastServerPos - transform.position) * 0.5f;
        }
    }

    public bool IsHeadshot(Collider c)
    {
        return c == _headCollider;
    }

    public void Life(PlayerMotor killer, int life)
    {
        if (entity.IsOwner)
        {
            int value = state.LifePoints + life;

            if (value < 0)
            {
                state.LifePoints = 0;
                state.IsDead = true;
            }
            else if (value > _totalLife)
            {
                state.LifePoints = _totalLife;
            }
            else
            {
                state.LifePoints = value;
            }
        }
    }

    public void OnDeath(bool b)
    {
        _networkRigidbody.enabled = !b;
        _headCollider.enabled = !b;
        _capsuleCollider.enabled = !b;
    }

    public struct State
    {
        public Vector3 position;
        public float rotation;
    }
}
