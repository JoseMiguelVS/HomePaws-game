using UnityEngine;

/// <summary>
/// Movimiento de patrulla para un NPC volador. Va y viene en horizontal
/// alrededor de su posicion inicial, con un balanceo vertical suave y una
/// pausa corta en los extremos. No usa fisica: mueve el transform.
///
/// Colocarlo en el objeto raiz del NPC (el que tiene el sprite como hijo).
/// </summary>
[DisallowMultipleComponent]
public class NpcVuelo : MonoBehaviour
{
    [Header("Patrulla horizontal")]
    [Tooltip("Distancia a cada lado de la posicion inicial.")]
    [SerializeField] private float distancia = 18f;

    [SerializeField] private float velocidad = 2.2f;

    [Tooltip("Pausa al llegar a cada extremo.")]
    [SerializeField] private float pausaEnExtremos = 0.7f;

    [Header("Aleteo")]
    [Tooltip("Cuanto sube y baja respecto a su altura base.")]
    [SerializeField] private float amplitudVertical = 0.55f;

    [SerializeField] private float frecuenciaVertical = 1.7f;

    [Header("Aspecto")]
    [Tooltip("Gira el sprite hacia el lado al que vuela.")]
    [SerializeField] private bool voltearAlCambiar = true;

    private Vector3 origen;
    private float direccion = 1f;
    private float espera;
    private float fase;

    private void Awake()
    {
        origen = transform.position;
        fase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        float x = transform.position.x;

        if (espera > 0f)
        {
            espera -= Time.deltaTime;
        }
        else
        {
            x += direccion * velocidad * Time.deltaTime;

            float minimo = origen.x - distancia;
            float maximo = origen.x + distancia;

            if (x >= maximo)
            {
                x = maximo;
                direccion = -1f;
                espera = pausaEnExtremos;
            }
            else if (x <= minimo)
            {
                x = minimo;
                direccion = 1f;
                espera = pausaEnExtremos;
            }
        }

        fase += Time.deltaTime * frecuenciaVertical;
        float y = origen.y + Mathf.Sin(fase) * amplitudVertical;

        transform.position = new Vector3(x, y, origen.z);

        if (voltearAlCambiar)
            transform.localScale = new Vector3(direccion >= 0f ? 1f : -1f, 1f, 1f);
    }
}
