using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Zona que reconoce un puzle. Mientras alguno de los dos personajes esta
/// dentro, avisa a ControlDuo para que el companero deje de seguir y se quede
/// esperando, y mantiene activo el estado de puzle aunque se cambie con TAB.
///
/// Lanza un UnityEvent al entrar y otro al salir para enganchar cualquier
/// accion extra desde el Inspector.
/// Colocarla en un objeto con un Collider2D marcado como Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ZonaPuzzle : MonoBehaviour
{
    [Header("Identidad")]
    [Tooltip("Nombre que muestra el HUD mientras el jugador esta en la zona.")]
    [SerializeField] private string nombrePuzzle = "Puzle";

    [Tooltip("Personaje que conviene usar. Si se deja vacio no se sugiere ninguno.")]
    [SerializeField] private string personajeSugerido = "";

    [Header("Aviso")]
    [SerializeField] private string mensajeEntrada =
        "Puzle: el companero espera. Pulsa TAB para cambiar de personaje.";

    [SerializeField] private float duracionMensaje = 4.5f;

    [Header("Eventos")]
    [Tooltip("Se invoca cuando el primer personaje entra en la zona del puzle.")]
    [SerializeField] private UnityEvent alEntrarPuzzle;

    [Tooltip("Se invoca cuando el ultimo personaje sale de la zona del puzle.")]
    [SerializeField] private UnityEvent alSalirPuzzle;

    private readonly List<Collider2D> dentro = new List<Collider2D>();
    private bool activa;

    public bool Activa { get { return activa; } }
    public string NombrePuzzle { get { return nombrePuzzle; } }

    private void Reset()
    {
        Collider2D zona = GetComponent<Collider2D>();
        if (zona != null)
            zona.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!EsJugador(other))
            return;

        if (!dentro.Contains(other))
            dentro.Add(other);

        RevisarEstado();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!EsJugador(other))
            return;

        dentro.Remove(other);
        RevisarEstado();
    }

    // Cambiar de personaje con TAB no dispara triggers, asi que revisamos el
    // estado cada frame para que la zona siga a quien tenga el control.
    private void Update()
    {
        RevisarEstado();
    }

    private void RevisarEstado()
    {
        bool hayJugadorDentro = false;

        for (int i = dentro.Count - 1; i >= 0; i--)
        {
            if (dentro[i] == null)
            {
                dentro.RemoveAt(i);
                continue;
            }

            hayJugadorDentro = true;
        }

        if (hayJugadorDentro && !activa)
            Entrar();
        else if (!hayJugadorDentro && activa)
            Salir();
    }

    private void Entrar()
    {
        activa = true;

        ControlDuo duo = ControlDuo.Instancia;
        if (duo != null)
            duo.EntrarPuzzle(nombrePuzzle);

        string texto = mensajeEntrada;

        if (!string.IsNullOrEmpty(personajeSugerido))
            texto += " Este puzle necesita a " + personajeSugerido + ".";

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje(texto, duracionMensaje, new Color(1f, 0.78f, 0.35f, 1f));

        alEntrarPuzzle?.Invoke();
    }

    private void Salir()
    {
        activa = false;

        ControlDuo duo = ControlDuo.Instancia;
        if (duo != null)
            duo.SalirPuzzle();

        alSalirPuzzle?.Invoke();
    }

    private static bool EsJugador(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player"))
            return false;

        return other.GetComponent<PlayerController>() != null;
    }
}
