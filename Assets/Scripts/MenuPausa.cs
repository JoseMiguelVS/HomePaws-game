
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Paneles de la interfaz")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject menuConfig;
    [SerializeField] private GameObject imagenFondo;

    [Header("Configuración de sonido")]
    [SerializeField] private Slider sliderVolumen;

    private bool estaPausado = false;

    private void Start()
    {
        Time.timeScale = 1f;
        estaPausado = false;

        if (panel != null)
            panel.SetActive(false);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(false);

        if (menuConfig != null)
            menuConfig.SetActive(false);

        if (imagenFondo != null)
            imagenFondo.SetActive(false);

        if (sliderVolumen != null)
        {
            sliderVolumen.value = AudioListener.volume;
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }
    }

    // Abrir el menú y pausar el juego
    public void PausarJuego()
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
    }

    // Cerrar el menú y continuar jugando
    public void ContinuarJuego()
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
    }

    // Abrir configuración sin reanudar el juego
    public void AbrirConfiguracion()
    {
        if (menuPrincipal != null)
            menuPrincipal.SetActive(false);

        if (menuConfig != null)
            menuConfig.SetActive(true);
    }

    // Volver desde configuración al menú de pausa
    public void VolverAlMenuPausa()
    {
        if (menuConfig != null)
            menuConfig.SetActive(false);

        if (menuPrincipal != null)
            menuPrincipal.SetActive(true);
    }

    // Ajustar el volumen general
    public void CambiarVolumen(float volumen)
    {
        AudioListener.volume = volumen;
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
