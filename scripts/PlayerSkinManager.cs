using UnityEngine;
using Unity.Netcode;

public class PlayerSkinManager : NetworkBehaviour
{
    [Header("REFERÊNCIAS")]
    public Animator _anim;
    public SpriteRenderer _sprite;
    public SpriteRenderer _fantasma;

    [Header("Configurações")]
    public float distanciaMapas = 70f;
    public RuntimeAnimatorController skinPassado;  // P1 (Castanho)
    public RuntimeAnimatorController skinPresente; // P2 (Espetado)
    
    // Tirei o Range para poderes escrever o número à mão se quiseres, mas deixei o default 0.4f
    public float opacidadeOutro = 0.4f; 

    public override void OnNetworkSpawn()
    {
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        if (_sprite == null && _anim != null) _sprite = _anim.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_sprite == null || _fantasma == null) return;

        // --- 1. A SKIN (Lógica pelo Mapa) ---
        // Se o boneco está no mapa da Esquerda -> Veste Passado.
        // Se está no mapa da Direita -> Veste Presente.
        // Isto corrige o Host ver dois bonecos castanhos!
        
        if (transform.position.x < 35f)
        {
            if (_anim.runtimeAnimatorController != skinPassado)
                _anim.runtimeAnimatorController = skinPassado;
        }
        else
        {
            if (_anim.runtimeAnimatorController != skinPresente)
                _anim.runtimeAnimatorController = skinPresente;
        }


        // --- 2. OPACIDADE (Quem sou eu?) ---
        // Se eu controlo este boneco -> Sólido.
        // Se eu não controlo -> Transparente.
        
        if (IsOwner)
        {
            _sprite.color = Color.white;
        }
        else
        {
            // Força 0.4 de transparência para ter a certeza que se nota
            _sprite.color = new Color(1f, 1f, 1f, 0.4f); 
        }


        // --- 3. O FANTASMA ---
        _fantasma.sprite = _sprite.sprite;
        _fantasma.flipX = _sprite.flipX;
        
        // O fantasma é SEMPRE transparente (0.4f)
        _fantasma.color = new Color(1f, 1f, 1f, 0.4f);

        // Posição do Fantasma (Troca de Mapa)
        if (transform.position.x < 35f) // Esquerda -> Fantasma na Direita
        {
            _fantasma.transform.localPosition = new Vector3(distanciaMapas, 0, 0);
        }
        else // Direita -> Fantasma na Esquerda
        {
            _fantasma.transform.localPosition = new Vector3(-distanciaMapas, 0, 0);
        }
    }
}