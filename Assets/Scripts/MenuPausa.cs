using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona el menú del juego. Sirve como pantalla de bienvenida al
/// iniciar la partida y como menú de pausa mientras se juega.
/// </summary>
public class MenuPausa : MonoBehaviour
{
    [Header("Paneles de la interfaz")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject menuConfig;
    [SerializeField] private GameObject imagenFondo;

    [Tooltip("Botón de pausa de la esquina. Se oculta mientras el menú está abierto.")]
    [SerializeField] private GameObject botonPausa;

    [Header("Ayuda en pantalla")]
    [Tooltip("Texto de ayuda de los controles. Se oculta durante el menú y cuando el jugador se mueve o salta.")]
    [SerializeField] private GameObject ayudaControles;

    [Header("Configuración de sonido")]
    [SerializeField] private Slider sliderVolumen;

    [Header("Menú de bienvenida")]
    [Tooltip("Mostrar el menú al iniciar la partida (pantalla de bienvenida).")]
    [SerializeField] private bool mostrarMenuAlIniciar = true;

    [Tooltip("Título que se muestra en la pantalla de bienvenida.")]
    [SerializeField] private string tituloBienvenida = "HomePaws";

    [Tooltip("Texto del botón que empieza la partida.")]
    [SerializeField] private string textoBotonJugar = "Iniciar juego";

    [Tooltip("Título que se muestra al pausar la partida.")]
    [SerializeField] private string tituloPausa = "Menu de Pausa";

    [Tooltip("Texto del botón que continúa la partida.")]
    [SerializeField] private string textoBotonContinuar = "Continuar";

    [Tooltip("Texto del título del menú (opcional).")]
    [SerializeField] private TMP_Text tituloMenu;

    [Tooltip("Texto del botón de jugar/continuar (opcional).")]
    [SerializeField] private TMP_Text textoBotonPlay;

    private bool estaPausado;
    private bool yaEmpezado;
    private bool ayudaVisibleAntesDePausar;

    public bool EstaPausado
    {
        get { return estaPausado; }
    }

    private void Update()
    {
        // Escape abre y cierra el menu mientras se juega. Asi el menu de pausa
        // funciona en cualquier nivel, aunque no tenga boton en pantalla.
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (estaPausado)
            ContinuarJuego();
        else
            PausarJuego();
    }

    private void Start()
    {
        estaPausado = false;
        yaEmpezado = false;
        ayudaVisibleAntesDePausar = false;

        if (sliderVolumen != null)
        {
            sliderVolumen.value = GestorAudio.Instancia.VolumenMusica;
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }

        if (mostrarMenuAlIniciar)
        {
            // Pantalla de bienvenida: el juego espera a que se pulse "Iniciar juego".
            MostrarMenu();
            ActualizarTextos(true);
        }
        else
        {
            OcultarMenu();
        }
    }

    // Mostrar el menú (bienvenida o pausa) y detener el juego.
    private void MostrarMenu()
    {
        estaPausado = true;
        Time.timeScale = 0f;

        if (panel != null)
            panel.SetActive(true);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(true);

        if (menuConfig != null)
            menuConfig.SetActive(false);

        if (imagenFondo != null)
            imagenFondo.SetActive(true);

        if (botonPausa != null)
            botonPausa.SetActive(false);

        // La ayuda solo debe verse mientras se juega.
        ayudaVisibleAntesDePausar = ayudaControles != null && ayudaControles.activeSelf;

        if (ayudaControles != null)
            ayudaControles.SetActive(false);
    }

    // Abrir el menú y pausar el juego
    public void PausarJuego()
    {
        GestorAudio.Sonar(Sonido.Pausa, 0.8f);
        MostrarMenu();
        ActualizarTextos(false);
    }

    // Cerrar el menú y continuar jugando
    public void ContinuarJuego()
    {
        GestorAudio.Sonar(Sonido.Boton, 0.7f);
        OcultarMenu();
    }

    // Cierra el menu sin reproducir sonido (arranque del nivel).
    private void OcultarMenu()
    {
        estaPausado = false;
        Time.timeScale = 1f;

        if (panel != null)
            panel.SetActive(false);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(false);

        if (menuConfig != null)
            menuConfig.SetActive(false);

        if (imagenFondo != null)
            imagenFondo.SetActive(false);

        if (botonPausa != null)
            botonPausa.SetActive(true);

        // La ayuda aparece la primera vez que se empieza a jugar,
        // o si estaba visible justo antes de pausar.
        bool mostrarAyuda = !yaEmpezado || ayudaVisibleAntesDePausar;
        yaEmpezado = true;

        if (ayudaControles != null)
            ayudaControles.SetActive(mostrarAyuda);
    }

    // Abrir configuración sin reanudar el juego
    public void AbrirConfiguracion()
    {
        GestorAudio.Sonar(Sonido.Boton, 0.7f);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(false);

        if (menuConfig != null)
            menuConfig.SetActive(true);
    }

    // Volver desde configuración al menú de pausa
    public void VolverAlMenuPausa()
    {
        GestorAudio.Sonar(Sonido.Boton, 0.7f);

        if (menuConfig != null)
            menuConfig.SetActive(false);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(true);
    }

    // Cambiar los textos según se muestre la bienvenida o la pausa
    private void ActualizarTextos(bool bienvenida)
    {
        if (tituloMenu != null)
            tituloMenu.text = bienvenida ? tituloBienvenida : tituloPausa;

        if (textoBotonPlay != null)
            textoBotonPlay.text = bienvenida ? textoBotonJugar : textoBotonContinuar;
    }

    // Ajustar el volumen general
    public void CambiarVolumen(float volumen)
    {
        GestorAudio.AjustarVolumenGeneral(volumen);
    }

    // Reiniciar el nivel actual
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Volver a la escena del menú principal
    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuPrincipal");
    }

    // Cerrar el juego
    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
