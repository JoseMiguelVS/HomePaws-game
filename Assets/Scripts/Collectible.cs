using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Recogible generico reutilizable (monedas, llaves, atun...). Al tocarlo el
/// jugador suena, lanza un evento, se oculta y se destruye.
///
/// Es opcional: la recogida de atun del nivel 1 la sigue haciendo
/// PlayerController por etiqueta. Usa este componente para recogibles nuevos
/// que no necesiten logica propia.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Etiqueta del objeto que puede recogerlo.")]
    [SerializeField] private string etiquetaJugador = "Player";

    [Header("Efectos")]
    [Tooltip("Sonido al recogerlo.")]
    [SerializeField] private Sonido sonido = Sonido.Moneda;

    [Tooltip("Objeto que se muestra al recogerlo (por ejemplo, un destello). Opcional.")]
    [SerializeField] private GameObject efecto;

    [Tooltip("Segundos antes de destruir el objeto. Usa 0 para destruirlo al instante.")]
    [SerializeField] private float retardo = 0f;

    [Header("Eventos")]
    [Tooltip("Se invoca al recogerlo. Util para sumar puntos o abrir una puerta.")]
    [SerializeField] private UnityEvent alRecoger;

    private bool recogido;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogido)
            return;

        if (!other.CompareTag(etiquetaJugador))
            return;

        Recoger();
    }

    /// <summary>Recoge el objeto. Tambien se puede llamar desde otro script.</summary>
    public void Recoger()
    {
        if (recogido)
            return;

        recogido = true;

        GestorAudio.Sonar(sonido);

        if (efecto != null)
        {
            efecto.SetActive(true);
            efecto.transform.SetParent(null);
            Destroy(efecto, 2f);
        }

        alRecoger?.Invoke();

        DesactivarCollider();

        Destroy(gameObject, Mathf.Max(0f, retardo));
    }

    private void DesactivarCollider()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        // Si el objeto tiene un SpriteRenderer, se oculta hasta destruirse.
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.enabled = false;
    }
}
