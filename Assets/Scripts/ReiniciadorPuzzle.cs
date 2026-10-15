using UnityEngine;

// Devuelve el puzle a su estado inicial: recoloca las cajas, suelta las
// placas y cierra las puertas. Sirve para "recuperar el estado inicial"
// cuando el jugador falla (se puede llamar al morir o desde una zona).
public class ReiniciadorPuzzle : MonoBehaviour
{
    [Header("Elementos del puzle")]
    [SerializeField] private CajaEmpujable[] cajas;
    [SerializeField] private PlacaPresion[] placas;
    [SerializeField] private Puerta[] puertas;

    [Header("Disparo automatico")]
    [Tooltip("Si se marca, al entrar el jugador en este objeto (con Collider2D trigger) se reinicia.")]
    [SerializeField] private bool reiniciarAlEntrarJugador = true;

    public void ReiniciarTodo()
    {
        if (cajas != null)
        {
            foreach (CajaEmpujable caja in cajas)
            {
                if (caja != null)
                    caja.Reiniciar();
            }
        }

        if (placas != null)
        {
            foreach (PlacaPresion placa in placas)
            {
                if (placa != null)
                    placa.Reiniciar();
            }
        }

        if (puertas != null)
        {
            foreach (Puerta puerta in puertas)
            {
                if (puerta != null)
                    puerta.Cerrar();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reiniciarAlEntrarJugador && other.CompareTag("Player"))
            ReiniciarTodo();
    }
}
