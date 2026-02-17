using System.Collections;
using UnityEngine;

public class DeathByPosition : MonoBehaviour
{
    [Header("Wall Limits")]
    public float minY = -10f;
    public float maxY = 100f;
    public float maxX = 100f;
    public float minX = -100f;
    public float maxZ = 100f;
    public float minZ = -100f;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Lives")]
    public int lives = 3;
    private int currentLives;

    private bool isRespawning = false;
    private Rigidbody rb;

    void Start()
    {
        currentLives = lives;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isRespawning) return;

        Vector3 pos = transform.position;

        if (pos.y >= maxY || pos.y <= minY || pos.x >= maxX || pos.x <= minX || pos.z >= maxZ || pos.z <= minZ)
        {
            Die();
        }
    }

    void Die()
    {
        currentLives--;
        Debug.Log(gameObject.name + " lost a life | Lives Remaining: " + currentLives);

        if (currentLives <= 0)
        {
            // Handle game over logic here (e.g., show game over screen, reset level, etc.)
            GameOver();
            return;
        }
        Respawn();
    }
    void Respawn()
    {
        isRespawning = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
        }

        transform.position = respawnPoint.position;


        isRespawning = false;
    }

    void GameOver()
    {
        Debug.Log("Game over");
        gameObject.SetActive(false);

    }
}
