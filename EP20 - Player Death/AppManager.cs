using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    [SerializeField]
    private HeadlessServerManager _headlessServerManager = null;

    private static AppManager _current;

    public static AppManager Current
    {
        get
        {
            if (_current == null)
                _current = FindObjectOfType<AppManager>();
            return _current;
        }
    }
    public string Username
    {
        get
        {
            return PlayerPrefs.GetString("Username", "None");
        }

        set
        {
            PlayerPrefs.SetString("Username", value);
        }
    }

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
