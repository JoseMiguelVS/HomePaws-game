#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Anade al Nivel 2 y al Nivel 3 el mismo MusicManager que tiene el Nivel 1:
/// un AudioSource 2D que reproduce "musica_fondo" en bucle al arrancar.
///
/// Menu: "HomePaws > Herramientas > Copiar MusicManager del Nivel 1"
/// Batch:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod CopiarMusicManager.EjecutarBatch
/// </summary>
public static class CopiarMusicManager
{
    private const string NombreObjeto = "MusicManager";
    private const string RutaMusica = "Assets/Resources/Audio/musica_fondo.wav";

    private static readonly string[] EscenasDestino =
    {
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    [MenuItem("HomePaws/Herramientas/Copiar MusicManager del Nivel 1")]
    public static void EjecutarMenu()
    {
        int creados = Ejecutar();

        EditorUtility.DisplayDialog(
            "Copiar MusicManager",
            creados > 0
                ? "MusicManager anadido a " + creados + " escena(s)."
                : "Los niveles ya tenian MusicManager.",
            "Vale"
        );
    }

    /// <summary>Entrada para ejecutar en batchmode.</summary>
    public static void EjecutarBatch()
    {
        try
        {
            Ejecutar();
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("CopiarMusicManager: " + e);
            EditorApplication.Exit(1);
        }
    }

    public static int Ejecutar()
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaMusica);

        if (clip == null)
            throw new Exception("No se encuentra la musica en " + RutaMusica);

        int creados = 0;

        foreach (string ruta in EscenasDestino)
        {
            Scene destino = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            if (BuscarRaiz(destino, NombreObjeto) != null)
            {
                Debug.Log("CopiarMusicManager: " + ruta + " ya tiene " + NombreObjeto + ".");
                continue;
            }

            GameObject objeto = new GameObject(NombreObjeto);
            SceneManager.MoveGameObjectToScene(objeto, destino);

            AudioSource fuente = objeto.AddComponent<AudioSource>();
            fuente.clip = clip;
            fuente.playOnAwake = true;
            fuente.loop = true;
            fuente.volume = 1f;
            fuente.spatialBlend = 0f;
            fuente.mute = false;

            EditorSceneManager.MarkSceneDirty(destino);
            bool guardada = EditorSceneManager.SaveScene(destino);

            creados++;

            Debug.Log("CopiarMusicManager: " + ruta + " -> " + NombreObjeto +
                      " creado (clip: " + clip.name + ", guardada: " + guardada + ")");
        }

        AssetDatabase.SaveAssets();

        return creados;
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
}
#endif
