using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ResetGame : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.BackQuote))
            RequestResetServerRpc();
    }

    [ServerRpc]
    void RequestResetServerRpc()
    {
        TriggerResetClientRpc();
    }

    [ClientRpc]
    void TriggerResetClientRpc()
    {
        if (GameResetter.Instance == null)
        {
            var obj = new GameObject("GameResetter");
            obj.AddComponent<GameResetter>();
        }
        GameResetter.Instance.TriggerReset(NetworkManager.Singleton.IsHost);
    }
}