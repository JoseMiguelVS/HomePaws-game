using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Palanca que solo puede accionar el personaje indicado (por defecto Sol).
/// No abre puertas por si misma: cambia su estado y la Compuerta lo consulta,
/// de forma que se puede reutilizar en cualquier puzle.
///
/// Colocarla en un objeto con SpriteRenderer (lever_off / lever_on). La
/// distancia de uso se mide desde el personaje activo, no hace falta collider.
/// </summary>
public class Palanca : MonoBehaviour
{
    [Header("Quien puede usarla")]
    [Tooltip("Nombre del PerfilPersonaje que puede moverla (Sol).")]
    [SerializeField] private string personajeRequerido = "Sol";

    [SerializeField] private float distanciaUso = 1.9f;

    [Header("Aspecto")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteApagada;
    [SerializeField] private Sprite spriteEncendida;

    [Header("Estado")]
    [SerializeField] private bool empiezaActivada = false;

    [Header("Eventos")]
    [Tooltip("Se invoca al accionarla. Opcional: la Compuerta tambien puede consultar Activada.")]
    [SerializeField] private UnityEvent alActivar;

    public bool Activada { get; private set; }
    public bool JugadorCerca { get; private set; }
    public string PersonajeRequerido { get { return personajeRequerido; } }

    private float proximoAviso;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Activada = empiezaActivada;
        Pintar();
    }

    private void Update()
    {
        JugadorCerca = false;

        if (Activada)
            return;

        ControlDuo duo = ControlDuo.Instancia;

        if (duo == null || duo.Activo == null || duo.ControlBloqueado)
            return;

        if (DialogoUI.Instancia != null && DialogoUI.Instancia.Activo)
            return;

        float distancia = Vector2.Distance(duo.Activo.transform.position, transform.position);
        JugadorCerca = distancia <= distanciaUso;

        if (!JugadorCerca)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (PuedeUsar(duo))
            Activar();
        else
            AvisarPersonajeIncorrecto();
    }

    /// <summary>True si el personaje activo es el que puede mover la palanca.</summary>
    public bool PuedeUsar(ControlDuo duo)
    {
        return duo != null && duo.NombreActivo == personajeRequerido;
    }

    /// <summary>Acciona la palanca. Tambien se puede llamar desde un boton.</summary>
    public void Activar()
    {
        if (Activada)
            return;

        Activada = true;
        Pintar();

        GestorAudio.Sonar(Sonido.Puerta, 0.7f);

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje("Palanca accionada. Un candado menos.", 2.5f);

        alActivar?.Invoke();
    }

    /// <summary>Vuelve al estado inicial (para reiniciar el puzle).</summary>
    public void Reiniciar()
    {
        Activada = empiezaActivada;
        Pintar();
    }

    private void Pintar()
    {
        if (spriteRenderer == null)
            return;

        if (Activada && spriteEncendida != null)
            spriteRenderer.sprite = spriteEncendida;
        else if (!Activada && spriteApagada != null)
            spriteRenderer.sprite = spriteApagada;
    }

    private void AvisarPersonajeIncorrecto()
    {
        if (Time.time < proximoAviso)
            return;

        proximoAviso = Time.time + 2f;

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje("Solo " + personajeRequerido + " puede mover esta palanca.", 2.5f);
    }
}
