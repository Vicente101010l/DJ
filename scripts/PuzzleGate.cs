using UnityEngine;
using Unity.Netcode;

public class PuzzleGate : NetworkBehaviour
{
    [Header("PAREDES")]
    public GameObject WallBefore;
    public GameObject WallTop;
    public GameObject WallBottom;

    [Header("EFEITOS VISUAIS E SONOROS")]
    public AudioClip somSucesso; 

    [Header("NARRATIVA (Mensagens Diferentes)")]
    [TextArea] public string mensagemParaPassado = "O teu eu do Futuro conseguiu libertar-te!";
    [TextArea] public string mensagemParaPresente = "Parabéns! A alteração que fizeste abriu o caminho no passado.";

    // Sincroniza se o portão está aberto ou fechado
    private NetworkVariable<bool> isDoorOpen = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        isDoorOpen.OnValueChanged += OnDoorStateChanged;
        UpdateWallState(isDoorOpen.Value);
    }

    public override void OnNetworkDespawn()
    {
        isDoorOpen.OnValueChanged -= OnDoorStateChanged;
    }

    // Chamado pelo script da CriaturaGigante
    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        if (!isDoorOpen.Value)
        {
            isDoorOpen.Value = true;
        }
    }

    void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        UpdateWallState(newValue);

        // Se a porta acabou de abrir (passou de false para true)
        if (newValue == true && previousValue == false)
        {
            ExecutarEfeitosLocais();
        }
    }

    void ExecutarEfeitosLocais()
    {
        // 1. Tocar Som Global (na posição da câmera local)
        if (somSucesso != null)
        {
            AudioSource.PlayClipAtPoint(somSucesso, Camera.main.transform.position, 1f);
        }

        // 2. Mostrar Mensagem Diferente para cada Jogador
        if (GameMessenger.Instance != null)
        {
            string textoFinal;

            if (IsHost) 
            {
                // Sou o Jogador do Passado
                textoFinal = mensagemParaPassado;
            }
            else 
            {
                // Sou o Jogador do Presente
                textoFinal = mensagemParaPresente;
            }

            // Usa a função Local do teu GameMessenger, porque a lógica já correu nos dois PCs
            GameMessenger.Instance.MostrarMensagemLocal(textoFinal);
        }
    }

    void UpdateWallState(bool open)
    {
        if (WallBefore == null || WallTop == null || WallBottom == null) return;

        if (!open) // FECHADO
        {
            WallBefore.SetActive(true);
            WallTop.SetActive(false);
            WallBottom.SetActive(false);
        }
        else // ABERTO
        {
            WallBefore.SetActive(false);
            WallTop.SetActive(true);
            WallBottom.SetActive(true);
        }
    }
}