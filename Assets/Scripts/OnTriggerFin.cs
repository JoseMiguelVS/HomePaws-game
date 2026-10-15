using UnityEngine;
using UnityEngine.SceneManagement;

// Zona de meta: al entrar el jugador muestra un mensaje de "nivel completado"
// y pasa a la siguiente escena.
public class OnTriggerFin : MonoBehaviour
{
    [Header("Nivel completado")]
    [Tooltip("Panel u objeto que se muestra al completar el nivel.")]
    [SerializeField] private GameObject m_gameObject;

    [Tooltip("Nombre de la siguiente escena. Si se deja vacio, carga la siguiente en Build Settings.")]
    [SerializeField] private string m_nextSceneName = "";

    [Tooltip("Segundos que se muestra el mensaje antes de cambiar de escena.")]
    [SerializeField] private float m_delay = 2f;

    private bool m_triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_triggered)
            return;

        if (!collision.CompareTag("Player"))
            return;

        m_triggered = true;

        if (m_gameObject != null)
            m_gameObject.SetActive(true);

        Debug.Log("Nivel completado.");

        if (m_delay > 0f)
            Invoke(nameof(IrAlSiguienteNivel), m_delay);
        else
            IrAlSiguienteNivel();
    }

    private void IrAlSiguienteNivel()
    {
        if (!string.IsNullOrEmpty(m_nextSceneName))
        {
            SceneManager.LoadScene(m_nextSceneName);
            return;
        }

        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;

        if (siguiente < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(siguiente);
        else
            Debug.Log("No hay una siguiente escena en Build Settings.");
    }
}
