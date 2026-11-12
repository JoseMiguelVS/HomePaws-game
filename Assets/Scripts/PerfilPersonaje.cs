using UnityEngine;

/// <summary>
/// Perfil de habilidad de un personaje jugable. Guarda sus estadisticas y las
/// aplica al PlayerController, al Rigidbody2D y al collider al empezar.
///
/// Colocarlo en el mismo objeto que PlayerController. Sirve para que Sol sea
/// más pesado y empuje más, y Estrella sea más ágil y salte más sin duplicar
/// el script de movimiento.
/// </summary>
[DisallowMultipleComponent]
public class PerfilPersonaje : MonoBehaviour
{
    [Header("Identidad")]
    [Tooltip("Nombre que muestra el HUD y que usan los puzles para pedir a un personaje.")]
    [SerializeField] private string nombre = "Sol";

    [Header("Movimiento")]
    [Tooltip("Velocidad maxima al correr.")]
    [SerializeField] private float velocidad = 5f;

    [Tooltip("Fuerza del salto. A más valor, más alto llega.")]
    [SerializeField] private float salto = 10f;

    [Header("Física")]
    [Tooltip("Masa del personaje. Mas masa = empuja mejor las cajas.")]
    [SerializeField] private float masa = 3f;

    [Tooltip("Escala de la gravedad. Mas valor = cae antes y salta menos.")]
    [SerializeField] private float escalaGravedad = 2f;

    [SerializeField] private Vector2 tamanoCollider = new Vector2(1.3376015f, 1.1027483f);
    [SerializeField] private Vector2 offsetCollider = new Vector2(-0.026910245f, -0.095409095f);

    [Header("Habilidades")]
    [Tooltip("Fuerza con la que empuja las cajas. La caja pesada pide 2 o más.")]
    [SerializeField] private float fuerzaEmpuje = 3f;

    public string Nombre { get { return nombre; } }
    public float FuerzaEmpuje { get { return fuerzaEmpuje; } }
    public float Salto { get { return salto; } }
    public float Velocidad { get { return velocidad; } }

    private void Awake()
    {
        Aplicar();
    }

    /// <summary>
    /// Copia las estadisticas del perfil a los componentes del personaje.
    /// Se puede volver a llamar si se cambian los valores en el Inspector.
    /// </summary>
    public void Aplicar()
    {
        PlayerController control = GetComponent<PlayerController>();
        if (control != null)
        {
            control.moveSpeed = velocidad;
            control.jumpForce = salto;
        }

        Rigidbody2D cuerpo = GetComponent<Rigidbody2D>();
        if (cuerpo != null)
        {
            cuerpo.mass = masa;
            cuerpo.gravityScale = escalaGravedad;
        }

        // El collider es opcional: si se deja en cero se respeta el que ya
        // tenga el personaje (Estrella usa uno más pequeño que Sol).
        if (tamanoCollider != Vector2.zero)
        {
            BoxCollider2D colisionador = GetComponent<BoxCollider2D>();
            if (colisionador != null)
            {
                colisionador.size = tamanoCollider;
                colisionador.offset = offsetCollider;
            }
        }
    }
}
