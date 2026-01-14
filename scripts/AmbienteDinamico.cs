using UnityEngine;
using Unity.Netcode;

public class AmbienteDinamico : NetworkBehaviour
{
    public AudioSource minhaColuna;
    public AudioClip somPassado;  // Ocean
    public AudioClip somPresente; // Lava

    public override void OnNetworkSpawn()
    {
        // 1. SEGURANÇA: Se este boneco não é o meu, desliga o som dele!
        // Assim evitas ouvir o som do "outro" jogador misturado com o teu.
        if (!IsOwner) 
        {
            minhaColuna.Stop();
            minhaColuna.volume = 0; // Garante silêncio absoluto
            return; 
        }

        // 2. Se sou eu, escolho o som certo
        if (IsServer) // Sou o Host (Passado)
        {
            minhaColuna.clip = somPassado;
        }
        else // Sou o Cliente (Presente)
        {
            minhaColuna.clip = somPresente;
        }

        // 3. Tocar
        minhaColuna.Play();
    }
}