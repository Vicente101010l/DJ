using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Portal : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private Color portalColor = Color.red;
    [SerializeField] private Portal linkedPortal;

    [Header("Teleport")]
    [SerializeField] private float exitOffset = 1.2f;
    [SerializeField] private float teleportCooldown = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip teleportSound;

    private SpriteRenderer portalBody;
    private SpriteRenderer sinalizador;

    private NetworkVariable<bool> isActiveNetwork = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Anti-loop por player
    private Dictionary<ulong, float> lastTeleportTime = new Dictionary<ulong, float>();

    void Awake()
    {
        portalBody = GetComponent<SpriteRenderer>();

        sinalizador = transform.Find("Sinalizador")?.GetComponent<SpriteRenderer>();
        if (sinalizador == null)
        {
            foreach (Transform child in transform)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sinalizador = sr;
                    break;
                }
            }
        }

        if (portalBody != null)
            portalBody.color = Color.white;

        if (sinalizador != null)
            sinalizador.color = portalColor;
    }

    public override void OnNetworkSpawn()
    {
        isActiveNetwork.OnValueChanged += OnActiveChanged;
        UpdateVisual(isActiveNetwork.Value);
    }

    private void OnActiveChanged(bool oldValue, bool newValue)
    {
        UpdateVisual(newValue);
    }

    private void UpdateVisual(bool active)
    {
        if (portalBody != null)
            portalBody.color = active ? portalColor : Color.white;

        if (sinalizador != null)
            sinalizador.color = portalColor;
    }

    public void ActivatePortal()
    {
        if (!IsServer) return;
        isActiveNetwork.Value = true;
    }

    public void DeactivatePortal()
    {
        if (!IsServer) return;
        isActiveNetwork.Value = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!isActiveNetwork.Value) return;
        if (!other.CompareTag("Player")) return;
        if (linkedPortal == null) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null) return;

        ulong playerId = netObj.OwnerClientId;

        if (lastTeleportTime.ContainsKey(playerId) &&
            Time.time - lastTeleportTime[playerId] < teleportCooldown)
            return;

        lastTeleportTime[playerId] = Time.time;

        // 🔊 SOM apenas para quem usou o portal
        PlayTeleportSoundClientRpc(
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerId }
                }
            }
        );

        Teleport(other.transform);
    }

    private void Teleport(Transform player)
    {
        Vector2 exitDirection =
            (player.position - transform.position).normalized;

        if (exitDirection == Vector2.zero)
            exitDirection = Vector2.up;

        Vector2 exitPosition =
            (Vector2)linkedPortal.transform.position +
            exitDirection * exitOffset;

        player.position = exitPosition;
    }

    [ClientRpc]
    private void PlayTeleportSoundClientRpc(ClientRpcParams rpcParams = default)
    {
        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    public bool IsActive()
    {
        return isActiveNetwork.Value;
    }
}
