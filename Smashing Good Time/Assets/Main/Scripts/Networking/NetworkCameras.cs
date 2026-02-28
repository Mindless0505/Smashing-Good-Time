using UnityEngine;
using Unity.Netcode;

public class NetworkCameras : NetworkBehaviour
{

    
    public Camera myCam;
    public AudioListener myAudioListener;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // I just put this here for reference to copy and paste. it means nothing yet

        // I need to make something like this
// using Unity.Netcode;
// using UnityEngine;

// public class PlayerSetup : NetworkBehaviour
// {
//     [SerializeField] private Camera playerCamera;
//     [SerializeField] private AudioListener audioListener;
//     [SerializeField] private GameObject viewModel;

//     public override void OnNetworkSpawn()
//     {
//         if (!IsOwner)
//         {
//             playerCamera.enabled = false;
//             audioListener.enabled = false;
//             viewModel.SetActive(false);
//         }
//         else
//         {
//             playerCamera.enabled = true;
//             audioListener.enabled = true;
//             viewModel.SetActive(true);
//         }
//     }
// }

        // if(!IsOwner)
        // {
        //         if(myCam.enabled == false)
        //     {
        //         myCam.enabled = true;
        //     }

        //     if(myAudioListener.enabled == false)
        //     {
        //         myAudioListener.enabled = true;
        //     }

        // }
        
    }

}
