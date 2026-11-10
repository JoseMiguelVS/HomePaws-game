using UnityEngine;

/// <summary>
/// Oculta el texto de ayuda de los controles ("A / D para avanzar",
/// "Espacio para saltar") en cuanto el jugador se mueve o salta.
/// Se coloca en el mismo objeto que el texto de ayuda.
/// </summary>
public class AyudaControles : MonoBehaviour
{
    [Tooltip("Objeto con el texto de ayuda. Si se deja vacío se usa este mismo objeto.")]
    [SerializeField] private GameObject textoAyuda;

    [Tooltip("Ocultar la ayuda cuando el jugador se mueva.")]
    [SerializeField] private bool ocultarAlMoverse = true;

    [Tooltip("Ocultar la ayuda cuando el jugador salte.")]
    [SerializeField] private bool ocultarAlSaltar = true;

    private bool yaOculto;

    private void Awake()
    {
        if (textoAyuda == null)
            textoAyuda = gameObject;
    }

    private void OnEnable()
    {
        // Si la ayuda vuelve a mostrarse (al empezar la partida),
        // se vuelve a escuchar la entrada hasta que el jugador actúe.
        yaOculto = false;
    }

    private void Update()
    {
        if (yaOculto)
            return;

        if (ocultarAlMoverse && SeEstaMoviendo())
        {
            Ocultar();
            return;
        }

        if (ocultarAlSaltar && SeEstaSaltando())
        {
            Ocultar();
        }
    }

    // ¿El jugador está intentando moverse en horizontal?
    private static bool SeEstaMoviendo()
    {
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f)
            return true;

        return Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.LeftArrow) ||
               Input.GetKey(KeyCode.RightArrow);
    }

    // ¿El jugador acaba de pulsar saltar?
    private static bool SeEstaSaltando()
    {
        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.UpArrow);
    }

    /// <summary>Vuelve a mostrar el texto de ayuda.</summary>
    public void Mostrar()
    {
        yaOculto = false;

        if (textoAyuda != null)
            textoAyuda.SetActive(true);
    }

    /// <summary>Oculta el texto de ayuda.</summary>
    public void Ocultar()
    {
        yaOculto = true;

        if (textoAyuda != null)
            textoAyuda.SetActive(false);
    }
}
