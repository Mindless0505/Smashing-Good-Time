using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ResetGame : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            RequestResetServerRpc();
        }
    }

    [ServerRpc]
    void RequestResetServerRpc()
    {
        // Server triggers reset for everyone
        NetworkManager.SceneManager.LoadScene(
            SceneManager.GetActiveScene().name,
            LoadSceneMode.Single
        );
    }
}