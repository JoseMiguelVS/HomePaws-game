using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Zona de meta: al entrar el jugador muestra el cartel unificado de
// "nivel completado", funde a negro y carga la siguiente escena.
public class OnTriggerFin : MonoBehaviour
{
    [Header("Nivel completado")]
    [Tooltip("Panel u objeto que se muestra al completar el nivel. Puede quedar vacio.")]
    [SerializeField] private GameObject m_gameObject;

    [Tooltip("Nombre de la siguiente escena. Si se deja vacio, carga la siguiente en Build Settings.")]
    [SerializeField] private string m_nextSceneName = "";

    [Tooltip("Segundos que se muestra el cartel antes de fundir a negro.")]
    [SerializeField] private float m_delay = 2f;

    [Tooltip("Subtitulo del cartel. Se puede dejar vacio.")]
    [SerializeField] private string mensajeFinal = "Preparando el siguiente nivel...";

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

        GestorAudio.Sonar(Sonido.Meta);

        if (ControlDuo.Instancia != null)
            ControlDuo.Instancia.BloquearControl(true);

        HUD_Duo.NivelCompletado(mensajeFinal, Mathf.Max(1.5f, m_delay));

        Debug.Log("Nivel completado.");

        StartCoroutine(SecuenciaFinal());
    }

    private IEnumerator SecuenciaFinal()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, m_delay));

        FundidoUI fundido = FundidoUI.Instancia;

        if (fundido == null)
            fundido = new GameObject("FundidoUI").AddComponent<FundidoUI>();

        yield return fundido.Fundir(0.8f, CargarSiguiente, null);
    }

    private void CargarSiguiente()
    {
        if (!string.IsNullOrEmpty(m_nextSceneName))
        {
            SceneManager.LoadScene(m_nextSceneName.Trim());
            return;
        }

        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;

        if (siguiente < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(siguiente);
        else
            Debug.Log("No hay una siguiente escena en Build Settings.");
    }
}
