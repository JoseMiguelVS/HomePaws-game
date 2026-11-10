using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hace que el companero inactivo del duo siga al personaje activo por el
/// mismo camino que ha recorrido (incluidos saltos y plataformas).
///
/// Mientras sigue, su cuerpo pasa a cinematico y su collider se apaga para no
/// estorbar; al detenerse recupera la fisica normal y se queda esperando.
/// Lo controla ControlDuo, asi que no hay que configurar nada a mano.
/// </summary>
[DisallowMultipleComponent]
public class CompaneroSeguir : MonoBehaviour
{
    [Header("Seguimiento")]
    [Tooltip("Distancia que se mantiene detras del lider.")]
    [SerializeField] private float separacion = 1.15f;

    [Tooltip("Distancia minima entre dos puntos del rastro.")]
    [SerializeField] private float pasoRastro = 0.12f;

    [Tooltip("Numero maximo de puntos guardados del recorrido.")]
    [SerializeField] private int maximoPuntos = 240;

    [Tooltip("Si el lider se aleja mas de esto de golpe (reaparicion o cambio de seccion) el rastro se reinicia.")]
    [SerializeField] private float saltoMaximo = 6f;

    private readonly List<Vector3> rastro = new List<Vector3>();

    private Transform lider;
    private bool siguiendo;
    private bool componentesListos;

    private Rigidbody2D cuerpo;
    private Collider2D colisionador;
    private PlayerController control;
    private RigidbodyType2D tipoOriginal = RigidbodyType2D.Dynamic;
    private Vector3 escalaOriginal = Vector3.one;

    public bool Siguiendo { get { return siguiendo; } }

    private void Awake()
    {
        PrepararComponentes();
        escalaOriginal = transform.localScale;
    }

    private void PrepararComponentes()
    {
        if (componentesListos)
            return;

        cuerpo = GetComponent<Rigidbody2D>();
        colisionador = GetComponent<Collider2D>();
        control = GetComponent<PlayerController>();

        if (cuerpo != null)
            tipoOriginal = cuerpo.bodyType;

        componentesListos = true;
    }

    /// <summary>Empieza a seguir al transform indicado.</summary>
    public void Seguir(Transform nuevoLider)
    {
        PrepararComponentes();

        if (nuevoLider == null)
        {
            Detener();
            return;
        }

        if (siguiendo && lider == nuevoLider)
            return;

        lider = nuevoLider;
        siguiendo = true;

        // El rastro empieza en la posicion actual para no dar un salto brusco.
        rastro.Clear();
        rastro.Add(transform.position);

        AplicarModo(true);
    }

    /// <summary>Deja de seguir y devuelve la fisica normal al personaje.</summary>
    public void Detener()
    {
        PrepararComponentes();

        if (!siguiendo)
        {
            if (control != null)
                control.EstablecerVelocidadSeguimiento(0f);

            return;
        }

        siguiendo = false;
        lider = null;
        rastro.Clear();

        AplicarModo(false);
    }

    private void LateUpdate()
    {
        if (!siguiendo || lider == null)
            return;

        Vector3 posicionLider = lider.position;

        // Reaparicion, cambio de seccion o reagrupacion: el rastro viejo ya no vale.
        if (rastro.Count > 0 &&
            Vector3.Distance(rastro[rastro.Count - 1], posicionLider) > saltoMaximo)
        {
            rastro.Clear();
            rastro.Add(transform.position);
        }

        if (rastro.Count == 0 ||
            Vector3.Distance(rastro[rastro.Count - 1], posicionLider) >= pasoRastro)
        {
            rastro.Add(posicionLider);

            if (rastro.Count > maximoPuntos)
                rastro.RemoveAt(0);
        }

        Vector3 objetivo = CalcularObjetivo();
        float desplazamiento = objetivo.x - transform.position.x;

        // El cuerpo es cinematico mientras sigue: hay que moverlo tambien a el
        // para que la fisica y el transform no se desincronicen.
        if (cuerpo != null)
            cuerpo.position = objetivo;

        transform.position = objetivo;

        if (Mathf.Abs(desplazamiento) > 0.002f)
        {
            transform.localScale = new Vector3(
                desplazamiento > 0f ? 1f : -1f,
                escalaOriginal.y,
                escalaOriginal.z
            );
        }

        if (control != null)
            control.EstablecerVelocidadSeguimiento(Mathf.Abs(desplazamiento) > 0.002f ? 1f : 0f);
    }

    // Punto del recorrido que queda a 'separacion' detras del lider.
    private Vector3 CalcularObjetivo()
    {
        if (rastro.Count == 0)
            return transform.position;

        Vector3 actual = rastro[rastro.Count - 1];
        float acumulado = 0f;

        for (int i = rastro.Count - 2; i >= 0; i--)
        {
            Vector3 siguiente = rastro[i];
            float tramo = Vector3.Distance(actual, siguiente);

            if (tramo <= 0.0001f)
                continue;

            if (acumulado + tramo >= separacion)
            {
                float t = (separacion - acumulado) / tramo;
                return Vector3.Lerp(actual, siguiente, t);
            }

            acumulado += tramo;
            actual = siguiente;
        }

        // Todavia no hay recorrido suficiente: no moverse.
        return transform.position;
    }

    private void AplicarModo(bool seguir)
    {
        if (cuerpo != null)
        {
            if (seguir)
            {
                if (cuerpo.bodyType != RigidbodyType2D.Kinematic)
                {
                    tipoOriginal = cuerpo.bodyType;
                    cuerpo.bodyType = RigidbodyType2D.Kinematic;
                }

                cuerpo.linearVelocity = Vector2.zero;
                cuerpo.angularVelocity = 0f;
            }
            else
            {
                cuerpo.bodyType = tipoOriginal;
                cuerpo.linearVelocity = Vector2.zero;
            }
        }

        if (colisionador != null)
            colisionador.enabled = !seguir;

        if (control != null && !seguir)
            control.EstablecerVelocidadSeguimiento(0f);
    }

    private void OnDisable()
    {
        if (siguiendo)
            Detener();
    }
}
