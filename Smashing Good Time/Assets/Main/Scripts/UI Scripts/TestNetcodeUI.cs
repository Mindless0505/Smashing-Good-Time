using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private TMP_InputField LobbyCodeInput;

    private bool hasStarted = false;
    private Lobby currentLobby;

    private async void Awake()
    {
        // Init Unity Services
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        StartHostButton.onClick.AddListener(async () =>
        {
            if (hasStarted) return;
            hasStarted = true;

            await StartHostWithLobby();
            Hide();
        });

        StartClientButton.onClick.AddListener(async () =>
        {
            await StartClientWithLobby();
            Hide();
        });
    }

    private async System.Threading.Tasks.Task StartHostWithLobby()
    {
        Debug.Log("Starting Host with Relay + Lobby...");

        // 1. Create Relay
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // 2. Configure transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            null, // host = null
            true
        );

        // 3. Create Lobby
        var options = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {
                    "relayCode",
                    new DataObject(DataObject.VisibilityOptions.Member, relayCode)
                }
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync("My Lobby", 4, options);

        Debug.Log("Lobby Code: " + currentLobby.LobbyCode);

        // 4. Start host
        NetworkManager.Singleton.StartHost();

        // 5. Start heartbeat
        StartCoroutine(HeartbeatLobby(currentLobby.Id));
    }

    private async System.Threading.Tasks.Task StartClientWithLobby()
    {
        Debug.Log("Starting Client...");

        string lobbyCode = LobbyCodeInput.text.Trim();

        // 1. Join Lobby
        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

        // 2. Get relay code
        string relayCode = lobby.Data["relayCode"].Value;

        // 3. Join Relay
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

        // 4. Configure transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData, 
            true
        );
        // 5. Start client
        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator HeartbeatLobby(string lobbyId)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return new WaitForSeconds(15);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}