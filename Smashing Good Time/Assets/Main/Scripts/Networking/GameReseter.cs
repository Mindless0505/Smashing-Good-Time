using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameResetter : MonoBehaviour
{
    public static GameResetter Instance;
    public bool wasHost;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerReset(bool isHost)
    {
        wasHost = isHost;
        NetworkManager.Singleton.Shutdown();
        StartCoroutine(WaitAndReload());
    }

    private IEnumerator WaitAndReload()
    {
        while (NetworkManager.Singleton.ShutdownInProgress)
            yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}