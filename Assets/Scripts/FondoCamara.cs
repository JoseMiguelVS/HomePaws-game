using UnityEngine;

/// <summary>
/// Cambia el fondo que viaja con la camara entre dos secciones del mismo
/// nivel (alcantarillas y tejados) y ajusta el color de fondo de la camara.
/// Lo llama CambioSeccion al cruzar de una seccion a otra.
/// </summary>
[DisallowMultipleComponent]
public class FondoCamara : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer fondo;
    [SerializeField] private Camera camara;

    [Header("Sprites de cada seccion")]
    [SerializeField] private Sprite spriteAlcantarillas;
    [SerializeField] private Sprite spriteTejados;

    [Header("Color de fondo de la camara")]
    [SerializeField] private Color colorAlcantarillas = new Color(0.19f, 0.3f, 0.47f, 0f);
    [SerializeField] private Color colorTejados = new Color(0.85f, 0.55f, 0.36f, 0f);

    public bool EnTejados { get; private set; }

    private void Awake()
    {
        if (fondo == null)
            fondo = GetComponent<SpriteRenderer>();

        if (camara == null)
            camara = GetComponentInParent<Camera>();
    }

    public void MostrarAlcantarillas()
    {
        EnTejados = false;
        Aplicar(spriteAlcantarillas, colorAlcantarillas);
    }

    public void MostrarTejados()
    {
        EnTejados = true;
        Aplicar(spriteTejados, colorTejados);
    }

    public void Cambiar(bool tejados)
    {
        if (tejados)
            MostrarTejados();
        else
            MostrarAlcantarillas();
    }

    private void Aplicar(Sprite sprite, Color color)
    {
        if (fondo != null && sprite != null)
            fondo.sprite = sprite;

        if (camara != null)
            camara.backgroundColor = color;
    }
}
