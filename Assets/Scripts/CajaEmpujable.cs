using UnityEngine;

// Caja empujable por fisicas 2D. Guarda su posicion inicial para poder
// devolverla a su sitio cuando se falla el puzle (ver ReiniciadorPuzzle).
[RequireComponent(typeof(Rigidbody2D))]
public class CajaEmpujable : MonoBehaviour
{
    [Header("Fisica (se aplica al Rigidbody2D)")]
    [Tooltip("Si se desmarca, respeta los valores que ya tengas en el Rigidbody2D.")]
    [SerializeField] private bool configurarFisica = true;

    [Tooltip("Peso de la caja. Mas masa = cuesta mas empujarla.")]
    [SerializeField] private float masa = 1f;

    [Tooltip("Gravedad. Se iguala a la del jugador (3) para que caiga al mismo ritmo.")]
    [SerializeField] private float escalaGravedad = 3f;

    [Tooltip("Arrastre lineal: cuanto mas alto, antes se detiene al dejar de empujar.")]
    [SerializeField] private float arrastreLineal = 2f;

    [SerializeField] private float arrastreAngular = 4f;

    private Rigidbody2D rb;
    private Vector2 posicionInicial;
    private float rotacionInicial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        posicionInicial = rb.position;
        rotacionInicial = rb.rotation;

        if (configurarFisica)
            ConfigurarFisica();
    }

    private void ConfigurarFisica()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = masa;
        rb.gravityScale = escalaGravedad;
        rb.freezeRotation = true;
        rb.linearDamping = arrastreLineal;
        rb.angularDamping = arrastreAngular;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Devuelve la caja a su posicion y rotacion iniciales, sin velocidad.
    public void Reiniciar()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = posicionInicial;
        rb.rotation = rotacionInicial;
    }
}
