using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PortalManager : NetworkBehaviour
{
    public static PortalManager Instance;

    [SerializeField] private List<PortalPair> portalPairs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para definir estado específico - usado pelo botão
    public void SetPortalPairState(int pairId, bool active)
    {
        if (!IsServer) return;

        PortalPair pair = portalPairs.Find(p => p.pairId == pairId);
        if (pair == null)
        {
            Debug.LogWarning($"Portal pair with ID {pairId} not found!");
            return;
        }

        pair.SetActive(active);
        Debug.Log($"Set portal pair {pairId} to: {active}");
    }

    // Método antigo de toggle (mantido para compatibilidade)
    public void TogglePortalPair(int pairId)
    {
        if (!IsServer) return;

        PortalPair pair = portalPairs.Find(p => p.pairId == pairId);
        if (pair == null) return;

        // Determinar novo estado baseado no primeiro portal do par
        bool newState = true;
        if (pair.portalA != null)
        {
            newState = !pair.portalA.IsActive();
        }
        else if (pair.portalB != null)
        {
            newState = !pair.portalB.IsActive();
        }

        pair.SetActive(newState);
    }
}