using UnityEngine;
using Bolt;

public class PlayerRenderer : EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private MeshRenderer _meshRenderer;
    private PlayerMotor _playerMotor;
    [SerializeField]
    private Color _enemyColor;
    [SerializeField]
    private Color _allyColor;

    [SerializeField]
    private Transform _camera;
    private Transform _sceneCamera;
    [SerializeField]
    private Transform _weaponView;

    [SerializeField]
    private TextMesh _textMesh;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
    }

    public void Init()
    {
        if (entity.IsControllerOrOwner)
            _camera.gameObject.SetActive(true);

        if (entity.HasControl)
        {
            _sceneCamera = GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerSetupController>().SceneCamera.transform;
            _sceneCamera.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            PlayerToken pt = (PlayerToken)entity.AttachToken;
            _textMesh.text = pt.name;
            _meshRenderer.gameObject.SetActive(true);

            if (_playerMotor.IsEnemy)
            {
                _meshRenderer.material.color = _enemyColor;
                _textMesh.gameObject.SetActive(false);
            }
            else
            {
                _textMesh.gameObject.SetActive(true);
                _meshRenderer.material.color = _allyColor;
            }
        }
    }

    public void OnDeath(bool b)
    {
        if (b)
        {
            if (entity.HasControl)
                _sceneCamera.gameObject.SetActive(true);

            _camera.gameObject.SetActive(false);
            _meshRenderer.gameObject.SetActive(false);
            _textMesh.gameObject.SetActive(false);
            _weaponView.gameObject.SetActive(false);
        }
        else
        {
            _weaponView.gameObject.SetActive(true);

            if (entity.IsControllerOrOwner)
                _camera.gameObject.SetActive(true);

            if (entity.HasControl)
            {
                _sceneCamera.gameObject.SetActive(false);
            }
            else
            {
                _meshRenderer.gameObject.SetActive(true);

                if (!_playerMotor.IsEnemy)
                {
                    _textMesh.gameObject.SetActive(true);
                    _meshRenderer.material.color = _allyColor;
                }
            }
        }
    }
}
