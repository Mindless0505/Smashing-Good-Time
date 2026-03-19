using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Button CreateLobby;
    [SerializeField] private Button JoinLobby;

    private void Awake()
    {
        //MainMenuButton.onClick.AddListener(() => {
        //    Loader.Load(Loader.Scene.SampleScene); 
        //});
        CreateLobby.onClick.AddListener(() => {
            GameLobby.Instance.CreateLobby("LobbyName", false);
        });
        JoinLobby.onClick.AddListener(() => {
            GameLobby.Instance.QuickJoin();
        });
    }
}
