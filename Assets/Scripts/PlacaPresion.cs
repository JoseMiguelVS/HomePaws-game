using UnityEngine;
using UnityEngine.Events;

// Placa de presion: detecta al jugador y/o a las cajas que se apoyan encima.
// No sabe nada de puertas: solo avisa mediante eventos (asi es reutilizable).
[RequireComponent(typeof(Collider2D))]
public class PlacaPresion : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Etiquetas que cuentan como peso valido (jugador, cajas...).")]
    [SerializeField] private string[] etiquetasValidas = { "Player", "Caja" };

    [Tooltip("Cuantos objetos hay que colocar encima para activarla.")]
    [SerializeField] private int cantidadNecesaria = 1;

    [Tooltip("Si se marca, una vez pulsada ya no se suelta.")]
    [SerializeField] private bool permanecePulsada = false;

    [Header("Eventos")]
    [SerializeField] private UnityEvent alPulsar;
    [SerializeField] private UnityEvent alSoltar;

    public bool EstaPulsada { get; private set; }

    private int objetosEncima;

    // Al anadir el componente, deja el collider como trigger.
    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!EsValido(other))
            return;

        objetosEncima++;
        ActualizarEstado();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!EsValido(other))
            return;

        objetosEncima = Mathf.Max(0, objetosEncima - 1);
        ActualizarEstado();
    }

    private bool EsValido(Collider2D other)
    {
        foreach (string etiqueta in etiquetasValidas)
        {
            if (!string.IsNullOrEmpty(etiqueta) && other.CompareTag(etiqueta))
                return true;
        }

        return false;
    }

    private void ActualizarEstado()
    {
        // Modo permanente: una vez pulsada, se ignora el soltar.
        if (permanecePulsada && EstaPulsada)
            return;

        bool pulsada = objetosEncima >= cantidadNecesaria;

        if (pulsada == EstaPulsada)
            return;

        EstaPulsada = pulsada;

        if (pulsada)
            alPulsar?.Invoke();
        else
            alSoltar?.Invoke();
    }

    // Suelta la placa y avisa, para volver al estado inicial del puzle.
    public void Reiniciar()
    {
        objetosEncima = 0;

        bool estabaPulsada = EstaPulsada;
        EstaPulsada = false;

        if (estabaPulsada)
            alSoltar?.Invoke();
    }
}
