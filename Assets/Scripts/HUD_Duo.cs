using UnityEngine;

/// <summary>
/// HUD del duo dibujado con OnGUI (sin Canvas): personaje activo, habilidad,
/// objetivo, atun, avisos, boton de pausa y el aviso de tecla E junto a la
/// palanca.
///
/// Colocarlo en un objeto vacio "HUD_Duo" y arrastrar ControlDuo, Compuerta,
/// Palanca y MenuPausa en el Inspector.
/// </summary>
[DisallowMultipleComponent]
public class HUD_Duo : MonoBehaviour
{
    public static HUD_Duo Instancia { get; private set; }

    [Header("Referencias")]
    [SerializeField] private ControlDuo controlDuo;
    [SerializeField] private Compuerta compuerta;
    [SerializeField] private Palanca palanca;
    [SerializeField] private MenuPausa menuPausa;

    [Header("Niveles de un solo personaje")]
    [Tooltip("Objetivo que muestra el HUD cuando la escena no tiene ControlDuo.")]
    [SerializeField] private string objetivoSinDuo = "Recoge los atunes y llega a la salida.";

    [Tooltip("Habilidad que muestra el HUD cuando la escena no tiene ControlDuo.")]
    [SerializeField] private string habilidadSinDuo = "Empuja objetos pesados";

    [Header("Colores")]
    [SerializeField] private Color colorSol = new Color(1f, 0.68f, 0.22f, 1f);
    [SerializeField] private Color colorEstrella = new Color(0.4f, 0.85f, 1f, 1f);
    [SerializeField] private Color colorTexto = new Color(0.96f, 0.94f, 0.88f, 1f);
    [SerializeField] private Color colorTextoSuave = new Color(0.68f, 0.7f, 0.76f, 1f);
    [SerializeField] private Color colorFondo = new Color(0.045f, 0.05f, 0.075f, 0.86f);

    private string mensaje;
    private float mensajeHasta;
    private Color colorMensaje = new Color(0.96f, 0.94f, 0.88f, 1f);

    private void Awake()
    {
        Instancia = this;

        if (controlDuo == null)
            controlDuo = ControlDuo.Instancia;
    }

    /// <summary>Muestra un aviso temporal en la parte superior de la pantalla.</summary>
    public static void Mensaje(string texto, float duracion = 3f)
    {
        Mensaje(texto, duracion, new Color(0.96f, 0.94f, 0.88f, 1f));
    }

    public static void Mensaje(string texto, float duracion, Color color)
    {
        if (Instancia == null || string.IsNullOrEmpty(texto))
            return;

        Instancia.mensaje = texto;
        Instancia.mensajeHasta = Time.unscaledTime + Mathf.Max(1f, duracion);
        Instancia.colorMensaje = color;
    }

    private void OnGUI()
    {
        // Con el juego pausado manda el menu de pausa.
        if (Time.timeScale <= 0f)
            return;

        InterfazJuego.Empezar();

        DibujarPersonaje();
        DibujarObjetivo();
        DibujarAtun();
        DibujarBotonPausa();
        DibujarAyuda();
        DibujarPrompt();
        DibujarMensaje();

        InterfazJuego.Terminar();
    }

    private void DibujarPersonaje()
    {
        string nombre = "SOL";
        string companero = "";
        string habilidad;
        Color color = colorSol;

        if (controlDuo == null)
        {
            // Nivel de un solo personaje (por ejemplo PrimerEscena).
            habilidad = habilidadSinDuo;
        }
        else
        {
            habilidad = "Empuja cajas pesadas y mueve palancas";

            if (controlDuo.NombreActivo == "Estrella")
            {
                nombre = "ESTRELLA";
                color = colorEstrella;
                habilidad = "Salto alto: alcanza la llave";
            }

            if (controlDuo.DuoActivo)
                companero = "Compa\u00f1ero: " + controlDuo.NombreInactivo;
            else
                habilidad = "Busca a Estrella para formar el duo";
        }

        Rect panel = new Rect(26f, 26f, 360f, 132f);
        InterfazJuego.Panel(new Rect(panel.x + 4f, panel.y + 5f, panel.width, panel.height), new Color(0f, 0f, 0f, 0.3f));
        InterfazJuego.PanelConBorde(panel, colorFondo, new Color(color.r, color.g, color.b, 0.85f), 2f);
        InterfazJuego.Barra(new Rect(panel.x + 2f, panel.y + 2f, 6f, panel.height - 4f), color);

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 22f, panel.y + 10f, panel.width - 32f, 42f),
            nombre,
            InterfazJuego.Estilo(32, color, true)
        );

        string teclas = companero.Length > 0 ? "TAB cambiar  ·  1 / 2" : "";
        InterfazJuego.Etiqueta(
            new Rect(panel.x + 22f, panel.y + 52f, panel.width - 32f, 24f),
            teclas,
            InterfazJuego.Estilo(17, colorTextoSuave)
        );

        if (companero.Length > 0)
        {
            InterfazJuego.Etiqueta(
                new Rect(panel.x + 22f, panel.y + 74f, panel.width - 32f, 22f),
                companero,
                InterfazJuego.Estilo(17, colorTextoSuave)
            );
        }

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 22f, panel.y + 98f, panel.width - 32f, 26f),
            habilidad,
            InterfazJuego.Estilo(18, colorTexto)
        );
    }

    private void DibujarObjetivo()
    {
        Rect panel = new Rect(26f, 176f, 360f, 104f);
        InterfazJuego.PanelConBorde(panel, colorFondo, new Color(0.35f, 0.38f, 0.45f, 0.8f), 2f);

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 18f, panel.y + 8f, panel.width - 30f, 24f),
            "OBJETIVO",
            InterfazJuego.Estilo(16, colorTextoSuave, true)
        );

        string objetivo;

        if (controlDuo != null && !controlDuo.DuoActivo)
        {
            objetivo = "Encuentra a Estrella en el alcantarillado.";
        }
        else if (compuerta != null && compuerta.Abierta == false)
        {
            objetivo = "Abre la compuerta: resuelve los 3 candados.";
        }
        else if (compuerta != null)
        {
            objetivo = "Compuerta abierta. Sigue hacia la salida.";
        }
        else
        {
            objetivo = objetivoSinDuo;
        }

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 18f, panel.y + 34f, panel.width - 30f, 46f),
            objetivo,
            InterfazJuego.Estilo(19, colorTexto)
        );

        if (compuerta != null)
        {
            InterfazJuego.Etiqueta(
                new Rect(panel.x + 18f, panel.y + 78f, panel.width - 30f, 22f),
                compuerta.TextoEstado,
                InterfazJuego.Estilo(16, new Color(1f, 0.72f, 0.35f, 1f), true)
            );
        }
    }

    private void DibujarAtun()
    {
        Rect panel = new Rect(InterfazJuego.Ancho - 26f - 224f, 110f, 224f, 56f);
        InterfazJuego.PanelConBorde(panel, colorFondo, new Color(1f, 0.68f, 0.22f, 0.7f), 2f);

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 16f, panel.y, 90f, panel.height),
            "ATUN",
            InterfazJuego.Estilo(18, colorTextoSuave, true, TextAnchor.MiddleLeft)
        );

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 100f, panel.y, panel.width - 116f, panel.height),
            PlayerController.AtunTotal.ToString(),
            InterfazJuego.Estilo(28, colorSol, true, TextAnchor.MiddleRight)
        );
    }

    private void DibujarBotonPausa()
    {
        Rect boton = new Rect(InterfazJuego.Ancho - 26f - 66f, 26f, 66f, 66f);

        bool encima = boton.Contains(Event.current.mousePosition);
        Color borde = encima
            ? new Color(1f, 0.68f, 0.22f, 1f)
            : new Color(0.65f, 0.68f, 0.75f, 0.85f);

        InterfazJuego.PanelConBorde(boton, colorFondo, borde, 2f);

        InterfazJuego.Etiqueta(
            boton,
            "II",
            InterfazJuego.Estilo(30, colorTexto, true, TextAnchor.MiddleCenter)
        );

        // La etiqueta no captura clics: el boton invisible si.
        if (GUI.Button(boton, GUIContent.none, GUIStyle.none))
        {
            if (menuPausa != null)
                menuPausa.PausarJuego();
        }
    }

    private void DibujarAyuda()
    {
        if (Time.timeSinceLevelLoad > 16f)
            return;

        float restante = 16f - Time.timeSinceLevelLoad;
        float alpha = Mathf.Clamp01(restante / 3f) * 0.9f;

        InterfazJuego.Etiqueta(
            new Rect(28f, InterfazJuego.Alto - 44f, 1100f, 30f),
            controlDuo != null && controlDuo.DuoActivo
                ? "A / D mover   ·   Espacio saltar   ·   E interactuar   ·   TAB cambiar   ·   ESC pausa"
                : "A / D mover   ·   Espacio saltar   ·   E interactuar   ·   ESC pausa",
            InterfazJuego.Estilo(18, new Color(0.95f, 0.94f, 0.88f, alpha))
        );
    }

    private void DibujarPrompt()
    {
        if (palanca == null || palanca.Activada || !palanca.JugadorCerca)
            return;

        if (DialogoUI.Instancia != null && DialogoUI.Instancia.Activo)
            return;

        bool puede = controlDuo != null && controlDuo.NombreActivo == palanca.PersonajeRequerido;

        string texto = puede
            ? "E  ·  Accionar palanca"
            : "Solo " + palanca.PersonajeRequerido + " puede mover esta palanca. Pulsa TAB.";

        Color color = puede ? colorSol : new Color(1f, 0.55f, 0.4f, 1f);

        Rect panel = new Rect(InterfazJuego.Ancho * 0.5f - 330f, InterfazJuego.Alto - 158f, 660f, 56f);
        InterfazJuego.Panel(new Rect(panel.x + 4f, panel.y + 5f, panel.width, panel.height), new Color(0f, 0f, 0f, 0.3f));
        InterfazJuego.PanelConBorde(panel, colorFondo, new Color(color.r, color.g, color.b, 0.9f), 2f);

        InterfazJuego.Etiqueta(
            panel,
            texto,
            InterfazJuego.Estilo(20, color, true, TextAnchor.MiddleCenter)
        );
    }

    private void DibujarMensaje()
    {
        if (string.IsNullOrEmpty(mensaje) || Time.unscaledTime > mensajeHasta)
            return;

        float restante = mensajeHasta - Time.unscaledTime;
        float alpha = Mathf.Clamp01(restante / 0.5f);

        Rect panel = new Rect(InterfazJuego.Ancho * 0.5f - 430f, 118f, 860f, 58f);
        InterfazJuego.Panel(
            new Rect(panel.x + 4f, panel.y + 5f, panel.width, panel.height),
            new Color(0f, 0f, 0f, 0.3f * alpha)
        );
        InterfazJuego.PanelConBorde(
            panel,
            new Color(colorFondo.r, colorFondo.g, colorFondo.b, colorFondo.a * alpha),
            new Color(colorMensaje.r, colorMensaje.g, colorMensaje.b, alpha),
            2f
        );

        InterfazJuego.Etiqueta(
            panel,
            mensaje,
            InterfazJuego.Estilo(21, new Color(colorMensaje.r, colorMensaje.g, colorMensaje.b, alpha), true, TextAnchor.MiddleCenter)
        );
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
