using TMPro;
using UnityEngine;

public class InformationOnCamera : MonoBehaviour
{
    private TestNetcodeUI netcodeUI;    
    private TMP_Text lobbyCodeText;

    private void Awake()
    {
        // Find NetcodeUI in the scene
        netcodeUI = FindObjectOfType<TestNetcodeUI>();
        if (netcodeUI == null)
        {
            Debug.LogError("No TestNetcodeUI found in scene!");
        }

        // Find TMP_Text on this prefab or its children
        lobbyCodeText = GetComponentInChildren<TMP_Text>();
        if (lobbyCodeText == null)
        {
            Debug.LogError("No TMP_Text found in children!");
        }
    }

    void Update()
    {
        if (netcodeUI == null || lobbyCodeText == null) return;

        // Update text
        lobbyCodeText.text = "Lobby Code: " + netcodeUI.CurrentLobbyCode;

    
    }
}