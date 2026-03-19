using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;

public class NetcodeUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    private bool hasStarted = false;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(() =>
        {
            if (hasStarted) return;
            hasStarted = true;

            Debug.Log("Starting Host...");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        StartClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Client...");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
