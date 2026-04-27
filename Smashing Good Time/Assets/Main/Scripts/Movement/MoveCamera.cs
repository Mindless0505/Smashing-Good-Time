using UnityEngine;
using Unity.Netcode;

public class MoveCamera : NetworkBehaviour
{
    public Transform cameraPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!IsOwner)
        {
            return;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        transform.position = cameraPosition.position;
    }
}
