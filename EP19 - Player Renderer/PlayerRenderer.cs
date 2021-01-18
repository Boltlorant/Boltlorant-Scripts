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
            _meshRenderer.gameObject.SetActive(true);

            if (_playerMotor.IsEnemy)
            {
                _meshRenderer.material.color = _enemyColor;
            }
            else
            {
                //PlayerToken pt = (PlayerToken)entity.AttachToken;
                _meshRenderer.material.color = _allyColor;
            }
        }
    }
}
