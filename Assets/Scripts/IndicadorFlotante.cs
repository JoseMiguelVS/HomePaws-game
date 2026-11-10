using UnityEngine;

/// <summary>
/// Cartel flotante que indica que personaje necesita un objeto del puzle
/// (por ejemplo "SOL - EMPUJA" o "ESTRELLA - SALTO"). Se dibuja con OnGUI,
/// asi no hace falta crear sprites ni tocar el Canvas.
///
/// Colocarlo en un objeto vacio sobre el mecanismo. El texto se puede cambiar
/// desde otro script con EstablecerTexto (lo hace Compuerta con sus candados).
/// </summary>
public class IndicadorFlotante : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] private string texto = "SOL - EMPUJA";
    [SerializeField] private Color color = new Color(1f, 0.68f, 0.22f, 1f);

    [Header("Posicion")]
    [Tooltip("Altura sobre el objeto donde se dibuja el cartel.")]
    [SerializeField] private float altura = 1.4f;

    [Tooltip("Movimiento suave de flotacion.")]
    [SerializeField] private float amplitudFlotacion = 0.07f;
    [SerializeField] private float velocidadFlotacion = 2.2f;

    [Header("Tamano")]
    [SerializeField] private float ancho = 210f;
    [SerializeField] private float alto = 36f;
    [SerializeField] private int tamanoTexto = 18;

    [Header("Visibilidad")]
    [Tooltip("Solo se dibuja si el jugador activo esta a esta distancia o menos.")]
    [SerializeField] private float distanciaMaxima = 18f;

    [SerializeField] private bool visible = true;

    private string textoActual;
    private bool mostrando = true;

    private void Awake()
    {
        textoActual = texto;
    }

    /// <summary>Cambia el texto del cartel (por ejemplo, el estado de la compuerta).</summary>
    public void EstablecerTexto(string nuevoTexto)
    {
        textoActual = nuevoTexto;
    }

    public void EstablecerColor(Color nuevoColor)
    {
        color = nuevoColor;
    }

    public void Mostrar(bool mostrar)
    {
        mostrando = mostrar;
    }

    private void OnGUI()
    {
        if (!visible || !mostrando)
            return;

        // Con el juego pausado manda el menu de pausa.
        if (Time.timeScale <= 0f)
            return;

        if (DialogoUI.Instancia != null && DialogoUI.Instancia.Activo)
            return;

        Camera camara = Camera.main;
        if (camara == null)
            return;

        ControlDuo duo = ControlDuo.Instancia;
        if (duo != null && duo.Activo != null)
        {
            float distancia = Vector2.Distance(duo.Activo.transform.position, transform.position);
            if (distancia > distanciaMaxima)
                return;
        }

        float flotacion = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        Vector3 mundo = transform.position + Vector3.up * (altura + flotacion);
        Vector3 pantalla = camara.WorldToScreenPoint(mundo);

        if (pantalla.z <= 0f)
            return;

        Vector2 punto = InterfazJuego.APantallaLogica(pantalla);

        if (punto.x < -ancho || punto.x > InterfazJuego.Ancho + ancho ||
            punto.y < -alto || punto.y > InterfazJuego.Alto + alto)
            return;

        InterfazJuego.Empezar();

        Rect cartel = new Rect(
            punto.x - ancho * 0.5f,
            punto.y - alto * 0.5f,
            ancho,
            alto
        );

        Color fondo = new Color(0.045f, 0.05f, 0.075f, 0.88f);
        Color borde = new Color(color.r, color.g, color.b, 0.95f);

        InterfazJuego.Panel(new Rect(cartel.x + 3f, cartel.y + 4f, cartel.width, cartel.height), new Color(0f, 0f, 0f, 0.30f));
        InterfazJuego.PanelConBorde(cartel, fondo, borde, 2f);

        // Franja de color a la izquierda y linea que apunta al objeto.
        InterfazJuego.Barra(new Rect(cartel.x + 2f, cartel.y + 2f, 6f, cartel.height - 4f), color);
        InterfazJuego.Panel(
            new Rect(cartel.x + cartel.width * 0.5f - 1f, cartel.y + cartel.height, 2f, 16f),
            new Color(color.r, color.g, color.b, 0.7f)
        );

        InterfazJuego.Etiqueta(
            new Rect(cartel.x + 12f, cartel.y, cartel.width - 18f, cartel.height),
            textoActual,
            InterfazJuego.Estilo(tamanoTexto, color, true, TextAnchor.MiddleCenter)
        );

        InterfazJuego.Terminar();
    }
}
