using UnityEngine;

public class SpectatorController : MonoBehaviour
{
    private bool _forward = false;
    private bool _backward = false;
    private bool _left = false;
    private bool _right = false;
    private bool _jump = false;
    private bool _crouch = false;
    private bool _sprint = false;

    private float _yaw = 0;
    private float _pitch = 0;
    private float _wheel = 0;

    private Transform _target;
    private float _speedLerp = 0.1f;
    private float _speed = 2.5f;
    private float _sprintSpeed = 5f;
    private GameObject[] _players;
    private int _currentTarget = -1;

    private void Awake()
    {
        _target = new GameObject("CameraTarget").transform;
    }

    private void PollKeys()
    {
        _forward = Input.GetKey(KeyCode.W);
        _backward = Input.GetKey(KeyCode.S);
        _left = Input.GetKey(KeyCode.A);
        _right = Input.GetKey(KeyCode.D);
        _jump = Input.GetKey(KeyCode.Space);
        _crouch = Input.GetKey(KeyCode.LeftControl);
        _sprint = Input.GetKey(KeyCode.LeftControl);
        _wheel = Input.GetAxis("Mouse ScrollWheel");
        _yaw += Input.GetAxisRaw("Mouse X") * 2f;
        _yaw %= 360f;
        _pitch -= Input.GetAxisRaw("Mouse Y") * 2f;
    }

    private void NextTarget()
    {
        _players = GameObject.FindGameObjectsWithTag("Player");
        _currentTarget++;

        bool check = false;

        while (_currentTarget != -1 && !check)
        {
            if (_currentTarget == _players.Length)
                _currentTarget = -1;

            if (_currentTarget != -1)
            {
                check = !_players[_currentTarget].GetComponent<PlayerMotor>().state.IsDead;

                if (!check)
                    _currentTarget++;

            }
        }
    }

    private void PreviousTarget()
    {
        _players = GameObject.FindGameObjectsWithTag("Player");
        _currentTarget--;

        bool check = false;

        while (_currentTarget != -1 && !check)
        {
            if (_currentTarget == -2)
                _currentTarget = _players.Length - 1;

            check = !_players[_currentTarget].GetComponent<PlayerMotor>().state.IsDead;

            if (!check)
                _currentTarget++;
        }
    }

    private void Update()
    {
        PollKeys();

        if (_wheel > 0)
            NextTarget();
        else if (_wheel < 0)
            PreviousTarget();

        if (_currentTarget == -1)
        {
            Vector3 movingDir = Vector3.zero;

            if (_forward ^ _backward)
            {
                movingDir += _forward ? _target.forward : -_target.forward;
            }

            if (_left ^ _right)
            {
                movingDir += _right ? _target.right : -_target.right;
            }

            if (_jump ^ _crouch)
            {
                movingDir += _crouch ? -_target.up : _target.up;
            }

            movingDir = Vector3.Normalize(movingDir);
            _target.position += ((_sprint) ? movingDir * _sprintSpeed : movingDir * _speed) * BoltNetwork.FrameDeltaTime;
            _target.rotation = Quaternion.Euler(_target.rotation.x + _pitch, _target.rotation.y + _yaw, 0f);

            transform.position = Vector3.Lerp(transform.position, _target.position, _speedLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, _speedLerp);
        }
        else
        {
            GameObject player = _players[_currentTarget];

            _target.position = player.transform.position + (-player.transform.forward * 3f) + (player.transform.up * 1f);
            _target.LookAt(player.transform);

            transform.position = Vector3.Lerp(transform.position, _target.position, _speedLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, _speedLerp);
        }

    }
}
