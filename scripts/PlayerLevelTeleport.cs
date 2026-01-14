using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerLevelTeleport : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Só o SERVIDOR (Host) precisa de ouvir este evento
        // É ele que vai pegar nos bonecos e mudá-los de sítio
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
             NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
        }
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // Esta função corre UMA VEZ por cada jogador que existe na cena
        if (!IsServer) return;

        ReposicionarEsteJogador();
    }

    void ReposicionarEsteJogador()
    {
        string nomeDoSpawn = "";

        // Verifica de quem é este boneco
        if (OwnerClientId == NetworkManager.ServerClientId)
        {
            // Se o dono deste boneco é o Servidor (Host/Passado)
            nomeDoSpawn = "PlayerSpawn1";
        }
        else
        {
            // Se o dono é qualquer outra pessoa (Client/Presente)
            nomeDoSpawn = "PlayerSpawn2";
        }

        GameObject pontoDeSpawn = GameObject.Find(nomeDoSpawn);

        if (pontoDeSpawn != null)
        {
            // O Servidor move o objeto. O NetworkTransform vai avisar o Cliente automaticamente.
            transform.position = pontoDeSpawn.transform.position;
            // Debug.Log($"Jogador {OwnerClientId} teleportado para {nomeDoSpawn}");
        }
        else
        {
            Debug.LogWarning($"AVISO: Não encontrei o objeto '{nomeDoSpawn}' na cena '{SceneManager.GetActiveScene().name}'!");
        }
    }
}