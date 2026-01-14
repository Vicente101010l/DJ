using UnityEngine;
using Unity.Netcode;

public class TentaculoInteraction : MonoBehaviour
{
    [Header("CONFIGURAÇÃO")]
    public AudioSource audioSource;
    public AudioClip somErro; // O som "Bzzz" ou "Errado"

    private bool jogadorNaZona = false;

    void Update()
    {
        // Se o jogador estiver encostado e carregar no E
        if (jogadorNaZona && Input.GetKeyDown(KeyCode.E))
        {
            TocarSomErro();
        }
    }

    void TocarSomErro()
    {
        if (audioSource != null && somErro != null)
        {
            // PlayOneShot permite tocar o som várias vezes seguidas rapidamente
            audioSource.PlayOneShot(somErro);
        }
    }

    // --- DETETAR PRESENÇA ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se é o Player e se é o MEU boneco (para não ativar com o fantasma)
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                jogadorNaZona = true;
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