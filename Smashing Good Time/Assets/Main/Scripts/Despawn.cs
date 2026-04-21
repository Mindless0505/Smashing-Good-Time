using UnityEngine;
using System.Collections;

public class Despawn : MonoBehaviour
{
    public float despawnDelay = 3.0f;

    void OnEnable()
    {
        Destroy(gameObject, despawnDelay);
    }
}
