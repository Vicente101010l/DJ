using UnityEngine;
using Unity.Netcode;

public class MessageTrigger : MonoBehaviour 
{
    [TextArea] public string mensagemParaPassado;
    [TextArea] public string mensagemParaPresente;
    
    private bool jaMostrou = false;

    // Usei OnTriggerEnter2D porque agora tens 2 colliders e funciona bem com o "Is Trigger"
    void OnTriggerEnter2D(Collider2D other)
    {
        // Se já mostrámos, não vale a pena repetir logo de seguida
        if (jaMostrou) return;

        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            
            // Verificamos se quem bateu foi o Host (Passado)
            // Porque é o Host que está preso na parede!
            if (netObj != null && NetworkManager.Singleton.IsServer)
            {
                // Chama a função global no Messenger
                if(GameMessenger.Instance != null)
                {
                    GameMessenger.Instance.EnviarMensagemGlobal(mensagemParaPassado, mensagemParaPresente);
                    jaMostrou = true;
                    StartCoroutine(ResetCooldown()); // Opcional: permitir mostrar de novo passado um tempo
                }
            }
        }
    }

    System.Collections.IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(10f); // Espera 10 segundos antes de poder mostrar outra vez
        jaMostrou = false;
    }
}