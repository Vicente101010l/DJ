using UnityEngine;

public class SomBotao : MonoBehaviour
{
    public AudioClip somClique;

    public void TocarSom()
    {
        if (somClique != null)
        {
            // Cria um objeto temporário que não morre quando a cena muda
            GameObject somObj = new GameObject("SomPortalTemp");
            AudioSource audio = somObj.AddComponent<AudioSource>();
            
            audio.clip = somClique;
            audio.Play();

            // O SEGREDO: Impede que o som seja destruído ao carregar o nível
            DontDestroyOnLoad(somObj);
            
            // Destroi o objeto automaticamente depois do som acabar (ex: 3 segundos)
            Destroy(somObj, 3f);
        }
    }
}