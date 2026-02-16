using UnityEngine;

public class DestructableWall : MonoBehaviour
{
    [Header("Impact Settings")]
    // set the required impact strength and health threshold for the wall to be destroyed
    public float requiredImpact = 5f;
    public float requiredHealth = 50f;
    [SerializeField] private Transform PrefabWall;
    
    public bool isDestroyed = false;

    void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return; // prevent multiple destructions
        Debug.Log("Collision detected with " + collision.gameObject.name);
        // calculate the impact strength based on the relative velocity of the collision
        float impactStrength = collision.relativeVelocity.magnitude;
        requiredHealth -= impactStrength;
        Debug.Log("Impact strength: " + impactStrength);
        // check if the impact strength meets the required threshold or if the health is depleted
        if (impactStrength >= requiredImpact || requiredHealth <= 0)
        {
            isDestroyed = true;
            SwitchObject();
        }
    }

    void SwitchObject()
    {
        // Instantiate the new wall prefab at the same position and rotation as the original wall
        Instantiate(PrefabWall, transform.position, transform.rotation);
        Destroy(gameObject);
        Debug.Log("Wall destroyed");
    }
    
}
