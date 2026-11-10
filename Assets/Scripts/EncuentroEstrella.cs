using System.Collections;
using UnityEngine;

/// <summary>
/// Encuentro con Estrella. Cuando el personaje controlado entra en la zona,
/// bloquea el control, muestra una conversacion corta y, al terminar, activa
/// el duo (ControlDuo.ActivarDuo).
///
/// Colocarlo en un objeto vacio con un BoxCollider2D marcado como Trigger que
/// cubra la zona donde se encuentran los dos personajes.
/// </summary>
public class EncuentroEstrella : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Control del duo. Si se deja vacio se usa ControlDuo.Instancia.")]
    [SerializeField] private ControlDuo controlDuo;

    [Header("Conversacion")]
    [Tooltip("Nombre del personaje que dice cada linea.")]
    [SerializeField] private string[] hablantes = { "Sol", "Estrella", "Sol", "Estrella" };

    [TextArea(2, 4)]
    [Tooltip("Lineas de la conversacion, en el mismo orden que los hablantes.")]
    [SerializeField] private string[] lineas =
    {
        "Estrella, te encontre. \u00bfEstas bien?",
        "\u00a1Sol! Pensaba que no llegabas. Esta compuerta tiene tres candados.",
        "Yo puedo empujar la caja pesada y mover la palanca.",
        "Y yo puedo saltar hasta la llave. \u00a1Vamos juntos!"
    };

    [Header("Ajustes")]
    [SerializeField] private float retardoAntesDeHablar = 0.35f;
    [SerializeField] private string mensajeFinal = "Estrella se uni\u00f3 al equipo. Pulsa TAB para cambiar de personaje.";

    private bool hecho;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hecho || !other.CompareTag("Player"))
            return;

        PlayerController jugador = other.GetComponent<PlayerController>();

        // Solo el personaje que controla el jugador puede iniciar la charla.
        if (jugador == null || !jugador.controlActivo)
            return;

        hecho = true;

        if (controlDuo == null)
            controlDuo = ControlDuo.Instancia;

        StartCoroutine(Conversacion());
    }

    private IEnumerator Conversacion()
    {
        if (controlDuo != null)
            controlDuo.BloquearControl(true);

        if (retardoAntesDeHablar > 0f)
            yield return new WaitForSecondsRealtime(retardoAntesDeHablar);

        if (DialogoUI.Instancia == null)
        {
            Terminar();
            yield break;
        }

        DialogoUI.Instancia.Mostrar(hablantes, lineas, Terminar);
    }

    private void Terminar()
    {
        if (controlDuo != null)
        {
            controlDuo.BloquearControl(false);
            controlDuo.ActivarDuo();
        }

        if (HUD_Duo.Instancia != null)
            HUD_Duo.Mensaje(mensajeFinal, 5f);
    }
}
