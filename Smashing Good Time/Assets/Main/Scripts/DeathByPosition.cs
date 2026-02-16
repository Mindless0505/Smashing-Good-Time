using UnityEngine;

public class DeathByPosition : MonoBehaviour
{
    public Transform respawnPoint;
    public float minY = -10f;
    public float maxY = 100f;
    public float maxX = 100f;
    public float minX = -100f;
    public float maxZ = 100f;
    public float minZ = -100f;

    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.y >= maxY || pos.y <= minY || pos.x >= maxX || pos.x <= minX || pos.z >= maxZ || pos.z <= minZ)
        {
            Respawn();
        }
    }


    void Respawn()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // use velocity if needed
        }

        transform.position = respawnPoint.position;
    }
}
