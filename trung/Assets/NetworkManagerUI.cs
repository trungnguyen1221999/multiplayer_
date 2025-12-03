using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    void Awake()
    {
        serverBtn.onClick.AddListener(StartServer);
        hostBtn.onClick.AddListener(StartHost);
        clientBtn.onClick.AddListener(StartClient);
    }

    private void StartServer()
    {
        Debug.Log("Start Server clicked");
        // TODO: Call your NetworkManager's StartServer here
        NetworkManager.Singleton.StartServer();
    }

    private void StartHost()
    {
        Debug.Log("Start Host clicked");
        // TODO: Call your NetworkManager's StartHost here
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        Debug.Log("Start Client clicked");
        // TODO: Call your NetworkManager's StartClient here
        NetworkManager.Singleton.StartClient();
    }
}
