using UnityEngine;

// Puerta: desplaza su parte movil entre una posicion cerrada y otra abierta.
// No decide cuando abrir: eso lo hacen la placa, un boton o una condicion de puzle.
public class Puerta : MonoBehaviour
{
    [Header("Partes")]
    [Tooltip("Transform que se mueve. Si se deja vacio, se usa este mismo objeto.")]
    [SerializeField] private Transform parteMovil;

    [Tooltip("Desplazamiento local cuando la puerta esta abierta.")]
    [SerializeField] private Vector3 desplazamientoAbierta = new Vector3(0f, 3f, 0f);

    [Tooltip("Velocidad de apertura/cierre.")]
    [SerializeField] private float velocidad = 3f;

    [Header("Estado inicial")]
    [SerializeField] private bool empiezaAbierta = false;

    private Vector3 posicionCerrada;
    private bool objetivoAbierta;

    public bool EstaAbierta => objetivoAbierta;

    private void Awake()
    {
        if (parteMovil == null)
            parteMovil = transform;

        posicionCerrada = parteMovil.localPosition;
        objetivoAbierta = empiezaAbierta;

        if (empiezaAbierta)
            parteMovil.localPosition = posicionCerrada + desplazamientoAbierta;
    }

    public void Abrir() => objetivoAbierta = true;
    public void Cerrar() => objetivoAbierta = false;
    public void Alternar() => objetivoAbierta = !objetivoAbierta;

    private void Update()
    {
        Vector3 destino = posicionCerrada +
            (objetivoAbierta ? desplazamientoAbierta : Vector3.zero);

        parteMovil.localPosition = Vector3.MoveTowards(
            parteMovil.localPosition,
            destino,
            velocidad * Time.deltaTime
        );
    }
}
