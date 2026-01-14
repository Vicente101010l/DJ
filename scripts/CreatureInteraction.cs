using UnityEngine;
using Unity.Netcode;

public class CreatureInteraction : NetworkBehaviour
{
    [Header("LIGAÇÃO")]
    public PuzzleGate portaoParaAbrir; // Arrasta o objeto do Portão para aqui no Inspector

    private bool jogadorNaZona = false;

    void Update()
    {
        // 1. Só verifica Input se eu for o dono deste boneco (IsLocalPlayer check implícito pela lógica)
        // Mas como este script está no MAPA (não no Player), temos de garantir que é o Player Local a clicar.
        
        if (jogadorNaZona && Input.GetKeyDown(KeyCode.E))
        {
             // Manda abrir o portão
             if (portaoParaAbrir != null)
             {
                 portaoParaAbrir.OpenDoorServerRpc();
             }
        }
    }

    // --- DETETAR SE O JOGADOR ENTROU NA ZONA DA CRIATURA ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se é um Player e se é o MEU Player (IsOwner)
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                jogadorNaZona = true;
                // Dica: Aqui podias fazer aparecer um texto "Pressiona E"
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                jogadorNaZona = false;
            }
        }
    }
}