using UnityEngine;

// Sección de puzles / obstáculos: al entrar el jugador activa un grupo de
// obstáculos y, si se quiere, los desactiva al salir.
public class PuzzleTrigger : MonoBehaviour
{
    [Header("Sección de puzle / obstáculos")]
    [Tooltip("Grupo de obstáculos que se activa al entrar el jugador.")]
    [SerializeField] private GameObject m_obstaculos;

    [Tooltip("Si se marca, los obstáculos se desactivan al salir el jugador.")]
    [SerializeField] private bool m_desactivarAlSalir = true;

    private bool avisoMostrado;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (m_obstaculos != null)
        {
            m_obstaculos.SetActive(true);
            return;
        }

        // Aviso util: sin grupo asignado el disparador no hace nada.
        if (!avisoMostrado)
        {
            avisoMostrado = true;
            Debug.LogWarning(
                "PuzzleTrigger de '" + name + "': falta asignar 'm_obstaculos' en el Inspector.",
                this
            );
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (m_desactivarAlSalir && m_obstaculos != null)
            m_obstaculos.SetActive(false);
    }
}
