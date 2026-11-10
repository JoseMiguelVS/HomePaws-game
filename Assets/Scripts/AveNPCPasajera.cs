using UnityEngine;

public class AveNPCPasajera : MonoBehaviour
{
    [Header("Configuración de Vuelo")]
    public float velocidad = 3f;       
    public float direccionX = 1f;     
    public float tiempoDeVida = 15f;  

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && direccionX < 0)
        {
            spriteRenderer.flipX = true;
        }

        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        transform.Translate(Vector3.right * direccionX * velocidad * Time.deltaTime);
    }
}