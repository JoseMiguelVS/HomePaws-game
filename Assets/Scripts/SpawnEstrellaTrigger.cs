using UnityEngine;
using UnityEngine.UI;

public class SpawnEstrellaTrigger : MonoBehaviour
{
    [Header("Referencias de Personaje")]
    public GameObject estrellaObject;

    [Header("Referencias de UI (Narrativa)")]
    public GameObject panelNarrativa; 
    public Button botonContinuar;     

    private bool yaAparecio = false;

    void Start()
    {
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

            if (panelNarrativa != null)
            {
                panelNarrativa.SetActive(true);
                Time.timeScale = 0f; 
            }

            Debug.Log("¡Estrella apareció y se mostró la narrativa!");
        }
    }

    void CerrarNarrativa()
    {
        if (panelNarrativa != null)
        {
            panelNarrativa.SetActive(false);
            Time.timeScale = 1f;
        }

        Destroy(gameObject);
    }
}