using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú de bienvenida del juego. Se coloca en el Canvas del menú y
/// arranca la partida, abre la configuración o cierra el juego.
/// </summary>
public class MenuBienvenida : MonoBehaviour
{
    [Header("Escenas")]
    [Tooltip("Nombre de la escena del juego que se carga al pulsar Jugar.")]
    [SerializeField] private string escenaJuego = "PrimerEscena";

    [Header("Configuración")]
    [Tooltip("Panel de configuración que se muestra al pulsar Configuracion. Puede quedar vacío.")]
    [SerializeField] private GameObject panelConfiguracion;

    [Tooltip("Panel del menú que se oculta al abrir la configuración. Puede quedar vacío.")]
    [SerializeField] private GameObject panelMenu;

    // Empezar la partida
    public void Jugar()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(escenaJuego))
        {
            Debug.LogError("MenuBienvenida: falta el nombre de la escena del juego.");
            return;
        }

        SceneManager.LoadScene(escenaJuego);
    }

    // Abrir la configuración
    public void AbrirConfiguracion()
    {
        if (panelConfiguracion != null)
            panelConfiguracion.SetActive(true);

        if (panelMenu != null)
            panelMenu.SetActive(false);
    }

    // Volver del panel de configuración al menú
    public void CerrarConfiguracion()
    {
        if (panelConfiguracion != null)
            panelConfiguracion.SetActive(false);

        if (panelMenu != null)
            panelMenu.SetActive(true);
    }

    // Cerrar el juego
    public void Salir()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
