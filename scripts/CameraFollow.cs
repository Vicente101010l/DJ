using UnityEngine;
using Unity.Netcode;

public class CameraFollow : MonoBehaviour
{
    [Header("Configurações")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Ajustei o Y para 0 (topo-down 2D costuma ser melhor assim)
    public float smoothSpeed = 5f; 

    private Transform target;

    void LateUpdate()
    {
        // 1. Se não tenho alvo, PROCURO UM AGORA
        if (target == null)
        {
            EncontrarJogadorLocal();
            return; // Se ainda não encontrei, paro por aqui este frame
        }

        // 2. Se tenho alvo, SEGUE-O
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        transform.position = smoothedPosition;
    }

    void EncontrarJogadorLocal()
    {
        // Procura todos os bonecos no jogo
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            // Pergunta ao Netcode: "Este boneco é meu?"
            var netObj = p.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                target = p.transform;
                // Debug.Log("✅ Câmera encontrou o jogador no novo nível!");
                break;
            }
        }
    }
}