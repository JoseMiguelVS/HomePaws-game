using UnityEngine;

// Caja empujable por fisicas 2D. Guarda su posicion inicial para poder
// devolverla a su sitio cuando se falla el puzle (ver ReiniciadorPuzzle).
//
// Las cajas pesadas solo se mueven con un personaje lo bastante fuerte
// (PerfilPersonaje.fuerzaEmpuje). Estrella se queda frenada al tocarlas y
// Sol, que empuja mas, las desplaza. Asi el puzle indica quien hace falta.
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

    [Header("Fuerza necesaria")]
    [Tooltip("Fuerza minima del PerfilPersonaje para poder moverla. Sol tiene 3, Estrella 0.8.")]
    [SerializeField] private float fuerzaNecesaria = 2f;

    [Tooltip("Empuje extra que recibe la caja del personaje fuerte.")]
    [SerializeField] private float fuerzaExtra = 16f;

    [Tooltip("Si se marca, los personajes debiles se frenan al tocarla en vez de arrastrarla.")]
    [SerializeField] private bool frenarPersonajesDebiles = true;

    [Tooltip("Segundos entre dos avisos de 'esta caja es muy pesada'.")]
    [SerializeField] private float esperaAviso = 3f;

    private Rigidbody2D rb;
    private Vector2 posicionInicial;
    private float rotacionInicial;
    private float proximoAviso;

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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        PerfilPersonaje perfil = collision.collider.GetComponentInParent<PerfilPersonaje>();

        // Sin perfil (por ejemplo, el nivel 1) la caja se comporta como siempre.
        if (perfil == null)
            return;

        float direccion = Mathf.Sign(transform.position.x - collision.transform.position.x);

        if (Mathf.Abs(direccion) < 0.01f)
            return;

        if (perfil.FuerzaEmpuje < fuerzaNecesaria)
        {
            FrenarPersonajeDebil(collision, direccion);
            return;
        }

        // Empuje extra: hace que Sol pueda mover la caja de forma constante
        // aunque el Rigidbody2D del jugador escriba su velocidad cada frame.
        float intensidad = fuerzaExtra * (perfil.FuerzaEmpuje / Mathf.Max(0.01f, fuerzaNecesaria));
        rb.AddForce(new Vector2(direccion * intensidad, 0f), ForceMode2D.Force);
    }

    private void FrenarPersonajeDebil(Collision2D collision, float direccion)
    {
        if (!frenarPersonajesDebiles)
            return;

        Rigidbody2D cuerpo = collision.collider.attachedRigidbody;

        if (cuerpo != null)
        {
            float velocidad = cuerpo.linearVelocity.x;

            // El personaje empuja hacia la caja: se frena en seco.
            if (Mathf.Abs(velocidad) > 0.05f && Mathf.Sign(velocidad) == direccion)
                cuerpo.linearVelocity = new Vector2(0f, cuerpo.linearVelocity.y);
        }

        if (Time.time < proximoAviso)
            return;

        proximoAviso = Time.time + esperaAviso;

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje("La caja es demasiado pesada. Necesitas a Sol.", 2.5f);
    }

    // Devuelve la caja a su posicion y rotacion iniciales, sin velocidad.
    public void Reiniciar()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = posicionInicial;
        rb.rotation = rotacionInicial;
    }

    // Para que la placa de presion pueda saber si esta encima una caja pesada.
    public float Masa
    {
        get { return rb != null ? rb.mass : masa; }
    }
}
