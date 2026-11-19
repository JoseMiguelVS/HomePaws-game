using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Fundido a negro a pantalla completa para los cambios de seccion.
/// Se dibuja con OnGUI, igual que el resto de la interfaz, para no depender
/// del Canvas de la escena.
///
/// Uso:  yield return FundidoUI.Instancia.Fundir(0.4f, AlLlegarAlNegro, null);
/// </summary>
[DisallowMultipleComponent]
public class FundidoUI : MonoBehaviour
{
    public static FundidoUI Instancia { get; private set; }

    [SerializeField] private float alpha;

    public bool Ocupado { get; private set; }

    private void Awake()
    {
        Instancia = this;
        alpha = 0f;
    }

    /// <summary>
    /// Oscurece la pantalla, ejecuta 'alNegro' y vuelve a aclararla.
    /// 'duracion' es la duracion total aproximada del fundido.
    /// </summary>
    public IEnumerator Fundir(float duracion, Action alNegro, Action alVolver)
    {
        Ocupado = true;
        float mitad = Mathf.Max(0.05f, duracion * 0.5f);

        for (float t = 0f; t < mitad; t += Time.unscaledDeltaTime)
        {
            alpha = Mathf.Clamp01(t / mitad);
            yield return null;
        }

        alpha = 1f;
        alNegro?.Invoke();

        yield return new WaitForSecondsRealtime(0.12f);

        for (float t = 0f; t < mitad; t += Time.unscaledDeltaTime)
        {
            alpha = 1f - Mathf.Clamp01(t / mitad);
            yield return null;
        }

        alpha = 0f;
        Ocupado = false;
        alVolver?.Invoke();
    }

    /// <summary>
    /// Empieza en negro y aclara la pantalla. Util al arrancar una escena
    /// cinematografica.
    /// </summary>
    public IEnumerator Aparecer(float duracion)
    {
        Ocupado = true;
        alpha = 1f;

        float tiempo = Mathf.Max(0.05f, duracion);

        for (float t = 0f; t < tiempo; t += Time.unscaledDeltaTime)
        {
            alpha = 1f - Mathf.Clamp01(t / tiempo);
            yield return null;
        }

        alpha = 0f;
        Ocupado = false;
    }

    /// <summary>Oscurece la pantalla hasta negro y la deja asi.</summary>
    public IEnumerator Desaparecer(float duracion)
    {
        Ocupado = true;

        float tiempo = Mathf.Max(0.05f, duracion);

        for (float t = 0f; t < tiempo; t += Time.unscaledDeltaTime)
        {
            alpha = Mathf.Clamp01(t / tiempo);
            yield return null;
        }

        alpha = 1f;
        Ocupado = false;
    }

    private void OnGUI()
    {
        if (alpha <= 0f)
            return;

        Color anterior = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            InterfazJuego.Textura(Color.white)
        );
        GUI.color = anterior;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
