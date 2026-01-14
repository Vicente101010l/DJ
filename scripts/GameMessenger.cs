using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;

// Voltamos a usar NetworkBehaviour para poder comunicar pela rede
public class GameMessenger : NetworkBehaviour 
{
    public static GameMessenger Instance;
    public TMP_Text textComponent;
    public float tempoPadrao = 5f;

    void Awake()
    {
        // Garante que só existe um destes
        if (Instance == null) Instance = this;
    }

    // --- NOVA FUNÇÃO GLOBAL ---
    // Esta é a função que o Trigger vai chamar
    public void EnviarMensagemGlobal(string msgHost, string msgClient)
    {
        // Se eu sou o Servidor (Host), mando o comando para todos
        if (IsServer)
        {
            MostrarMensagemClientRpc(msgHost, msgClient);
        }
    }

    // Mostra mensagem só no meu ecrã (útil para "Erro" ou "Pistas")
    public void MostrarMensagemLocal(string mensagem)
    {
        textComponent.text = mensagem;
        StopAllCoroutines();
        StartCoroutine(ApagarTexto(tempoPadrao));
    }

    // [ClientRpc] significa: "Servidor, corre esta função em TODOS os clientes (e no host também)"
    [ClientRpc]
    void MostrarMensagemClientRpc(string msgHost, string msgClient)
    {
        string textoFinal = "";

        // Cada computador decide qual texto deve mostrar
        if (IsHost) 
        {
            textoFinal = msgHost; // O Passado vê isto
        }
        else 
        {
            textoFinal = msgClient; // O Presente vê isto
        }

        // Mostra o texto
        textComponent.text = textoFinal;
        
        // Reinicia o timer
        StopAllCoroutines();
        StartCoroutine(ApagarTexto(tempoPadrao));
    }

    IEnumerator ApagarTexto(float tempo)
    {
        yield return new WaitForSeconds(tempo);
        textComponent.text = "";
    }
}