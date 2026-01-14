using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovementNew : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // A coluna dos passos
    [SerializeField] private AudioClip[] passosRelva; // Lista para o Passado
    [SerializeField] private AudioClip[] passosPedra; // Lista para o Presente
    [SerializeField] private float tempoEntrePassos = 0.4f; // Velocidade (0.4 é bom para andar normal)

    private AudioClip[] passosAtuais; // Vai guardar a lista certa
    private float timerPassos;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 movement;
    private bool isInitialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Transform spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null)
        {
            anim = spriteTransform.GetComponent<Animator>();
            sr = spriteTransform.GetComponent<SpriteRenderer>();
        }
    }

    public override void OnNetworkSpawn()
    {
        SetupInitialPosition();
        isInitialized = true;

        // --- LÓGICA DO SOM DO CHÃO ---
        // Se sou o Host, estou no Passado (Relva). Se sou Client, estou no Presente (Pedra).
        if (IsServer)
        {
            passosAtuais = passosRelva;
        }
        else
        {
            passosAtuais = passosPedra;
        }
    }

    void SetupInitialPosition()
    {
        if (!IsOwner) return;

        GameObject p1Spawn = GameObject.Find("PlayerSpawn1");
        GameObject p2Spawn = GameObject.Find("PlayerSpawn2");

        if (p1Spawn == null || p2Spawn == null) return;

        if (IsHost)
            transform.position = p1Spawn.transform.position;
        else
            RquestPositionAtServerRpc(p2Spawn.transform.position);
    }

    [ServerRpc]
    void RquestPositionAtServerRpc(Vector3 position)
    {
        transform.position = position;
    }

    void Update()
    {
        if (!IsOwner || !isInitialized) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (anim != null)
        {
            anim.SetInteger("horizontal", (int)movement.x);
            anim.SetInteger("vertical", (int)movement.y);
            
            bool aAndar = movement.sqrMagnitude > 0;
            anim.SetBool("isMoving", aAndar);

            if (movement.x != 0)
                sr.flipX = movement.x < 0;

            // --- TOCA O SOM SE ESTIVER A ANDAR ---
            if (aAndar)
            {
                GerirPassos();
            }
            else
            {
                timerPassos = 0; // Reinicia para o som sair logo ao arrancar
            }
        }
    }

    void GerirPassos()
    {
        timerPassos -= Time.deltaTime;

        if (timerPassos <= 0)
        {
            // Toca um som aleatório da lista certa
            if (passosAtuais != null && passosAtuais.Length > 0)
            {
                int index = Random.Range(0, passosAtuais.Length);
                
                // O volumeScale a 0.8 serve para os passos não serem tão altos como a música
                audioSource.PlayOneShot(passosAtuais[index], 0.6f); 
            }
            timerPassos = tempoEntrePassos;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || !isInitialized) return;

        Vector2 dir = Vector2.ClampMagnitude(movement, 1f);

        if (IsHost)
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        else
            ClientMoveRequestServerRpc(dir);
    }

    [ServerRpc]
    void ClientMoveRequestServerRpc(Vector2 dir)
    {
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner || !isInitialized) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Colidiu com outro jogador!");

            // Só o servidor muda a cena
            if (IsServer)
            {
                LoadNextSceneForAll();
            }
            else
            {
                // Se não for servidor, pede para o servidor carregar
                RequestServerLoadSceneServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestServerLoadSceneServerRpc()
    {
        LoadNextSceneForAll();
    }

private void LoadNextSceneForAll()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.SceneManager.LoadScene("FinalScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}