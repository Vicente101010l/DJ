using UnityEngine;
using Unity.Netcode;

public class WrongCreature : MonoBehaviour
{
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Apenas mensagem na consola, sem Logger
            Debug.Log("❌ Esta não é a criatura certa...");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se é o Player e se é o Dono
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = false;
        }
    }
}