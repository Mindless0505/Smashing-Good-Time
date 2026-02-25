using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;

public class NetcodeUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(() =>
        {
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
