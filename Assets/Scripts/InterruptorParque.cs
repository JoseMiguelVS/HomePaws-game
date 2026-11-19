using UnityEngine;

/// <summary>
/// Interruptor de piedra del parque. Se enciende cuando un personaje se sube
/// encima y ya no se apaga: asi el jugador puede resolverlo en el orden que
/// quiera. No conoce la reja; solo avisa al GestorParque, de modo que se puede
/// reutilizar en cualquier nivel.
///
/// Colocarlo en un objeto con SpriteRenderer y un Collider2D en trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InterruptorParque : MonoBehaviour
{
    [Header("Aspecto")]
    [Tooltip("Sprite del interruptor. Si se deja vacio se busca en este objeto.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteApagado;
    [SerializeField] private Sprite spriteEncendido;

    [Header("Deteccion")]
    [Tooltip("Etiqueta que enciende el interruptor.")]
    [SerializeField] private string etiquetaJugador = "Player";

    [Tooltip("Si se marca, basta con que el collider del jugador lo roce.")]
    [SerializeField] private bool usarTrigger = true;

    public bool Encendido { get; private set; }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Pintar();
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!usarTrigger || Encendido || !EsJugador(otro))
            return;

        Encender();
    }

    private void OnCollisionEnter2D(Collision2D choque)
    {
        if (usarTrigger || Encendido)
            return;

        if (choque.collider != null && EsJugador(choque.collider))
            Encender();
    }

    /// <summary>Enciende el interruptor. Tambien se puede llamar desde un boton.</summary>
    public void Encender()
    {
        if (Encendido)
            return;

        Encendido = true;
        Pintar();

        GestorAudio.Sonar(Sonido.Checkpoint, 0.7f);

        if (GestorParque.Instancia != null)
            GestorParque.Instancia.InterruptorEncendido(this);
        else
            HUD_Duo.Mensaje("Interruptor encendido.", 2f);
    }

    /// <summary>Vuelve al estado inicial (para reiniciar el puzle).</summary>
    public void Reiniciar()
    {
        Encendido = false;
        Pintar();
    }

    private bool EsJugador(Collider2D otro)
    {
        if (otro == null)
            return false;

        if (!string.IsNullOrEmpty(etiquetaJugador) && otro.CompareTag(etiquetaJugador))
            return true;

        // Respaldo por si el collider cuelga de un objeto sin la etiqueta.
        return otro.GetComponentInParent<PerfilPersonaje>() != null;
    }

    private void Pintar()
    {
        if (spriteRenderer == null)
            return;

        if (Encendido && spriteEncendido != null)
            spriteRenderer.sprite = spriteEncendido;
        else if (!Encendido && spriteApagado != null)
            spriteRenderer.sprite = spriteApagado;
    }
}
