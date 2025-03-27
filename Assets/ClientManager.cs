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

        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;

        ConnectToServer();
        RegisterMessageHandler();
    }


    private void RegisterMessageHandler()
    {
        print(NetworkManager.Singleton.IsClient);
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("✅ Клиент подписывается на AuthResponse...");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("AuthResponse", (senderClientId, reader) =>
            {
                Debug.Log("📩 Клиент получил AuthResponse!");

                reader.ReadValueSafe(out bool isAuthorized);
                Debug.Log($"📌 Декодированный ответ: {isAuthorized}");

                messageText.text = isAuthorized ? "Вход выполнен!" : "Ошибка: неверные данные";

                if (isAuthorized)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
                }
            });
        }
    }

    private void OnConnected(ulong clientId)
    {
        Debug.Log($"Подключено к серверу как {clientId}");
    }

    public void ConnectToServer()
    {
        NetworkManager.Singleton.StartClient();
        messageText.text = "Подключаемся...";
    }

    private void SendLoginData()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            ConnectToServer();
            Invoke(nameof(SendLoginData), 1f); // Подождём 1 секунду перед повторной попыткой
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

        messageText.text = "Отправка данных...";
    }
}
