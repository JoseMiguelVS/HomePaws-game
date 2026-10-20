using UnityEngine;
using UnityEngine.Events;

// Punto de control (checkpoint). Cuando el jugador lo toca se convierte en el
// lugar donde reaparece si se cae al vacio o si muere. Solo avisa al
// GestorReaparicion y cambia de aspecto: asi es reutilizable en cualquier nivel.
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Etiqueta que activa el checkpoint.")]
    [SerializeField] private string etiquetaJugador = "Player";

    [Header("Punto de reaparicion")]
    [Tooltip("Donde aparece el jugador. Si se deja vacio se usa este objeto.")]
    [SerializeField] private Transform puntoReaparicion;

    [Header("Aspecto")]
    [Tooltip("Sprite que cambia de color al activarse (normalmente la bandera).")]
    [SerializeField] private SpriteRenderer bandera;
    [SerializeField] private Color colorInactivo = new Color(0.55f, 0.6f, 0.7f, 1f);
    [SerializeField] private Color colorActivo = new Color(0.35f, 0.9f, 0.45f, 1f);

    [Header("Efectos (opcional)")]
    [SerializeField] private GameObject efectoActivar;
    [SerializeField] private AudioSource sonidoActivar;

    [Header("Eventos")]
    [SerializeField] private UnityEvent alActivar;

    public bool Activado { get; private set; }

    // Posicion exacta donde debe aparecer el jugador.
    public Vector3 PuntoMundo
    {
        get { return puntoReaparicion != null ? puntoReaparicion.position : transform.position; }
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Awake()
    {
        if (bandera == null)
            bandera = GetComponentInChildren<SpriteRenderer>();

        Pintar(Activado);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Activado)
            return;

        if (!other.CompareTag(etiquetaJugador))
            return;

        Activar();
    }

    // Se puede llamar tambien desde un boton o desde otro script.
    public void Activar()
    {
        if (Activado)
            return;

        Activado = true;
        Pintar(true);

        if (efectoActivar != null)
            efectoActivar.SetActive(true);

        if (sonidoActivar != null)
            sonidoActivar.Play();

        alActivar?.Invoke();

        if (GestorReaparicion.Instancia != null)
            GestorReaparicion.Instancia.RegistrarCheckpoint(this);
    }

    // Vuelve al estado apagado (por ejemplo al reiniciar el nivel).
    public void Reiniciar()
    {
        Activado = false;
        Pintar(false);

        if (efectoActivar != null)
            efectoActivar.SetActive(false);
    }

    private void Pintar(bool activo)
    {
        if (bandera != null)
            bandera.color = activo ? colorActivo : colorInactivo;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 p = PuntoMundo;
        Gizmos.color = Activado ? Color.green : new Color(0.6f, 0.6f, 0.6f);
        Gizmos.DrawWireSphere(p, 0.35f);
        Gizmos.DrawLine(transform.position, p);
    }
}
