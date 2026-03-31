using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject messagePrefab; // just a TextMeshPro text object

    public bool chatOpen = false;

    private void Awake()
    {
        Instance = this;
        chatPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T) && !chatOpen)
        {
            OpenChat();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && chatOpen)
        {
            CloseChat();
        }

        if (Input.GetKeyDown(KeyCode.Return) && chatOpen)
        {
            SendMessage();
        }
    }

    private void OpenChat()
    {
        chatOpen = true;
        chatPanel.SetActive(true);
        inputField.ActivateInputField();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseChat()
    {
        chatOpen = false;
        chatPanel.SetActive(false);
        inputField.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SendMessage()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        // Send to server which broadcasts to everyone
        SendMessageServerRpc($"Player {OwnerClientId}: {text}");
        CloseChat();
    }

    [Rpc(SendTo.Server)]
    private void SendMessageServerRpc(string message)
    {
        ReceiveMessageClientRpc(message);
    }

    [Rpc(SendTo.Everyone)]
    private void ReceiveMessageClientRpc(string message)
    {
        GameObject msg = Instantiate(messagePrefab, messageContainer);
        msg.GetComponent<TMP_Text>().text = message;

        // Auto delete old messages if too many
        if (messageContainer.childCount > 20)
            Destroy(messageContainer.GetChild(0).gameObject);

        // Auto hide message after a few seconds
        StartCoroutine(FadeMessage(msg, 5f));
    }

    private System.Collections.IEnumerator FadeMessage(GameObject msg, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (msg != null) Destroy(msg);
    }
}