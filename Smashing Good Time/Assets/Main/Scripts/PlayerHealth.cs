using Cinemachine.Utility;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Netcode;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{

    // NetworkVariables automatically sync to all clients
    public NetworkVariable<int> damPerc = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> mult = new NetworkVariable<int>(1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);


    // private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        // Tell the HUD a new player has joined
        // HUDManager.Instance.RegisterPlayer(this);
        StartCoroutine(RegisterWithHUD());
    }

    private IEnumerator RegisterWithHUD()
    {
        while (HUDManager.Instance == null)
            yield return null;

        HUDManager.Instance.RegisterPlayer(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gameManager = GameManager.Instance;
    }


    public int Hit(bool Hammer)
    {
        if (Hammer)
        {
            damPerc.Value += Random.Range(15,25);  
        }
        else
        {
            damPerc.Value += Random.Range(10,20);
        }
        

        int newMult = Mathf.RoundToInt(Mathf.Pow(damPerc.Value, .85f));
        mult.Value = newMult;
        // gameManager.OnPlayerHit(playerID,    mult);

        Debug.Log(damPerc.Value +"%" + " " + mult.Value);
        return newMult;
        
    }


  public void ResetHealth()
    {
        ResetHealthServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ResetHealthServerRpc()
    {
        damPerc.Value = 0;
        mult.Value = 1;
    }
}
