using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mecanica del parque: cuenta los interruptores de piedra y, cuando estan
/// todos encendidos, activa los objetos del camino de piedra y abre la reja si
/// hay una. No sabe como se encienden ni donde estan: los InterruptorParque se
/// registran solos, asi el mismo gestor sirve para cualquier nivel.
///
/// Colocarlo en un objeto "MecanicaParque" y arrastrar los interruptores y los
/// objetos que aparecen al completarlos.
/// </summary>
public class GestorParque : MonoBehaviour
{
    [Header("Interruptores")]
    [Tooltip("Interruptores que hay que encender. Si esta vacio se buscan en la escena.")]
    [SerializeField] private List<InterruptorParque> interruptores = new List<InterruptorParque>();

    [Header("Recompensa")]
    [Tooltip("Objetos que se activan al encender todos los interruptores (camino de piedra).")]
    [SerializeField] private List<GameObject> objetosAlCompletar = new List<GameObject>();

    [Header("Reja (opcional)")]
    [Tooltip("Sprite de la reja que cambia al abrirse.")]
    [SerializeField] private SpriteRenderer reja;
    [Tooltip("Collider solido que bloquea el paso mientras la reja esta cerrada.")]
    [SerializeField] private Collider2D rejaCollider;
    [SerializeField] private Sprite rejaCerrada;
    [SerializeField] private Sprite rejaAbierta;

    [Header("Mensajes")]
    [SerializeField] private string mensajeInicio =
        "Enciende los 3 interruptores de piedra para abrir el camino del parque.";
    [SerializeField] private string mensajeProgreso = "Interruptor encendido ({0}/{1}).";
    [SerializeField] private string mensajeAbierta = "El camino de piedra aparecio. Ya puedes cruzar.";

    public static GestorParque Instancia { get; private set; }

    public bool Abierta { get; private set; }

    public int Total
    {
        get { return interruptores.Count; }
    }

    public int Encendidos
    {
        get
        {
            int total = 0;

            foreach (InterruptorParque interruptor in interruptores)
            {
                if (interruptor != null && interruptor.Encendido)
                    total++;
            }

            return total;
        }
    }

    private void Awake()
    {
        Instancia = this;

        if (interruptores.Count == 0)
        {
            interruptores.AddRange(
                FindObjectsByType<InterruptorParque>(FindObjectsSortMode.None)
            );
        }

        if (reja != null && rejaCerrada != null)
            reja.sprite = rejaCerrada;

        // Si la escena se guardo con todo encendido, la recompensa ya esta puesta.
        if (Total > 0 && Encendidos >= Total)
            Abrir(false);
        else
            MostrarObjetos(false);
    }

    private void Start()
    {
        if (Abierta || Total == 0)
            return;

        HUD_Duo.Mensaje(mensajeInicio, 4f);
    }

    /// <summary>Registra un interruptor creado o activado en tiempo de ejecucion.</summary>
    public void Registrar(InterruptorParque interruptor)
    {
        if (interruptor != null && !interruptores.Contains(interruptor))
            interruptores.Add(interruptor);
    }

    /// <summary>La llaman los interruptores al encenderse.</summary>
    public void InterruptorEncendido(InterruptorParque interruptor)
    {
        Registrar(interruptor);

        if (Abierta)
            return;

        HUD_Duo.Mensaje(string.Format(mensajeProgreso, Encendidos, Total), 2.5f);

        if (Encendidos >= Total)
            Abrir(true);
    }

    private void Abrir(bool avisar)
    {
        Abierta = true;

        if (reja != null && rejaAbierta != null)
            reja.sprite = rejaAbierta;

        if (rejaCollider != null)
            rejaCollider.enabled = false;

        MostrarObjetos(true);

        if (!avisar)
            return;

        GestorAudio.Sonar(Sonido.Puerta, 0.9f);
        HUD_Duo.Mensaje(mensajeAbierta, 3.5f);
    }

    private void MostrarObjetos(bool mostrar)
    {
        foreach (GameObject objeto in objetosAlCompletar)
        {
            if (objeto != null)
                objeto.SetActive(mostrar);
        }
    }

    /// <summary>Vuelve a cerrar el camino y apaga los interruptores.</summary>
    public void Reiniciar()
    {
        Abierta = false;

        if (reja != null && rejaCerrada != null)
            reja.sprite = rejaCerrada;

        if (rejaCollider != null)
            rejaCollider.enabled = true;

        MostrarObjetos(false);

        foreach (InterruptorParque interruptor in interruptores)
        {
            if (interruptor != null)
                interruptor.Reiniciar();
        }
    }
}
