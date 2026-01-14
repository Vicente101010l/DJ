using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class NetworkManagerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;

    [SerializeField] Canvas canvas;

    private void Awake()
    {
        HostButton.onClick.AddListener(() =>
        {
            
            NetworkManager.Singleton.StartHost();
            canvas.gameObject.SetActive(false);

        });

        ClientButton.onClick.AddListener(() =>
        {
            
            NetworkManager.Singleton.StartClient();
            canvas.gameObject.SetActive(false);

        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
