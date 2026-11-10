using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caja de dialogo dibujada con OnGUI, sin tocar el Canvas de la escena.
/// Se usa desde EncuentroEstrella (o desde cualquier otro script):
///
///   DialogoUI.Instancia.Mostrar(hablantes, textos, AlTerminar);
///
/// Avanza con E, Espacio, Enter o clic. Mientras esta activa, ControlDuo
/// bloquea el movimiento de los personajes.
/// </summary>
[DisallowMultipleComponent]
public class DialogoUI : MonoBehaviour
{
    public static DialogoUI Instancia { get; private set; }

    [Header("Aspecto")]
    [SerializeField] private Color colorSol = new Color(1f, 0.68f, 0.22f, 1f);
    [SerializeField] private Color colorEstrella = new Color(0.4f, 0.85f, 1f, 1f);
    [SerializeField] private Color colorFondo = new Color(0.04f, 0.05f, 0.08f, 0.94f);
    [SerializeField] private float velocidadTexto = 55f;

    [Header("Sonido")]
    [SerializeField] private float volumenAvance = 0.35f;

    private readonly List<string> hablantes = new List<string>();
    private readonly List<string> textos = new List<string>();

    private int indice;
    private int caracteresVisibles;
    private int totalCaracteres;
    private Action alTerminar;

    public bool Activo { get; private set; }

    private void Awake()
    {
        Instancia = this;
    }

    private void Update()
    {
        if (!Activo)
            return;

        if (Time.timeScale <= 0f)
            return;

        // Efecto de maquina de escribir.
        if (caracteresVisibles < totalCaracteres)
        {
            caracteresVisibles += Mathf.CeilToInt(velocidadTexto * Time.unscaledDeltaTime);
            if (caracteresVisibles > totalCaracteres)
                caracteresVisibles = totalCaracteres;
        }

        if (AvanzarPulsado())
            Siguiente();
    }

    /// <summary>
    /// Muestra una conversacion. Los dos arrays deben tener el mismo tamano.
    /// Al terminar se llama a alFinal (opcional).
    /// </summary>
    public void Mostrar(string[] nuevosHablantes, string[] nuevosTextos, Action alFinal = null)
    {
        hablantes.Clear();
        textos.Clear();

        if (nuevosTextos != null)
        {
            for (int i = 0; i < nuevosTextos.Length; i++)
            {
                textos.Add(nuevosTextos[i]);

                string hablante = "";
                if (nuevosHablantes != null && i < nuevosHablantes.Length)
                    hablante = nuevosHablantes[i];

                hablantes.Add(hablante);
            }
        }

        alTerminar = alFinal;
        indice = 0;
        Activo = textos.Count > 0;

        PrepararLinea();

        if (!Activo && alTerminar != null)
            Terminar();
    }

    /// <summary>Pasa a la siguiente linea (o completa la que se esta escribiendo).</summary>
    public void Siguiente()
    {
        if (!Activo)
            return;

        if (caracteresVisibles < totalCaracteres)
        {
            caracteresVisibles = totalCaracteres;
            return;
        }

        GestorAudio.Sonar(Sonido.Boton, volumenAvance);

        indice++;

        if (indice >= textos.Count)
        {
            Terminar();
            return;
        }

        PrepararLinea();
    }

    private void PrepararLinea()
    {
        caracteresVisibles = 0;
        totalCaracteres = textos.Count > 0 ? textos[Mathf.Clamp(indice, 0, textos.Count - 1)].Length : 0;
    }

    private void Terminar()
    {
        Activo = false;
        hablantes.Clear();
        textos.Clear();

        Action callback = alTerminar;
        alTerminar = null;

        if (callback != null)
            callback();
    }

    private static bool AvanzarPulsado()
    {
        return Input.GetKeyDown(KeyCode.E)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetMouseButtonDown(0);
    }

    private void OnGUI()
    {
        if (!Activo || textos.Count == 0)
            return;

        if (Time.timeScale <= 0f)
            return;

        InterfazJuego.Empezar();

        int linea = Mathf.Clamp(indice, 0, textos.Count - 1);
        string hablante = hablantes.Count > linea ? hablantes[linea] : "";
        string texto = textos[linea];
        Color colorPersonaje = ColorPersonaje(hablante);

        Rect panel = new Rect(220f, 740f, 1480f, 270f);

        // Sombra exterior y panel principal.
        InterfazJuego.Panel(new Rect(panel.x + 6f, panel.y + 8f, panel.width, panel.height), new Color(0f, 0f, 0f, 0.35f));
        InterfazJuego.PanelConBorde(panel, colorFondo, new Color(colorPersonaje.r, colorPersonaje.g, colorPersonaje.b, 0.9f), 3f);

        // Franja de color con la inicial del personaje.
        Rect retrato = new Rect(panel.x + 26f, panel.y + 26f, 92f, 92f);
        InterfazJuego.PanelConBorde(retrato, new Color(colorPersonaje.r * 0.25f, colorPersonaje.g * 0.25f, colorPersonaje.b * 0.25f, 1f), colorPersonaje, 2f);
        InterfazJuego.Etiqueta(
            retrato,
            Inicial(hablante),
            InterfazJuego.Estilo(46, colorPersonaje, true, TextAnchor.MiddleCenter)
        );

        GUIStyle estiloNombre = InterfazJuego.Estilo(30, colorPersonaje, true);
        GUIStyle estiloTexto = InterfazJuego.Estilo(29, new Color(0.96f, 0.94f, 0.88f, 1f));
        GUIStyle estiloPista = InterfazJuego.Estilo(20, new Color(0.7f, 0.72f, 0.78f, 1f), false, TextAnchor.LowerRight);

        InterfazJuego.Etiqueta(new Rect(panel.x + 140f, panel.y + 24f, panel.width - 180f, 42f), hablante.ToUpperInvariant(), estiloNombre);

        string visible = texto.Substring(0, Mathf.Clamp(caracteresVisibles, 0, texto.Length));

        InterfazJuego.Etiqueta(
            new Rect(panel.x + 140f, panel.y + 74f, panel.width - 190f, 130f),
            visible,
            estiloTexto
        );

        string pista = caracteresVisibles < totalCaracteres ? "E / Espacio para completar" : "E / Espacio para continuar  >";
        InterfazJuego.Etiqueta(
            new Rect(panel.x + 140f, panel.y + panel.height - 52f, panel.width - 180f, 30f),
            pista,
            estiloPista
        );

        InterfazJuego.Terminar();
    }

    private Color ColorPersonaje(string hablante)
    {
        if (!string.IsNullOrEmpty(hablante) && hablante.ToLowerInvariant().Contains("estrella"))
            return colorEstrella;

        return colorSol;
    }

    private static string Inicial(string hablante)
    {
        if (string.IsNullOrEmpty(hablante))
            return "?";

        return hablante.Substring(0, 1).ToUpperInvariant();
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
