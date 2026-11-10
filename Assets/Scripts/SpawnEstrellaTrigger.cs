using UnityEngine;
using UnityEngine.UI;

public class SpawnEstrellaTrigger : MonoBehaviour
{
    [Header("Referencias de Personaje")]
    public GameObject estrellaObject;

    [Header("Referencias de UI (Narrativa)")]
    public GameObject panelNarrativa; 
    public Button botonContinuar;     

    [Header("Audio")]
    private AudioSource audioSource; // Componente de audio en este objeto

    private bool yaAparecio = false;

    void Start()
    {
        // Obtenemos automáticamente el AudioSource del GameObject
        audioSource = GetComponent<AudioSource>();

        if (panelNarrativa != null)
        {
            panelNarrativa.SetActive(false);
        }

        if (botonContinuar != null)
        {
            botonContinuar.onClick.AddListener(CerrarNarrativa);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!yaAparecio && collision.CompareTag("Player"))
        {
            yaAparecio = true;

            if (estrellaObject != null)
            {
                estrellaObject.transform.position = collision.transform.position + new Vector3(1.5f, 0f, 0f);
                estrellaObject.SetActive(true);
            }

            // Reproducir el sonido del maullido
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            if (panelNarrativa != null)
            {
                panelNarrativa.SetActive(true);
                Time.timeScale = 0f; // Pausa el juego para mostrar la narrativa
            }

            Debug.Log("¡Estrella apareció, sonó el maullido y se mostró la narrativa!");
        }
    }

    void CerrarNarrativa()
    {
        if (panelNarrativa != null)
        {
            panelNarrativa.SetActive(false);
            Time.timeScale = 1f; // Reanuda el juego
        }

        Destroy(gameObject);
    }
}