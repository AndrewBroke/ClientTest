using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Unity.Collections;

public class ClientManager : MonoBehaviour
{
    public InputField loginInput;
    public InputField passwordInput;
    public Button loginButton;
    public Text messageText;

    private void Start()
    {
        loginButton.onClick.AddListener(SendLoginData);
        NetworkManager.Singleton.OnServerStarted += RegisterMessageHandlers;
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"���������� � ������� ��� {clientId}");
        };
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
        Invoke("RegisterMessageHandlers", 2);
    }

    private void Awake()
    {
        // ���������� ������ �� �������
        //NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("AuthResponse", (senderClientId, reader) =>
        //{
        //    reader.ReadValueSafe(out bool isAuthorized);
        //    messageText.text = isAuthorized ? "���� ��������!" : "������: �������� ������";

        //    if (isAuthorized)
        //    {
        //        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        //    }
        //});
    }

    private void RegisterMessageHandlers()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            print("������� ������ �� �������� ���������");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("AuthResponse", (senderClientId, reader) =>
            {
                
                reader.ReadValueSafe(out bool isAuthorized);
                messageText.text = isAuthorized ? "���� ��������!" : "������: �������� ������";

                if (isAuthorized)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
                }
            });
        }
    }

    private void OnConnected(ulong clientId)
    {
        Debug.Log($"���������� � ������� ��� {clientId}");
    }

    public void ConnectToServer()
    {
        NetworkManager.Singleton.StartClient();
        messageText.text = "������������...";
    }

    private void SendLoginData()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            ConnectToServer();
            Invoke(nameof(SendLoginData), 1f); // ������� 1 ������� ����� ��������� ��������
            return;
        }

        string login = loginInput.text;
        string password = passwordInput.text;

        using FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp);
        byte[] loginBytes = Encoding.UTF8.GetBytes(login);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        writer.WriteValueSafe(loginBytes.Length);
        writer.WriteBytesSafe(loginBytes);
        writer.WriteValueSafe(passwordBytes.Length);
        writer.WriteBytesSafe(passwordBytes);

        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("AuthRequest", NetworkManager.ServerClientId, writer);

        messageText.text = "�������� ������...";
    }
}
