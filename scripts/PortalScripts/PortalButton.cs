using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PortalButton : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private int portalPairId;

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color pressedColor = Color.green;

    private SpriteRenderer buttonRenderer;
    
    // Lista para rastrear múltiplos jogadores no botão
    private List<ulong> playersInButton = new List<ulong>();

    private NetworkVariable<bool> isPressedNetwork = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        buttonRenderer = GetComponent<SpriteRenderer>();
        if (buttonRenderer != null)
            buttonRenderer.color = normalColor;
    }

    public override void OnNetworkSpawn()
    {
        isPressedNetwork.OnValueChanged += OnPressedChanged;
        UpdateVisual(isPressedNetwork.Value);
    }

    private void OnPressedChanged(bool oldValue, bool newValue)
    {
        UpdateVisual(newValue);
        
        // Ativar/desativar o par de portais
        if (PortalManager.Instance != null)
        {
            PortalManager.Instance.SetPortalPairState(portalPairId, newValue);
        }
    }

    private void UpdateVisual(bool pressed)
    {
        if (buttonRenderer != null)
            buttonRenderer.color = pressed ? pressedColor : normalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null) return;

        ulong playerId = netObj.OwnerClientId;
        
        if (!IsServer)
        {
            PlayerEnterServerRpc(playerId);
        }
        else
        {
            PlayerEnterInternal(playerId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerEnterServerRpc(ulong playerId)
    {
        PlayerEnterInternal(playerId);
    }

    private void PlayerEnterInternal(ulong playerId)
    {
        // Adicionar jogador à lista se não estiver lá
        if (!playersInButton.Contains(playerId))
        {
            playersInButton.Add(playerId);
        }
        
        // Ativar botão se ainda não estiver ativado
        if (!isPressedNetwork.Value)
        {
            isPressedNetwork.Value = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null) return;

        ulong playerId = netObj.OwnerClientId;
        
        if (!IsServer)
        {
            PlayerExitServerRpc(playerId);
        }
        else
        {
            PlayerExitInternal(playerId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerExitServerRpc(ulong playerId)
    {
        PlayerExitInternal(playerId);
    }

    private void PlayerExitInternal(ulong playerId)
    {
        // Remover jogador da lista
        if (playersInButton.Contains(playerId))
        {
            playersInButton.Remove(playerId);
        }
        
        // Desativar botão se não houver mais jogadores
        if (playersInButton.Count == 0 && isPressedNetwork.Value)
        {
            isPressedNetwork.Value = false;
        }
    }
}