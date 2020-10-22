using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    [SerializeField]
    private HeadlessServerManager _headlessServerManager = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!_headlessServerManager.IsServer)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
