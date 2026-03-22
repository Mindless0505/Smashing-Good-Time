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

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Log("Signed in! Player ID: " + AuthenticationService.Instance.PlayerId);
            }
            catch (System.Exception)
            {
                // Wait for existing session to fully load
                await Task.Delay(500);
                Log("Using existing session. Player ID: " + AuthenticationService.Instance.PlayerId);
            }
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
            Log("Host Step 1: Checking NetworkManager...");
            if (NetworkManager.Singleton == null) { LogError("NetworkManager is null!"); return; }

            Log("Host Step 2: Checking UnityTransport...");
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null) { LogError("UnityTransport is null!"); return; }

            Log("Host Step 3: Checking RelayService...");
            if (RelayService.Instance == null) { LogError("RelayService is null!"); return; }

            Log("Host Step 4: Creating Relay allocation...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4, "us-central1");
            Log("Allocation region: " + allocation.Region);
            if (allocation == null) { LogError("Allocation is null!"); return; }

            Log("Host Step 5: Getting join code...");
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            if (string.IsNullOrEmpty(relayCode)) { LogError("Relay code is null or empty!"); return; }
            Log("Relay code: " + relayCode);

            Log("Host Step 6: Building RelayServerData...");
            if (allocation.RelayServer == null) { LogError("RelayServer is null!"); return; }
            var relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData,  // host uses ConnectionData for both
                allocation.Key,
                true,   // isSecure
                true    // isWebSocket
            );

            Log("Host Step 7: Setting transport data...");
            transport.SetRelayServerData(relayServerData);

            Log("Host Step 8: Creating lobby...");
            if (LobbyService.Instance == null) { LogError("LobbyService is null!"); return; }
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "relayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
            }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("My Lobby", 4, options);
            if (currentLobby == null) { LogError("Lobby creation returned null!"); return; }
            Log("Lobby created! Code: " + currentLobby.LobbyCode);

            Log("Host Step 9: Starting NetworkManager host...");
            NetworkManager.Singleton.StartHost();
            Log("Host started successfully!");

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

            // --- FIX 1: Poll until relay code is available ---
            string relayCode = null;
            int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

                if (lobby.Data != null &&
                    lobby.Data.ContainsKey("relayCode") &&
                    !string.IsNullOrEmpty(lobby.Data["relayCode"].Value))
                {
                    relayCode = lobby.Data["relayCode"].Value;
                    Log($"Relay code retrieved on attempt {i + 1}: {relayCode}");
                    break;
                }

                Log($"Relay code not ready yet, attempt {i + 1}/{maxAttempts}. Retrying...");
                await Task.Delay(1000);
            }

            if (string.IsNullOrEmpty(relayCode))
            {
                LogError("Relay code never became available after polling.");
                hasClientStarted = false;
                return;
            }

            // Join Relay
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            // Configure transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                joinAllocation.Key,
                true,   // isSecure
                true    // isWebSocket
            );
            transport.SetRelayServerData(relayServerData);

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
    public string RelayCode
    {
        get
        {
            // Fix: Retrieve relayCode from currentLobby.Data dictionary
            if (currentLobby != null &&
                currentLobby.Data != null &&
                currentLobby.Data.ContainsKey("relayCode") &&
                !string.IsNullOrEmpty(currentLobby.Data["relayCode"].Value))
            {
                return currentLobby.Data["relayCode"].Value;
            }
            return string.Empty;
        }
    }
}