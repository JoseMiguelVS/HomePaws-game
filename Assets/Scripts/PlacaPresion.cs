using UnityEngine;
using UnityEngine.Events;

// Placa de presion: detecta al jugador y/o a las cajas que se apoyan encima.
// No sabe nada de puertas: solo avisa mediante eventos (asi es reutilizable).
//
// Con "masaMinima" solo cuenta el peso de verdad: la caja pesada de Sol la
// pulsa, pero un personaje ligero (Estrella) no. El cambio de color/sprite
// avisa de que esta pulsada.
[RequireComponent(typeof(Collider2D))]
public class PlacaPresion : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Etiquetas que cuentan como peso valido (jugador, cajas...).")]
    [SerializeField] private string[] etiquetasValidas = { "Player", "Caja" };

    [Tooltip("Cuantos objetos hay que colocar encima para activarla.")]
    [SerializeField] private int cantidadNecesaria = 1;

    [Tooltip("Si se marca, una vez pulsada ya no se suelta.")]
    [SerializeField] private bool permanecePulsada = false;

    [Header("Peso")]
    [Tooltip("Masa minima para contar. 0 = cualquier objeto valido. La caja pesada tiene 6.")]
    [SerializeField] private float masaMinima = 0f;

    [Header("Aspecto (opcional)")]
    [Tooltip("Sprite de la placa. Si se deja vacio se busca en este objeto.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite spriteArriba;
    [SerializeField] private Sprite spriteAbajo;

    [Tooltip("Color cuando esta pulsada (si no se usan sprites).")]
    [SerializeField] private Color colorPulsada = new Color(0.45f, 0.95f, 0.5f, 1f);

    [Header("Eventos")]
    [SerializeField] private UnityEvent alPulsar;
    [SerializeField] private UnityEvent alSoltar;

    public bool EstaPulsada { get; private set; }

    private int objetosEncima;
    private Color colorOriginal = Color.white;

    // Al anadir el componente, deja el collider como trigger.
    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            colorOriginal = spriteRenderer.color;

        Pintar(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!EsValido(other))
            return;

        objetosEncima++;
        ActualizarEstado();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!EsValido(other))
            return;

        objetosEncima = Mathf.Max(0, objetosEncima - 1);
        ActualizarEstado();
    }

    private bool EsValido(Collider2D other)
    {
        bool etiquetaValida = false;

        foreach (string etiqueta in etiquetasValidas)
        {
            if (!string.IsNullOrEmpty(etiqueta) && other.CompareTag(etiqueta))
            {
                etiquetaValida = true;
                break;
            }
        }

        if (!etiquetaValida)
            return false;

        if (masaMinima <= 0f)
            return true;

        Rigidbody2D cuerpo = other.attachedRigidbody;

        return cuerpo != null && cuerpo.mass >= masaMinima;
    }

    private void ActualizarEstado()
    {
        // Modo permanente: una vez pulsada, se ignora el soltar.
        if (permanecePulsada && EstaPulsada)
            return;

        bool pulsada = objetosEncima >= cantidadNecesaria;

        if (pulsada == EstaPulsada)
            return;

        EstaPulsada = pulsada;
        Pintar(pulsada);

        if (pulsada)
            alPulsar?.Invoke();
        else
            alSoltar?.Invoke();
    }

    private void Pintar(bool pulsada)
    {
        if (spriteRenderer == null)
            return;

        if (pulsada && spriteAbajo != null)
        {
            spriteRenderer.sprite = spriteAbajo;
            spriteRenderer.color = colorOriginal;
            return;
        }

        if (!pulsada && spriteArriba != null)
        {
            spriteRenderer.sprite = spriteArriba;
            spriteRenderer.color = colorOriginal;
            return;
        }

        spriteRenderer.color = pulsada ? colorPulsada : colorOriginal;
    }

    // Suelta la placa y avisa, para volver al estado inicial del puzle.
    public void Reiniciar()
    {
        objetosEncima = 0;

        bool estabaPulsada = EstaPulsada;
        EstaPulsada = false;
        Pintar(false);

        if (estabaPulsada)
            alSoltar?.Invoke();
    }
}
