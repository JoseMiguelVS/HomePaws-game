using UnityEngine;

/// <summary>
/// Compuerta con varios candados. Cada candado se resuelve con un mecanismo
/// distinto (placa de presion, palanca y llave) y la compuerta solo se abre
/// cuando todos estan resueltos.
///
/// No necesita eventos: consulta el estado de los mecanismos cada fotograma,
/// asi que se puede montar entera desde el Inspector.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Compuerta : MonoBehaviour
{
    [Header("Candados")]
    [SerializeField] private bool requierePlaca = true;
    [Tooltip("Placa que debe estar pulsada (la caja pesada que empuja Sol).")]
    [SerializeField] private PlacaPresion placa;

    [SerializeField] private bool requierePalanca = true;
    [Tooltip("Palanca que debe accionar Sol.")]
    [SerializeField] private Palanca palanca;

    [SerializeField] private bool requiereLlave = true;
    [Tooltip("Llave que debe recoger Estrella.")]
    [SerializeField] private Llave llave;

    [Header("Aspecto")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteCerrada;
    [SerializeField] private Sprite spriteAbierta;

    [Tooltip("Collider que bloquea el paso. Se desactiva al abrir.")]
    [SerializeField] private Collider2D bloqueo;

    [Tooltip("Cartel flotante que muestra los candados abiertos. Opcional.")]
    [SerializeField] private IndicadorFlotante indicador;

    [Header("Comportamiento")]
    [Tooltip("Si se marca, una vez abierta ya no se vuelve a cerrar.")]
    [SerializeField] private bool quedaAbierta = true;

    [Header("Efectos")]
    [SerializeField] private GameObject efectoAbrir;

    public bool Abierta { get; private set; }
    public int CandadosTotales { get; private set; }
    public int CandadosAbiertos { get; private set; }

    public string TextoEstado
    {
        get
        {
            if (Abierta)
                return "COMPUERTA ABIERTA";

            return "COMPUERTA " + CandadosAbiertos + "/" + CandadosTotales;
        }
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (bloqueo == null)
            bloqueo = GetComponent<Collider2D>();

        CandadosTotales = 0;

        if (requierePlaca)
            CandadosTotales++;

        if (requierePalanca)
            CandadosTotales++;

        if (requiereLlave)
            CandadosTotales++;
    }

    private void Start()
    {
        ActualizarEstado(true);
    }

    private void Update()
    {
        ActualizarEstado(false);
    }

    private void ActualizarEstado(bool forzar)
    {
        bool placaResuelta = !requierePlaca || (placa != null && placa.EstaPulsada);
        bool palancaResuelta = !requierePalanca || (palanca != null && palanca.Activada);
        bool llaveRecogida = !requiereLlave || (llave != null && llave.Recogida);

        int abiertos = 0;

        if (placaResuelta)
            abiertos++;

        if (palancaResuelta)
            abiertos++;

        if (llaveRecogida)
            abiertos++;

        bool completa = abiertos >= CandadosTotales;
        bool cambio = abiertos != CandadosAbiertos || completa != Abierta;

        CandadosAbiertos = abiertos;

        if (quedaAbierta && Abierta)
        {
            if (forzar)
                AplicarEstado();

            return;
        }

        if (!cambio && !forzar)
            return;

        Abierta = completa;
        AplicarEstado();

        if (forzar || !cambio)
            return;

        if (Abierta)
        {
            GestorAudio.Sonar(Sonido.Puerta, 0.9f);

            if (efectoAbrir != null)
                efectoAbrir.SetActive(true);

            if (HUD_Duo.Instancia != null)
                HUD_Duo.Mensaje("La compuerta se abri\u00f3. \u00a1Adelante!", 3.5f);
        }
        else
        {
            GestorAudio.Sonar(Sonido.Boton, 0.5f);
        }
    }

    private void AplicarEstado()
    {
        if (spriteRenderer != null && spriteCerrada != null && spriteAbierta != null)
            spriteRenderer.sprite = Abierta ? spriteAbierta : spriteCerrada;

        if (bloqueo != null)
            bloqueo.enabled = !Abierta;

        if (indicador != null)
        {
            indicador.EstablecerTexto(TextoEstado);
            indicador.EstablecerColor(
                Abierta
                    ? new Color(0.4f, 0.95f, 0.5f, 1f)
                    : new Color(1f, 0.55f, 0.25f, 1f)
            );
        }
    }

    // Si el jugador vuelve a empezar el nivel, la compuerta se puede reiniciar
    // desde un ReiniciadorPuzzle o desde otro script.
    public void Reiniciar()
    {
        Abierta = false;
        CandadosAbiertos = 0;
        AplicarEstado();
    }
}
