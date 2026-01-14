using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MenuPrincipal : MonoBehaviour
{
    public string nomeDaCenaDoJogo = "level1";
    public TMP_InputField ipInput;
    public TMP_Text statusText; // Adicione um TextMeshPro para feedback

    void Start()
    {
        // Configura callbacks para feedback
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        if (statusText != null)
            statusText.text = "Pronto para conectar";
    }

    public void IniciarHost()
    {
        if (statusText != null)
            statusText.text = "Iniciando servidor...";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        // Configura o host para aceitar conexões externas
        transport.SetConnectionData("0.0.0.0", 7777);
        
        // Habilita debug para ver logs de rede
        NetworkManager.Singleton.NetworkConfig.EnableNetworkLogs = true;

        if (NetworkManager.Singleton.StartHost())
        {
            if (statusText != null)
                statusText.text = "Host iniciado! Carregando nível...";
            
            NetworkManager.Singleton.SceneManager.LoadScene(nomeDaCenaDoJogo, LoadSceneMode.Single);
        }
        else
        {
            if (statusText != null)
                statusText.text = "Falha ao iniciar host!";
        }
    }

    public void IniciarClient()
    {
        string ip = ipInput.text;

        if (string.IsNullOrEmpty(ip))
        {
            ip = "127.0.0.1"; // Localhost para testes
            if (statusText != null)
                statusText.text = "Usando localhost (teste)";
        }

        if (statusText != null)
            statusText.text = $"Conectando a {ip}...";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransport não encontrado!");
            if (statusText != null)
                statusText.text = "Erro: UnityTransport não encontrado!";
            return;
        }

        // Configura o cliente
        transport.SetConnectionData(ip, 7777);
        
        // Configura timeout menor para testes
        transport.MaxConnectAttempts = 10;
        transport.ConnectTimeoutMS = 5000;
        
        NetworkManager.Singleton.NetworkConfig.EnableNetworkLogs = true;

        if (!NetworkManager.Singleton.StartClient())
        {
            if (statusText != null)
                statusText.text = "Falha ao iniciar cliente!";
        }
    }

    // Callbacks para feedback
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Cliente conectado: {clientId}");
        if (statusText != null)
            statusText.text = $"Conectado! ID: {clientId}";
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Cliente desconectado: {clientId}");
        if (statusText != null)
            statusText.text = "Desconectado";
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}