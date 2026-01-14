using UnityEngine;
using Unity.Netcode; // <--- OBRIGATÓRIO PARA ISTO FUNCIONAR

public class SomParede : MonoBehaviour
{
    [Header("Configuração")]
    public AudioSource audioSource;
    public AudioClip somBatida;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Verifica se é um Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Vai buscar o componente de Rede do boneco que bateu
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();

            // 3. O FILTRO MÁGICO:
            // Só toca o som SE o boneco que bateu for o boneco controlado por MIM (Local Player).
            // Se for o boneco do outro jogador (IsOwner = false), ignora e mantém o silêncio.
            if (netObj != null && netObj.IsOwner)
            {
                if (audioSource != null && somBatida != null)
                {
                    // Variação de pitch para não ser irritante
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.PlayOneShot(somBatida);
                }
            }
        }
    }
}