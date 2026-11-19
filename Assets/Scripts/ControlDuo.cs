using UnityEngine;

/// <summary>
/// Controla el duo Sol + Estrella. Solo un personaje recibe las entradas en
/// cada momento (TAB o 1/2 para cambiar); el otro se queda quieto en Idle.
/// Tambien mueve la camara, bloquea el control durante los dialogos y avisa
/// al GestorReaparicion de quien es el jugador activo.
///
/// Colocarlo en un objeto vacio de la escena (por ejemplo "ControlDuo") y
/// arrastrar los dos PlayerController en el Inspector.
/// </summary>
[DisallowMultipleComponent]
public class ControlDuo : MonoBehaviour
{
    public static ControlDuo Instancia { get; private set; }

    [Header("Personajes")]
    [SerializeField] private PlayerController sol;
    [SerializeField] private PlayerController estrella;

    [Header("Camara")]
    [Tooltip("CameraFollow de la escena. Se le cambia el objetivo al cambiar de personaje.")]
    [SerializeField] private CameraFollow camara;

    [Header("Teclas")]
    [SerializeField] private KeyCode teclaCambiar = KeyCode.Tab;
    [SerializeField] private KeyCode teclaSol = KeyCode.Alpha1;
    [SerializeField] private KeyCode teclaEstrella = KeyCode.Alpha2;

    [Header("Nivel 3")]
    [Tooltip("Empezar la escena con Estrella ya en el equipo (sin el encuentro).")]
    [SerializeField] private bool empezarConDuo = false;

    [Tooltip("Fuera de los puzles el companero inactivo sigue al personaje activo.")]
    [SerializeField] private bool companeroSigue = false;

    /// <summary>True cuando Estrella ya se ha unido al equipo.</summary>
    public bool DuoActivo { get; private set; }

    /// <summary>True mientras el jugador esta dentro de una zona de puzle.</summary>
    public bool EnPuzzle { get; private set; }

    /// <summary>Nombre del puzle activo (lo pone ZonaPuzzle).</summary>
    public string NombrePuzzle { get; private set; }

    /// <summary>True mientras un dialogo o una cinematica bloquea el control.</summary>
    public bool ControlBloqueado { get; private set; }

    public PlayerController Activo { get; private set; }
    public PlayerController Inactivo { get; private set; }

    public PerfilPersonaje PerfilActivo
    {
        get { return Activo != null ? Activo.GetComponent<PerfilPersonaje>() : null; }
    }

    public string NombreActivo
    {
        get
        {
            if (PerfilActivo != null)
                return PerfilActivo.Nombre;

            return Activo != null ? Activo.name : "";
        }
    }

    public string NombreInactivo
    {
        get
        {
            if (Inactivo == null)
                return "";

            PerfilPersonaje perfil = Inactivo.GetComponent<PerfilPersonaje>();
            return perfil != null ? perfil.Nombre : Inactivo.name;
        }
    }

    private void Awake()
    {
        Instancia = this;

        // Nivel 3: el duo ya empieza junto, sin conversacion de encuentro.
        if (empezarConDuo)
        {
            DuoActivo = true;

            if (estrella != null)
                estrella.gameObject.SetActive(true);
        }

        if (sol != null)
            sol.EstablecerControl(false);

        if (estrella != null)
            estrella.EstablecerControl(false);

        Activo = sol;
        Inactivo = estrella;

        AplicarControl();
        SincronizarCamara();
    }

    private void Start()
    {
        SincronizarReaparicion();
    }

    private void Update()
    {
        // No cambiar de personaje con el juego pausado ni durante un dialogo.
        if (Time.timeScale <= 0f)
            return;

        if (DialogoUI.Instancia != null && DialogoUI.Instancia.Activo)
            return;

        if (ControlBloqueado || !DuoActivo)
            return;

        if (Input.GetKeyDown(teclaCambiar))
        {
            Alternar();
            return;
        }

        if (Input.GetKeyDown(teclaSol))
            CambiarA(0);
        else if (Input.GetKeyDown(teclaEstrella))
            CambiarA(1);
    }

    /// <summary>Marca que Estrella se ha unido y deja el control en el personaje activo.</summary>
    public void ActivarDuo()
    {
        DuoActivo = true;

        if (estrella != null)
        {
            estrella.gameObject.SetActive(true);
            estrella.EstablecerControl(false);
        }

        if (Activo == null)
            Activo = sol;

        if (Inactivo == null)
            Inactivo = estrella;

        AplicarControl();
        SincronizarCamara();
        SincronizarReaparicion();

        GestorAudio.Sonar(Sonido.Checkpoint, 0.9f);

        if (HUD_Duo.Instancia != null)
        {
            HUD_Duo.Mensaje(
                "Estrella se uni\u00f3 al equipo. Pulsa TAB para cambiar de personaje.",
                4.5f
            );
        }
    }

    /// <summary>Alterna entre Sol y Estrella.</summary>
    public void Alternar()
    {
        if (!DuoActivo || sol == null || estrella == null)
            return;

        if (Activo == sol)
            CambiarA(1);
        else
            CambiarA(0);
    }

    /// <summary>Pone el control en el personaje indicado (0 = Sol, 1 = Estrella).</summary>
    public void CambiarA(int indice)
    {
        if (!DuoActivo)
            return;

        PlayerController elegido = indice == 1 ? estrella : sol;
        if (elegido == null || elegido == Activo)
            return;

        Inactivo = Activo;
        Activo = elegido;

        AplicarControl();
        SincronizarCamara();
        SincronizarReaparicion();

        GestorAudio.Sonar(Sonido.Boton, 0.55f);
    }

    /// <summary>
    /// Bloquea o devuelve el control. Se usa en los dialogos y en las
    /// cinematicas para que el personaje no se mueva solo.
    /// </summary>
    public void BloquearControl(bool bloqueado)
    {
        ControlBloqueado = bloqueado;

        if (bloqueado)
        {
            if (sol != null)
                sol.EstablecerControl(false);

            if (estrella != null)
                estrella.EstablecerControl(false);

            ActualizarSeguimiento();
        }
        else
        {
            AplicarControl();
        }
    }

    /// <summary>
    /// Avisa de que el jugador activo ha entrado en un puzle: el companero
    /// deja de seguir y se queda esperando. Lo llama ZonaPuzzle.
    /// </summary>
    public void EntrarPuzzle(string nombre)
    {
        if (EnPuzzle && NombrePuzzle == nombre)
            return;

        EnPuzzle = true;
        NombrePuzzle = nombre;

        ActualizarSeguimiento();
    }

    /// <summary>
    /// Sale de la zona de puzle. Si el companero se quedo lejos, se reunen
    /// junto al jugador activo y se vuelve a activar el seguimiento.
    /// </summary>
    public void SalirPuzzle()
    {
        if (!EnPuzzle)
            return;

        EnPuzzle = false;
        NombrePuzzle = null;

        Reagrupar();
        ActualizarSeguimiento();
    }

    /// <summary>Coloca la camara encima del personaje activo sin suavizado.</summary>
    public void CentrarCamara()
    {
        if (camara == null || Activo == null)
            return;

        camara.target = Activo.transform;

        Vector3 posicion = camara.transform.position;
        posicion.x = Activo.transform.position.x;
        posicion.y = Activo.transform.position.y;
        camara.transform.position = posicion;
    }

    private void Reagrupar()
    {
        if (!DuoActivo || Activo == null || Inactivo == null)
            return;

        if (Vector2.Distance(Activo.transform.position, Inactivo.transform.position) <= 2.5f)
            return;

        float lado = Activo.transform.localScale.x >= 0f ? -1f : 1f;
        Vector3 destino = Activo.transform.position + Vector3.right * 1.2f * lado;

        Rigidbody2D cuerpo = Inactivo.GetComponent<Rigidbody2D>();

        if (cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            cuerpo.position = destino;
        }

        Inactivo.transform.position = destino;
    }

    private void AplicarControl()
    {
        if (sol != null)
            sol.EstablecerControl(!ControlBloqueado && Activo == sol);

        if (estrella != null)
            estrella.EstablecerControl(!ControlBloqueado && DuoActivo && Activo == estrella);

        ActualizarSeguimiento();
    }

    // Solo el companero inactivo sigue, y solo fuera de los puzles.
    private void ActualizarSeguimiento()
    {
        bool seguir = companeroSigue
            && DuoActivo
            && !EnPuzzle
            && !ControlBloqueado
            && Activo != null
            && Inactivo != null;

        CompaneroSeguir seguidorActivo = Activo != null ? Activo.GetComponent<CompaneroSeguir>() : null;
        CompaneroSeguir seguidorInactivo = Inactivo != null ? Inactivo.GetComponent<CompaneroSeguir>() : null;

        if (seguidorActivo != null)
            seguidorActivo.Detener();

        if (seguidorInactivo == null)
            return;

        if (seguir)
            seguidorInactivo.Seguir(Activo.transform);
        else
            seguidorInactivo.Detener();
    }

    private void SincronizarCamara()
    {
        if (camara != null && Activo != null)
            camara.target = Activo.transform;
    }

    private void SincronizarReaparicion()
    {
        if (GestorReaparicion.Instancia == null)
            return;

        GestorReaparicion.Instancia.ConfigurarJugadores(
            Activo != null ? Activo.transform : null,
            DuoActivo && Inactivo != null ? Inactivo.transform : null
        );
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
