using UnityEngine;

/// <summary>
/// Llave de puzle que solo puede recoger el personaje indicado (por defecto
/// Estrella). La Compuerta consulta Recogida, asi que no necesita eventos.
///
/// Colocarla en un objeto con SpriteRenderer (llave) y un CircleCollider2D
/// marcado como Trigger. Conviene ponerla en una repisa alta a la que solo
/// llegue el salto de Estrella.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Llave : MonoBehaviour
{
    [Header("Quien puede recogerla")]
    [Tooltip("Nombre del PerfilPersonaje que puede coger la llave (Estrella).")]
    [SerializeField] private string personajeRequerido = "Estrella";

    [Header("Aspecto")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D zona;

    [Tooltip("Cartel flotante de la llave. Se oculta al recogerla. Opcional.")]
    [SerializeField] private IndicadorFlotante indicador;

    [Header("Flotacion")]
    [SerializeField] private float alturaFlotacion = 0.18f;
    [SerializeField] private float velocidadFlotacion = 2.5f;

    public bool Recogida { get; private set; }
    public string PersonajeRequerido { get { return personajeRequerido; } }

    private Vector3 posicionBase;
    private float proximoAviso;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (zona == null)
            zona = GetComponent<Collider2D>();

        posicionBase = transform.position;
    }

    private void Update()
    {
        if (Recogida)
            return;

        transform.position = posicionBase + Vector3.up * (Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Recogida || !other.CompareTag("Player"))
            return;

        ControlDuo duo = ControlDuo.Instancia;

        if (duo != null && duo.NombreActivo != personajeRequerido)
        {
            AvisarPersonajeIncorrecto();
            return;
        }

        Recoger();
    }

    /// <summary>Recoge la llave. Tambien se puede llamar desde otro script.</summary>
    public void Recoger()
    {
        if (Recogida)
            return;

        Recogida = true;

        if (zona != null)
            zona.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (indicador != null)
            indicador.Mostrar(false);

        GestorAudio.Sonar(Sonido.Moneda, 0.9f);

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje("Estrella recogi\u00f3 la llave. Un candado menos.", 3f);
    }

    private void AvisarPersonajeIncorrecto()
    {
        if (Time.time < proximoAviso)
            return;

        proximoAviso = Time.time + 2f;

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje("Solo " + personajeRequerido + " llega hasta la llave.", 2.5f);
    }
}
