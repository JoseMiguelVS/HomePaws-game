using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Configuración de Energía")]
    public float energiaMaxima = 100f;
    public float energiaActual;
    
    [Tooltip("Velocidad a la que disminuye la energía por segundo. A mayor número, más rápido se vacía.")]
    public float velocidadConsumo = 15f; 

    [Header("UI")]
    public Slider barraEnergia;
    public TextMeshProUGUI textoPuntos;

    private int puntosTotales = 0;
    private bool juegoTerminado = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        energiaActual = energiaMaxima;

        if (barraEnergia != null)
        {
            barraEnergia.maxValue = energiaMaxima;
            barraEnergia.value = energiaActual;
        }
    }

    private void Update()
    {
        if (juegoTerminado) return;

        if (energiaActual > 0)
        {
            energiaActual -= velocidadConsumo * Time.deltaTime;
            
            if (barraEnergia != null)
            {
                barraEnergia.value = energiaActual;
            }
        }
        
        if (energiaActual <= 0)
        {
            energiaActual = 0;
            if (barraEnergia != null) barraEnergia.value = 0;
            
            TerminarPartida();
        }
    }

    public void RecargarEnergia(float cantidad)
    {
        energiaActual += cantidad;
        
        if (energiaActual > energiaMaxima)
        {
            energiaActual = energiaMaxima;
        }

        if (barraEnergia != null)
        {
            barraEnergia.value = energiaActual;
        }
    }

    public void AgregarPuntos(int puntos)
    {
        puntosTotales += puntos;
        if (textoPuntos != null)
        {
            textoPuntos.text = "Coleccionable: " + puntosTotales;
        }
    }

    void TerminarPartida()
    {
        juegoTerminado = true;
        Debug.Log("¡Se acabó la energía! Reiniciando nivel...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}