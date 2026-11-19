using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cinematica final: Sol y Estrella caminan por el barrio hasta su casa,
/// la puerta se abre y el juego vuelve al menu principal.
///
/// Todo el texto se dibuja con OnGUI (igual que el HUD) y el fundido usa
/// FundidoUI, asi la escena no necesita Canvas.
/// </summary>
[DisallowMultipleComponent]
public class CinematicaFinal : MonoBehaviour
{
    [Header("Protagonistas")]
    [SerializeField] private Transform sol;
    [SerializeField] private Transform estrella;
    [SerializeField] private SpriteRenderer solRenderer;
    [SerializeField] private SpriteRenderer estrellaRenderer;
    [SerializeField] private Sprite[] solFrames;
    [SerializeField] private Sprite[] estrellaFrames;

    [Header("Casa")]
    [SerializeField] private SpriteRenderer puerta;
    [SerializeField] private Sprite puertaCerrada;
    [SerializeField] private Sprite puertaAbierta;

    [Header("Recorrido")]
    [Tooltip("Donde empieza Sol, en unidades de mundo.")]
    [SerializeField] private float xInicio = 1.4f;

    [Tooltip("Donde se detiene Sol delante de la puerta.")]
    [SerializeField] private float xDestino = 9.4f;

    [Tooltip("Altura (centro del sprite) a la que caminan los gatos.")]
    [SerializeField] private float ySuelo = 1.9f;

    [Tooltip("Separacion entre los dos gatos.")]
    [SerializeField] private float separacion = 1.15f;

    [SerializeField] private float velocidad = 1.15f;
    [SerializeField] private float fpsAnimacion = 8f;
    [SerializeField] private bool mirandoDerecha = true;

    [Header("Textos")]
    [SerializeField] private string textoInicio = "Por fin... el olor a hogar.";
    [SerializeField] private string textoFinal = "Sol y Estrella han vuelto a casa.";
    [SerializeField] private float esperaAntesDeSalir = 2.6f;

    [Header("Datos ambientales")]
    [Tooltip("Muestra el panel con datos reales del problema ambiental al terminar la cinematica.")]
    [SerializeField] private bool mostrarDatosAmbientales = true;

    [SerializeField] private string tituloDatos = "EL PROBLEMA QUE NO SE VE";

    [SerializeField] private string[] datosAmbientales =
    {
        "M\u00e9xico genera alrededor de 42 millones de toneladas de residuos s\u00f3lidos urbanos al a\u00f1o (SEMARNAT, 2020).",
        "Solo se recicla cerca del 10% de esos residuos: el resto termina en rellenos, calles y drenajes.",
        "Se estima que 23 millones de perros y gatos viven en situaci\u00f3n de calle en M\u00e9xico (AMMVEPE)."
    };

    [SerializeField] private string fuentesDatos =
        "Fuentes: SEMARNAT, Diagn\u00f3stico B\u00e1sico para la Gesti\u00f3n Integral de los Residuos (2020); AMMVEPE.";

    [Tooltip("Tiempo minimo que se queda el panel de datos en pantalla. Se puede saltar con ESPACIO.")]
    [SerializeField] private float esperaConDatos = 8f;

    [Header("Escena")]
    [SerializeField] private string siguienteEscena = "MenuPrincipal";

    private float posicionSol;
    private bool llegaron;
    private bool saliendo;
    private bool saltar;
    private float tiempoLlegada;
    private float tiempoFrame;
    private int frame;

    private void Awake()
    {
        if (FundidoUI.Instancia == null)
            gameObject.AddComponent<FundidoUI>();
    }

    private void Start()
    {
        posicionSol = xInicio;

        if (puerta != null && puertaCerrada != null)
            puerta.sprite = puertaCerrada;

        AplicarPosiciones();
        StartCoroutine(Guion());
    }

    private void Update()
    {
        if (llegaron && !saliendo && Input.GetKeyDown(KeyCode.Space))
            saltar = true;

        if (saliendo || llegaron)
            return;

        posicionSol = Mathf.MoveTowards(posicionSol, xDestino, velocidad * Time.deltaTime);
        AplicarPosiciones();
        Animar(Time.deltaTime);

        if (posicionSol >= xDestino - 0.001f)
            Llegada();
    }

    private void AplicarPosiciones()
    {
        float signo = mirandoDerecha ? 1f : -1f;

        if (sol != null)
        {
            sol.position = new Vector3(posicionSol, ySuelo, 0f);
            Vector3 escala = sol.localScale;
            escala.x = Mathf.Abs(escala.x) * signo;
            sol.localScale = escala;
        }

        if (estrella != null)
        {
            estrella.position = new Vector3(posicionSol + separacion, ySuelo, 0f);
            Vector3 escala = estrella.localScale;
            escala.x = Mathf.Abs(escala.x) * signo;
            estrella.localScale = escala;
        }
    }

    private void Animar(float delta)
    {
        if (solFrames == null || solFrames.Length == 0)
            return;

        tiempoFrame += delta;

        if (tiempoFrame < 1f / Mathf.Max(1f, fpsAnimacion))
            return;

        tiempoFrame = 0f;
        frame++;

        if (solRenderer != null)
            solRenderer.sprite = solFrames[frame % solFrames.Length];

        if (estrellaRenderer != null && estrellaFrames != null && estrellaFrames.Length > 0)
            estrellaRenderer.sprite = estrellaFrames[frame % estrellaFrames.Length];
    }

    private void Llegada()
    {
        llegaron = true;
        tiempoLlegada = Time.timeSinceLevelLoad;

        if (puerta != null && puertaAbierta != null)
            puerta.sprite = puertaAbierta;

        if (solRenderer != null && solFrames != null && solFrames.Length > 0)
            solRenderer.sprite = solFrames[0];

        if (estrellaRenderer != null && estrellaFrames != null && estrellaFrames.Length > 0)
            estrellaRenderer.sprite = estrellaFrames[0];

        GestorAudio.Sonar(Sonido.Puerta, 0.9f);
    }

    private IEnumerator Guion()
    {
        FundidoUI fundido = FundidoUI.Instancia;

        if (fundido != null)
            yield return fundido.Aparecer(1.1f);

        while (!llegaron)
            yield return null;

        float espera = 0f;

        float tiempoMinimo = mostrarDatosAmbientales
            ? Mathf.Max(esperaAntesDeSalir, esperaConDatos)
            : esperaAntesDeSalir;

        while (espera < tiempoMinimo && !saltar)
        {
            espera += Time.unscaledDeltaTime;
            yield return null;
        }

        saliendo = true;

        if (fundido != null)
            yield return fundido.Desaparecer(1f);

        if (!string.IsNullOrEmpty(siguienteEscena))
            SceneManager.LoadScene(siguienteEscena);
    }

    private void OnGUI()
    {
        InterfazJuego.Empezar();

        Color color = new Color(1f, 0.96f, 0.88f, 1f);

        if (!llegaron)
        {
            if (Time.timeSinceLevelLoad > 0.6f)
            {
                InterfazJuego.EtiquetaConSombra(
                    new Rect(0f, InterfazJuego.Alto - 150f, InterfazJuego.Ancho, 60f),
                    textoInicio,
                    InterfazJuego.Estilo(30, color, true, TextAnchor.MiddleCenter),
                    3f
                );
            }
        }
        else
        {
            float alpha = Mathf.Clamp01((Time.timeSinceLevelLoad - tiempoLlegada) / 0.8f);

            InterfazJuego.EtiquetaConSombra(
                new Rect(0f, InterfazJuego.Alto * 0.18f, InterfazJuego.Ancho, 80f),
                textoFinal,
                InterfazJuego.Estilo(52, new Color(color.r, color.g, color.b, alpha), true, TextAnchor.MiddleCenter),
                4f
            );

            if (mostrarDatosAmbientales)
                DibujarDatos(alpha);

            InterfazJuego.Etiqueta(
                new Rect(0f, InterfazJuego.Alto - 70f, InterfazJuego.Ancho, 40f),
                "ESPACIO para continuar",
                InterfazJuego.Estilo(20, new Color(0.9f, 0.9f, 0.95f, alpha * 0.75f), false, TextAnchor.MiddleCenter)
            );
        }

        InterfazJuego.Terminar();
    }

    /// <summary>
    /// Panel con datos reales del problema ambiental. Aparece despues de la
    /// llegada a casa y se puede saltar con ESPACIO como el resto de la cinematica.
    /// </summary>
    private void DibujarDatos(float alpha)
    {
        if (datosAmbientales == null || datosAmbientales.Length == 0)
            return;

        const float ancho = 1360f;
        const float alto = 440f;

        float x = (InterfazJuego.Ancho - ancho) * 0.5f;
        float y = 300f;

        Color verde = new Color(0.45f, 0.9f, 0.55f, alpha);
        Color fondo = new Color(0.04f, 0.07f, 0.08f, 0.86f * alpha);
        Color texto = new Color(0.96f, 0.95f, 0.9f, alpha);

        InterfazJuego.PanelConBorde(
            new Rect(x, y, ancho, alto),
            fondo,
            new Color(0.35f, 0.8f, 0.45f, 0.9f * alpha),
            3f
        );

        InterfazJuego.Etiqueta(
            new Rect(x + 40f, y + 22f, ancho - 80f, 44f),
            tituloDatos,
            InterfazJuego.Estilo(32, verde, true, TextAnchor.MiddleLeft)
        );

        float lineaY = y + 80f;

        for (int i = 0; i < datosAmbientales.Length; i++)
        {
            string linea = datosAmbientales[i];

            if (string.IsNullOrEmpty(linea))
                continue;

            InterfazJuego.Etiqueta(
                new Rect(x + 40f, lineaY, ancho - 80f, 64f),
                "\u2022 " + linea,
                InterfazJuego.Estilo(23, texto, false, TextAnchor.UpperLeft)
            );

            lineaY += 74f;
        }

        if (!string.IsNullOrEmpty(fuentesDatos))
        {
            InterfazJuego.Etiqueta(
                new Rect(x + 40f, y + alto - 52f, ancho - 80f, 40f),
                fuentesDatos,
                InterfazJuego.Estilo(15, new Color(0.72f, 0.8f, 0.76f, alpha), false, TextAnchor.MiddleLeft)
            );
        }
    }
}
