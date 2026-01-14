using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LevelExit : NetworkBehaviour
{
    [Header("Configuração")]
    public string nomeDaProximaCena = "Level2";

    // Variável estática para contar (0, 1 ou 2)
    private static int jogadoresNaSaida = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer) jogadoresNaSaida = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            
            // Verifica se FUI EU que entrei na saída
            if (netObj != null && netObj.IsOwner)
            {
                // 1. Mostrar mensagem SÓ PARA MIM (Local)
                if (GameMessenger.Instance != null)
                {
                    // Determina qual a mensagem baseada em quem sou eu
                    string msg = IsHost ? 
                        "Estás na saída! Espera pelo Presente." : 
                        "Estás na saída! Espera pelo Passado.";

                    GameMessenger.Instance.MostrarMensagemLocal(msg);
                }

                // 2. Avisar o servidor que cheguei (para ele contar)
                RegistarEntradaServerRpc();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                // Se eu sair, limpo a minha mensagem local
                if (GameMessenger.Instance != null) GameMessenger.Instance.MostrarMensagemLocal(""); 
                
                RegistarSaidaServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RegistarEntradaServerRpc()
    {
        jogadoresNaSaida++;
        if (jogadoresNaSaida >= 2)
        {
            // Se já estão os dois, muda de nível para todos
            NetworkManager.Singleton.SceneManager.LoadScene(nomeDaProximaCena, LoadSceneMode.Single);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RegistarSaidaServerRpc()
    {
        jogadoresNaSaida--;
        if (jogadoresNaSaida < 0) jogadoresNaSaida = 0;
    }
}