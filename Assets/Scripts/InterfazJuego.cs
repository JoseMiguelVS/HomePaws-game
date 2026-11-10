using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utilidades de dibujo compartidas por el HUD, el dialogo y los indicadores.
/// Todo se dibuja con OnGUI en un espacio de 1920x1080, asi no hace falta
/// tocar el Canvas ni los menus que ya existen.
/// </summary>
public static class InterfazJuego
{
    public const float Ancho = 1920f;
    public const float Alto = 1080f;

    private static Texture2D texturaBlanca;
    private static readonly Dictionary<string, GUIStyle> estilos = new Dictionary<string, GUIStyle>();

    public static float Escala
    {
        get { return Mathf.Min(Screen.width / Ancho, Screen.height / Alto); }
    }

    public static float OffsetX
    {
        get { return (Screen.width - Ancho * Escala) * 0.5f; }
    }

    public static float OffsetY
    {
        get { return (Screen.height - Alto * Escala) * 0.5f; }
    }

    /// <summary>Activa la escala de referencia 1920x1080.</summary>
    public static void Empezar()
    {
        float escala = Escala;
        GUI.matrix = Matrix4x4.TRS(
            new Vector3(OffsetX, OffsetY, 0f),
            Quaternion.identity,
            new Vector3(escala, escala, 1f)
        );
    }

    /// <summary>Devuelve la matriz de GUI a su estado normal.</summary>
    public static void Terminar()
    {
        GUI.matrix = Matrix4x4.identity;
    }

    /// <summary>Convierte un punto de la pantalla al espacio de 1920x1080.</summary>
    public static Vector2 APantallaLogica(Vector3 puntoPantalla)
    {
        float escala = Escala;

        return new Vector2(
            (puntoPantalla.x - OffsetX) / escala,
            ((Screen.height - puntoPantalla.y) - OffsetY) / escala
        );
    }

    /// <summary>Textura de 1x1 pixel del color indicado (se reutiliza).</summary>
    public static Texture2D Textura(Color color)
    {
        if (texturaBlanca == null)
        {
            texturaBlanca = new Texture2D(1, 1);
            texturaBlanca.hideFlags = HideFlags.HideAndDontSave;
            texturaBlanca.SetPixel(0, 0, Color.white);
            texturaBlanca.Apply();
        }

        texturaBlanca.SetPixel(0, 0, color);
        texturaBlanca.Apply();

        return texturaBlanca;
    }

    public static void Panel(Rect rect, Color color)
    {
        Color anterior = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Textura(Color.white));
        GUI.color = anterior;
    }

    public static void PanelConBorde(Rect rect, Color fondo, Color borde, float grosor = 2f)
    {
        Panel(rect, borde);
        Panel(
            new Rect(
                rect.x + grosor,
                rect.y + grosor,
                rect.width - grosor * 2f,
                rect.height - grosor * 2f
            ),
            fondo
        );
    }

    /// <summary>Estilo de texto reutilizable (se cachea por tamano, color y anclaje).</summary>
    public static GUIStyle Estilo(int tamano, Color color, bool negrita = false, TextAnchor anclaje = TextAnchor.UpperLeft)
    {
        string clave = tamano + "|" + color + "|" + negrita + "|" + anclaje;

        GUIStyle estilo;
        if (estilos.TryGetValue(clave, out estilo))
            return estilo;

        estilo = new GUIStyle(GUI.skin.label);
        estilo.fontSize = tamano;
        estilo.fontStyle = negrita ? FontStyle.Bold : FontStyle.Normal;
        estilo.alignment = anclaje;
        estilo.normal.textColor = color;
        estilo.hover.textColor = color;
        estilo.active.textColor = color;
        estilo.wordWrap = true;
        estilo.richText = true;
        estilo.clipping = TextClipping.Overflow;

        estilos[clave] = estilo;
        return estilo;
    }

    public static void Etiqueta(Rect rect, string texto, GUIStyle estilo)
    {
        GUI.Label(rect, texto, estilo);
    }

    /// <summary>Texto con una sombra oscura para que se lea sobre cualquier fondo.</summary>
    public static void EtiquetaConSombra(Rect rect, string texto, GUIStyle estilo, float desplazamiento = 2f)
    {
        Color color = estilo.normal.textColor;

        GUIStyle sombra = Estilo(
            estilo.fontSize,
            new Color(0f, 0f, 0f, 0.75f),
            estilo.fontStyle == FontStyle.Bold,
            estilo.alignment
        );

        GUI.Label(new Rect(rect.x + desplazamiento, rect.y + desplazamiento, rect.width, rect.height), texto, sombra);
        GUI.Label(rect, texto, estilo);

        estilo.normal.textColor = color;
    }

    /// <summary>Dibuja una barra de color (util para separadores y acentos).</summary>
    public static void Barra(Rect rect, Color color)
    {
        Panel(rect, color);
    }

    /// <summary>Ancho aproximado de un texto con el estilo indicado.</summary>
    public static float AnchoTexto(string texto, GUIStyle estilo)
    {
        return estilo.CalcSize(new GUIContent(texto)).x;
    }
}
