using System.Collections;
using UnityEngine;

/// <summary>
/// Cambia de seccion dentro del mismo nivel: al entrar el jugador activo hace
/// un fundido, coloca al duo en el destino, cambia el fondo de la camara y
/// mueve el punto de reaparicion.
///
/// Se coloca en un objeto con un Collider2D marcado como Trigger al final de
/// la primera seccion.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CambioSeccion : MonoBehaviour
{
    [Header("Destino del duo")]
    [Tooltip("Donde aparece el personaje activo al cambiar de seccion.")]
    [SerializeField] private Vector3 destino = new Vector3(83f, 28.6f, 0f);

    [Tooltip("Separacion horizontal entre los dos personajes al llegar.")]
    [SerializeField] private float separacion = 1.3f;

    [Header("Punto de reaparicion")]
    [Tooltip("Nuevo punto donde reaparece el duo si se cae en esta seccion.")]
    [SerializeField] private Vector3 puntoReaparicion = new Vector3(83f, 27.9f, 0f);

    [Header("Fondo")]
    [SerializeField] private FondoCamara fondo;

    [Header("Mensaje")]
    [SerializeField] private string mensaje = "Los tejados. El olor a hogar esta cerca.";

    [Header("Fundido")]
    [SerializeField] private float duracionFundido = 0.45f;

    private bool hecho;

    private void Reset()
    {
        Collider2D zona = GetComponent<Collider2D>();
        if (zona != null)
            zona.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hecho || !EsJugadorActivo(other))
            return;

        hecho = true;
        StartCoroutine(Transicion());
    }

    private IEnumerator Transicion()
    {
        ControlDuo duo = ControlDuo.Instancia;

        if (duo != null)
            duo.BloquearControl(true);

        if (FundidoUI.Instancia != null)
            yield return FundidoUI.Instancia.Fundir(duracionFundido, AplicarCambio, null);
        else
            AplicarCambio();

        if (duo != null)
            duo.BloquearControl(false);

        if (HUD_Duo.Instancia != null && !string.IsNullOrEmpty(mensaje))
            HUD_Duo.Mensaje(mensaje, 4.5f, new Color(1f, 0.72f, 0.45f, 1f));
    }

    private void AplicarCambio()
    {
        ControlDuo duo = ControlDuo.Instancia;

        if (duo != null && duo.Activo != null)
        {
            Colocar(duo.Activo, destino);

            if (duo.DuoActivo && duo.Inactivo != null)
            {
                float lado = duo.Activo.transform.localScale.x >= 0f ? 1f : -1f;
                Colocar(duo.Inactivo, destino + Vector3.right * separacion * lado);
            }

            // La zona de puzle se queda atras: hay que salir de ella aunque el
            // trigger no llegue a disparar el exit al teletransportar.
            duo.SalirPuzzle();
            duo.CentrarCamara();
        }

        if (fondo != null)
            fondo.MostrarTejados();

        if (GestorReaparicion.Instancia != null)
            GestorReaparicion.Instancia.EstablecerPuntoActual(puntoReaparicion);
    }

    private static void Colocar(PlayerController jugador, Vector3 posicion)
    {
        if (jugador == null)
            return;

        Rigidbody2D cuerpo = jugador.GetComponent<Rigidbody2D>();

        if (cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            cuerpo.position = posicion;
        }

        jugador.transform.position = posicion;
    }

    private static bool EsJugadorActivo(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return false;

        PlayerController jugador = other.GetComponent<PlayerController>();

        if (jugador == null)
            return false;

        ControlDuo duo = ControlDuo.Instancia;

        return duo != null ? jugador == duo.Activo : jugador.controlActivo;
    }
}
