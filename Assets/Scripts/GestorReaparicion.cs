using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Guarda el ultimo checkpoint activado y devuelve al jugador ahi cuando se cae
// al vacio o cuando otro script llama a Reaparecer(). Tambien puede devolver el
// puzle a su estado inicial para que el jugador no quede atrapado.
//
// Colocar este script en un objeto vacio de la escena (por ejemplo "GestorReaparicion").
public class GestorReaparicion : MonoBehaviour
{
    // Acceso rapido desde los checkpoints y desde otros scripts.
    public static GestorReaparicion Instancia { get; private set; }

    [Header("Jugador")]
    [Tooltip("Si se deja vacio se busca por etiqueta al empezar.")]
    [SerializeField] private Transform jugador;
    [SerializeField] private string etiquetaJugador = "Player";

    [Header("Punto de partida")]
    [Tooltip("Donde reaparece el jugador mientras no haya tocado ningun checkpoint.")]
    [SerializeField] private Transform puntoInicial;

    [Header("Caida al vacio")]
    [SerializeField] private bool reaparecerAlCaer = true;
    [Tooltip("Si el jugador baja de esta altura, reaparece.")]
    [SerializeField] private float limiteCaidaY = -30f;

    [Header("Reaparicion")]
    [Tooltip("Desplazamiento respecto al checkpoint (para no aparecer dentro del suelo).")]
    [SerializeField] private Vector2 desplazamiento = new Vector2(0f, 0.6f);
    [Tooltip("Tiempo minimo entre dos reapariciones seguidas.")]
    [SerializeField] private float esperaEntreReapariciones = 0.5f;

    [Header("Puzzle (opcional)")]
    [SerializeField] private ReiniciadorPuzzle reiniciadorPuzzle;
    [SerializeField] private bool reiniciarPuzzleAlReaparecer = true;

    [Header("Eventos")]
    [SerializeField] private UnityEvent alReaparecer;

    [Header("Checkpoints de la escena")]
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    private Vector3 puntoActual;
    private Rigidbody2D cuerpo;
    private float proximaReaparicion;
    private int reapariciones;

    public Vector3 PuntoActual { get { return puntoActual; } }
    public int Reapariciones { get { return reapariciones; } }

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        BuscarJugador();

        if (puntoInicial != null)
            puntoActual = puntoInicial.position;
        else if (jugador != null)
            puntoActual = jugador.position;

        // Si algun checkpoint ya estaba activado en el editor, manda ese.
        if (checkpoints != null)
        {
            foreach (Checkpoint cp in checkpoints)
            {
                if (cp != null && cp.Activado)
                {
                    puntoActual = cp.PuntoMundo;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (!reaparecerAlCaer || jugador == null)
            return;

        if (jugador.position.y < limiteCaidaY)
            Reaparecer();
    }

    private void BuscarJugador()
    {
        if (jugador != null)
            return;

        GameObject encontrado = GameObject.FindGameObjectWithTag(etiquetaJugador);
        if (encontrado != null)
            jugador = encontrado.transform;
    }

    private Rigidbody2D ObtenerCuerpo()
    {
        if (cuerpo != null)
            return cuerpo;

        BuscarJugador();
        if (jugador != null)
            cuerpo = jugador.GetComponent<Rigidbody2D>();

        return cuerpo;
    }

    // Lo llaman los checkpoints al activarse.
    public void RegistrarCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
            return;

        puntoActual = checkpoint.PuntoMundo;
    }

    // Coloca al jugador en el ultimo punto seguro y avisa a quien escuche.
    public void Reaparecer()
    {
        if (Time.time < proximaReaparicion)
            return;

        BuscarJugador();
        if (jugador == null)
            return;

        proximaReaparicion = Time.time + esperaEntreReapariciones;
        reapariciones++;

        Vector3 destino = puntoActual + (Vector3)desplazamiento;

        Rigidbody2D rb = ObtenerCuerpo();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.rotation = 0f;
            rb.position = destino;
        }
        else
        {
            jugador.position = destino;
        }

        if (reiniciarPuzzleAlReaparecer && reiniciadorPuzzle != null)
            reiniciadorPuzzle.ReiniciarTodo();

        alReaparecer?.Invoke();
    }

    // Reaparece en el punto de partida (sin borrar los checkpoints).
    public void ReaparecerEnElInicio()
    {
        if (puntoInicial != null)
            puntoActual = puntoInicial.position;

        Reaparecer();
    }

    // Empieza el nivel de cero: apaga los checkpoints y vuelve al punto inicial.
    public void ReiniciarNivel()
    {
        if (checkpoints != null)
        {
            foreach (Checkpoint cp in checkpoints)
            {
                if (cp != null)
                    cp.Reiniciar();
            }
        }

        ReaparecerEnElInicio();
    }

    // Anade a la lista los checkpoints que esten sueltos en la escena.
    public void BuscarCheckpointsEnEscena()
    {
        Checkpoint[] encontrados = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (Checkpoint cp in encontrados)
        {
            if (cp != null && !checkpoints.Contains(cp))
                checkpoints.Add(cp);
        }
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(puntoActual, 0.4f);
    }
}
