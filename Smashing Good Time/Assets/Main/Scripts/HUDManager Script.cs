using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class HUDManager : NetworkBehaviour
{
    public static HUDManager Instance;

    public GameObject playerCardPrefab;
    public Transform hudContainer;

    private Dictionary<ulong, TMP_Text> percTexts = new Dictionary<ulong, TMP_Text>();
    private List<PlayerHealth> trackedPlayers = new List<PlayerHealth>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Instance = this;

            // Catch any players that already spawned
            PlayerHealth[] existing = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (PlayerHealth health in existing)
                RegisterPlayer(health);
        }
    }

    void Update()
    {
        foreach (PlayerHealth health in trackedPlayers)
        {
            if (percTexts.TryGetValue(health.OwnerClientId, out TMP_Text text))
                text.text = health.damPerc.Value + "%";
        }
    }

    public void RegisterPlayer(PlayerHealth health)
    {

        if (percTexts.ContainsKey(health.OwnerClientId)) return;

        GameObject card = Instantiate(playerCardPrefab, hudContainer);

        int colorIndex = (int)health.OwnerClientId % ColorReference.PlayerColors.Length;

        TMP_Text percText = card.transform.Find("PercText").GetComponent<TMP_Text>();
        percText.text = "0%";
        percTexts[health.OwnerClientId] = percText;
        // percText.color = PlayerHealth.PlayerColors[colorIndex];

        TMP_Text idText = card.transform.Find("IDText").GetComponent<TMP_Text>();
        idText.text = "P" + (health.OwnerClientId + 1);
        idText.color = ColorReference.PlayerColors[colorIndex];

        trackedPlayers.Add(health);

        trackedPlayers.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));

        // Reorder the actual UI cards to match
        foreach (PlayerHealth p in trackedPlayers)
        {
            if (percTexts.TryGetValue(p.OwnerClientId, out TMP_Text text))
                text.transform.parent.SetSiblingIndex(trackedPlayers.IndexOf(p));
        }
    }
}