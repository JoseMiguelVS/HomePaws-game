#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Anade al panel de configuracion de los tres niveles un boton que alterna
/// entre pantalla completa y ventana. Reutiliza el boton "Salir" que ya existe
/// en el Canvas para no cambiar el estilo del menu.
///
/// Menu: "HomePaws > Herramientas > Boton de pantalla completa"
/// Batch:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod BotonPantallaCompleta.AplicarBatch
/// </summary>
public static class BotonPantallaCompleta
{
    private static readonly string[] Escenas =
    {
        "Assets/Scenes/Level1.unity",
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    private const string NombreBoton = "btnPantalla";
    private const float Separacion = 110f;

    [MenuItem("HomePaws/Herramientas/Boton de pantalla completa")]
    public static void EjecutarMenu()
    {
        int total = 0;

        foreach (string escena in Escenas)
            total += Ejecutar(escena);

        EditorUtility.DisplayDialog(
            "Boton de pantalla completa",
            "Botones revisados en " + total + " escenas. Mira la consola para el detalle.",
            "Vale"
        );
    }

    public static void AplicarBatch()
    {
        try
        {
            foreach (string escena in Escenas)
                Ejecutar(escena);

            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("BotonPantallaCompleta: " + e);
            EditorApplication.Exit(1);
        }
    }

    private static int Ejecutar(string ruta)
    {
        Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

        MenuPausa menu = UnityEngine.Object.FindFirstObjectByType<MenuPausa>(FindObjectsInactive.Include);

        if (menu == null)
        {
            Debug.LogWarning("BotonPantallaCompleta: no hay MenuPausa en " + ruta);
            return 0;
        }

        SerializedObject serializado = new SerializedObject(menu);
        SerializedProperty propiedadConfig = serializado.FindProperty("menuConfig");

        GameObject menuConfig = propiedadConfig != null
            ? propiedadConfig.objectReferenceValue as GameObject
            : null;

        if (menuConfig == null)
        {
            Debug.LogWarning("BotonPantallaCompleta: MenuPausa sin menuConfig en " + ruta);
            return 0;
        }

        Transform existente = menuConfig.transform.Find(NombreBoton);
        GameObject boton;

        if (existente != null)
        {
            boton = existente.gameObject;
        }
        else
        {
            Transform modelo = menuConfig.transform.Find("btnExiit");

            if (modelo == null)
            {
                Debug.LogWarning("BotonPantallaCompleta: no se encontro btnExiit en " + ruta);
                return 0;
            }

            boton = UnityEngine.Object.Instantiate(modelo.gameObject, menuConfig.transform);
            boton.name = NombreBoton;

            RectTransform rectNuevo = boton.GetComponent<RectTransform>();
            RectTransform rectModelo = modelo.GetComponent<RectTransform>();

            if (rectNuevo != null && rectModelo != null)
            {
                rectNuevo.anchorMin = rectModelo.anchorMin;
                rectNuevo.anchorMax = rectModelo.anchorMax;
                rectNuevo.pivot = rectModelo.pivot;
                rectNuevo.sizeDelta = rectModelo.sizeDelta;
                rectNuevo.anchoredPosition =
                    rectModelo.anchoredPosition + new Vector2(0f, -Separacion);
            }
        }

        Button componenteBoton = boton.GetComponent<Button>();

        if (componenteBoton == null)
        {
            Debug.LogWarning("BotonPantallaCompleta: el boton copiado no tiene Button en " + ruta);
            return 0;
        }

        // La copia hereda el "Salir" del boton original: se quitan esos eventos
        // porque el clic lo gestiona MenuPausa en tiempo de ejecucion.
        SerializedObject serializadoBoton = new SerializedObject(componenteBoton);
        SerializedProperty llamadas =
            serializadoBoton.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");

        if (llamadas != null)
        {
            llamadas.arraySize = 0;
            serializadoBoton.ApplyModifiedPropertiesWithoutUndo();
        }

        TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);

        if (texto != null)
            texto.text = "Pantalla: Completa";

        serializado.FindProperty("botonPantalla").objectReferenceValue = componenteBoton;
        serializado.FindProperty("textoBotonPantalla").objectReferenceValue = texto;
        serializado.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(escena);
        bool guardada = EditorSceneManager.SaveScene(escena);

        Debug.Log("BotonPantallaCompleta: " + ruta + " boton listo, guardada=" + guardada);

        return 1;
    }
}
#endif
