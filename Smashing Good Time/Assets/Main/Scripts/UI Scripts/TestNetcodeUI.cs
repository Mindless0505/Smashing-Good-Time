using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TestNetcodeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private TMP_InputField LobbyCodeInput;
    [SerializeField] private TMP_Text DebugText; // optional on-screen debug

    private bool hasHostStarted = false;
    private bool hasClientStarted = false;
    public Lobby currentLobby;

    #region On-Screen Debug
    private string debugLog = "";
    private void Log(string msg)
    {
        Debug.Log(msg);
        debugLog += msg + "\n";
        if (DebugText != null) DebugText.text = debugLog;
    }
    private void LogError(string msg)
    {
        Debug.LogError(msg);
        debugLog += "[ERROR] " + msg + "\n";
        if (DebugText != null) DebugText.text = debugLog;
    }
    #endregion

    private async void Awake()
    {
        try
        {
            Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Log("Signed in! Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        {
            LogError("Unity Services initialization failed: " + e);
        }

        StartHostButton.onClick.AddListener(async () =>
        {
            if (hasHostStarted) return;
            hasHostStarted = true;
            await StartHostAsync();
            HideUI();
        });

        StartClientButton.onClick.AddListener(async () =>
        {
            if (hasClientStarted) return;
            hasClientStarted = true;
            await StartClientAsync();
            HideUI();
        });
    }

    #region Host
    private async Task StartHostAsync()
    {
        try
        {
            Log("Creating Relay allocation...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Configure transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                null, // host
                true
            );

            // Create Lobby
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("My Lobby", 4, options);

            Log("Lobby created! Code: " + currentLobby.LobbyCode + " | Relay code: " + relayCode);

            // Start host
            NetworkManager.Singleton.StartHost();
            Log("Host started!");

            // Start heartbeat
            StartCoroutine(Heartbeat(currentLobby.Id));
        }
        catch (System.Exception e)
        {
            LogError("Failed to start host: " + e);
            hasHostStarted = false;
        }
    }
    #endregion

    #region Client
    private async Task StartClientAsync()
    {
        try
        {
            string code = LobbyCodeInput.text.Trim();
            Log("Joining lobby: " + code);

            // Join Lobby
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            Log("Lobby joined! Players: " + lobby.Players.Count);

            // Wait a short delay to ensure lobby data propagates
            await Task.Delay(500);

            // Check relay code exists
            if (!lobby.Data.ContainsKey("relayCode"))
            {
                LogError("Relay code not found in lobby data");
                hasClientStarted = false;
                return;
            }

            string relayCode = lobby.Data["relayCode"].Value;
            Log("Relay code retrieved: " + relayCode);

            // Join Relay
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            // Configure transport
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

            // Start client
            NetworkManager.Singleton.StartClient();
            Log("Client started!");
        }
        catch (System.Exception e)
        {
            LogError("Failed to start client: " + e);
            hasClientStarted = false;
        }
    }
    #endregion

    #region Heartbeat
    private IEnumerator Heartbeat(string lobbyId)
    {
        while (true)
        {
            try
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            }
            catch (System.Exception e)
            {
                LogError("Heartbeat failed: " + e);
            }
            yield return new WaitForSeconds(15f);
        }
    }
    #endregion

    private void HideUI()
    {
        gameObject.SetActive(false);
    }

    public string CurrentLobbyCode
    {
        get
        {
            return currentLobby != null ? currentLobby.LobbyCode : string.Empty;
        }
    }
}