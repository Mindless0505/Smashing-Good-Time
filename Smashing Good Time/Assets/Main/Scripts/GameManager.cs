using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerHit(int playerID, float knockbackMult)
    {
        // Next: find that player's ragdoll and apply the force
        Debug.Log($"Player {playerID} hit with multiplier {knockbackMult}");
    }

}


public enum GameState
{
    
}