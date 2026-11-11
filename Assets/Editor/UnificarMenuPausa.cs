#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Copia el Canvas "MenuPausa" del Nivel 1 (nivel de referencia) a los
/// Niveles 2 y 3, sustituyendo los Canvas antiguos y reconectando las
/// referencias (HUD_Duo.menuPausa y cualquier otra del tipo MenuPausa).
///
/// Se puede lanzar desde el menu "HomePaws > Herramientas" o en batch:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod UnificarMenuPausa.EjecutarBatch
/// </summary>
public static class UnificarMenuPausa
{
    private const string EscenaModelo = "Assets/Scenes/Level1.unity";
    private const string NombreMenu = "MenuPausa";

    private static readonly string[] EscenasDestino =
    {
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    [MenuItem("HomePaws/Herramientas/Unificar menu de pausa")]
    public static void EjecutarMenu()
    {
        bool continuar = EditorUtility.DisplayDialog(
            "Unificar menu de pausa",
            "Se copiara el Canvas MenuPausa del Nivel 1 a los Niveles 2 y 3.\n" +
            "Los Canvas actuales de esos niveles se sustituiran.",
            "Copiar",
            "Cancelar"
        );

        if (!continuar)
            return;

        Ejecutar(false);
        EditorUtility.DisplayDialog("Unificar menu de pausa", "Menu copiado a los Niveles 2 y 3.", "Vale");
    }

    /// <summary>Entrada para ejecutar en batchmode.</summary>
    public static void EjecutarBatch()
    {
        try
        {
            Ejecutar(true);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("UnificarMenuPausa: " + e);
            EditorApplication.Exit(1);
        }
    }

    public static void Ejecutar(bool silencioso)
    {
        Scene modelo = EditorSceneManager.OpenScene(EscenaModelo, OpenSceneMode.Single);

        GameObject raizModelo = BuscarRaiz(modelo, NombreMenu);

        if (raizModelo == null)
            throw new Exception("No se encontro el Canvas '" + NombreMenu + "' en " + EscenaModelo);

        MenuPausa componenteModelo = raizModelo.GetComponent<MenuPausa>();

        if (componenteModelo == null)
            throw new Exception("El Canvas '" + NombreMenu + "' del Nivel 1 no tiene el componente MenuPausa.");

        Debug.Log("UnificarMenuPausa: modelo encontrado en " + EscenaModelo +
                  " (hijos: " + raizModelo.transform.childCount + ")");

        foreach (string ruta in EscenasDestino)
        {
            Scene destino = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(destino);

            GameObject viejo = BuscarRaiz(destino, NombreMenu);
            MenuPausa componenteViejo = viejo != null ? viejo.GetComponent<MenuPausa>() : null;

            if (viejo == null)
                Debug.LogWarning("UnificarMenuPausa: " + ruta + " no tenia un Canvas '" + NombreMenu + "'. Se crea uno nuevo.");
            else if (componenteViejo == null)
                Debug.LogWarning("UnificarMenuPausa: el Canvas antiguo de " + ruta + " no tenia MenuPausa.");

            GameObject copia = UnityEngine.Object.Instantiate(raizModelo);
            copia.name = NombreMenu;
            SceneManager.MoveGameObjectToScene(copia, destino);
            copia.SetActive(true);

            MenuPausa componenteNuevo = copia.GetComponent<MenuPausa>();

            if (componenteNuevo == null)
                throw new Exception("La copia del menu en " + ruta + " no tiene MenuPausa.");

            // Las referencias se cambian antes de borrar el Canvas antiguo:
            // al destruirlo, los campos que apuntaban a el quedarian a null.
            int referencias = ReemplazarReferencias(destino, componenteViejo, componenteNuevo);

            if (viejo != null)
                UnityEngine.Object.DestroyImmediate(viejo);

            EditorSceneManager.MarkSceneDirty(destino);
            bool guardada = EditorSceneManager.SaveScene(destino);

            Debug.Log("UnificarMenuPausa: " + ruta +
                      " -> menu copiado (hijos: " + copia.transform.childCount +
                      "), referencias reconectadas: " + referencias +
                      ", guardada: " + guardada);

            EditorSceneManager.CloseScene(destino, true);
        }

        // El Nivel 1 solo se ha leido: no se guarda.
        if (SceneManager.sceneCount > 1)
            EditorSceneManager.CloseScene(modelo, true);

        AssetDatabase.SaveAssets();

        if (!silencioso)
            Debug.Log("UnificarMenuPausa: terminado.");
    }

    private static GameObject BuscarRaiz(Scene escena, string nombre)
    {
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name == nombre)
                return raiz;
        }

        return null;
    }

    /// <summary>
    /// Recorre todos los componentes de la escena y cambia cualquier referencia
    /// al MenuPausa antiguo por el nuevo. Devuelve cuantas ha cambiado.
    /// </summary>
    private static int ReemplazarReferencias(Scene escena, MenuPausa viejo, MenuPausa nuevo)
    {
        if (nuevo == null)
            return 0;

        int cambios = 0;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (MonoBehaviour componente in raiz.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (componente == null || componente == nuevo)
                    continue;

                SerializedObject serializado = new SerializedObject(componente);
                SerializedProperty propiedad = serializado.GetIterator();
                bool modificado = false;

                while (propiedad.NextVisible(true))
                {
                    if (propiedad.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    UnityEngine.Object valor = propiedad.objectReferenceValue;

                    if (viejo != null)
                    {
                        if (valor != viejo)
                            continue;
                    }
                    else if (!(valor is MenuPausa))
                    {
                        continue;
                    }

                    propiedad.objectReferenceValue = nuevo;
                    modificado = true;
                    cambios++;
                }

                if (modificado)
                {
                    serializado.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(componente);
                }
            }
        }

        return cambios;
    }
}
#endif
