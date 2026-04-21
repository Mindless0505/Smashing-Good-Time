using UnityEngine;

public class FallenObject : MonoBehaviour
{

    [Header("Wall Limits")]
    public float minY = -10f;
    public float maxY = 100f;
    public float maxX = 100f;
    public float minX = -100f;
    public float maxZ = 100f;
    public float minZ = -100f;

    void Update()
    {
        DestroyObject();
    }

    private void DestroyObject()
    {
        Vector3 pos = transform.position;
        if (pos.y < minY || pos.y > maxY || pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
        {
            
            Destroy(gameObject);
        }
    }
}

