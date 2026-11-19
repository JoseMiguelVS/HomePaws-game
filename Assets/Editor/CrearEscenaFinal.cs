using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Herramienta de editor para crear la escena final (cinematica de llegada a
/// casa) y dejar la lista de escenas de la build en orden.
///
/// En batch:
///   Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CrearEscenaFinal.Crear
/// </summary>
public static class CrearEscenaFinal
{
    private const string RutaFinal = "Assets/Scenes/Final.unity";
    private const string RutaFondo = "Assets/sprites/fondos/camino-hogares.png";
    private const string RutaSol = "Assets/Sprites/Players/Sol/cat_walk_01.png";
    private const string RutaEstrella = "Assets/sprites/Players/Estrella/walk_01_siamese.png";
    private const string RutaPuertaCerrada = "Assets/sprites/items/door_closed.png";
    private const string RutaPuertaAbierta = "Assets/sprites/items/door_open.png";

    [MenuItem("HomePaws/Crear escena final")]
    public static void Crear()
    {
        Sprite fondo = PrimerSprite(RutaFondo);
        Sprite puertaCerrada = PrimerSprite(RutaPuertaCerrada);
        Sprite puertaAbierta = PrimerSprite(RutaPuertaAbierta);
        Sprite[] solFrames = Frames(RutaSol);
        Sprite[] estrellaFrames = Frames(RutaEstrella);

        if (fondo == null || puertaCerrada == null || puertaAbierta == null || solFrames.Length == 0 || estrellaFrames.Length == 0)
        {
            Debug.LogError("CrearEscenaFinal: faltan sprites para montar la escena.");
            return;
        }

        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camara
        GameObject camaraGO = new GameObject("Main Camera");
        camaraGO.tag = "MainCamera";
        Camera camara = camaraGO.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = 5.12f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.1f, 0.08f, 0.14f, 1f);
        camaraGO.transform.position = new Vector3(7.68f, 5.12f, -10f);
        camaraGO.AddComponent<AudioListener>();

        // Fondo con las casas: se escala para cubrir la camara sea cual sea el
        // pivote del sprite importado.
        GameObject fondoGO = new GameObject("Fondo");
        SpriteRenderer fondoSr = fondoGO.AddComponent<SpriteRenderer>();
        fondoSr.sprite = fondo;
        fondoSr.sortingOrder = -20;
        ColocarFondo(fondoGO.transform, fondo);

        // Puerta de casa
        GameObject puertaGO = new GameObject("PuertaCasa");
        SpriteRenderer puertaSr = puertaGO.AddComponent<SpriteRenderer>();
        puertaSr.sprite = puertaCerrada;
        puertaSr.sortingOrder = 1;
        puertaGO.transform.position = new Vector3(11.7f, 2.62f, 0f);
        puertaGO.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        // Gatos
        GameObject solGO = CrearGato("Sol", solFrames[0], 2);
        GameObject estrellaGO = CrearGato("Estrella", estrellaFrames[0], 2);

        // Cinematica
        GameObject sistema = new GameObject("CinematicaFinal");
        CinematicaFinal cine = sistema.AddComponent<CinematicaFinal>();

        SerializedObject so = new SerializedObject(cine);
        so.FindProperty("sol").objectReferenceValue = solGO.transform;
        so.FindProperty("estrella").objectReferenceValue = estrellaGO.transform;
        so.FindProperty("solRenderer").objectReferenceValue = solGO.GetComponent<SpriteRenderer>();
        so.FindProperty("estrellaRenderer").objectReferenceValue = estrellaGO.GetComponent<SpriteRenderer>();
        AsignarArray(so.FindProperty("solFrames"), solFrames);
        AsignarArray(so.FindProperty("estrellaFrames"), estrellaFrames);
        so.FindProperty("puerta").objectReferenceValue = puertaSr;
        so.FindProperty("puertaCerrada").objectReferenceValue = puertaCerrada;
        so.FindProperty("puertaAbierta").objectReferenceValue = puertaAbierta;
        so.FindProperty("xInicio").floatValue = 1.4f;
        so.FindProperty("xDestino").floatValue = 9.4f;
        so.FindProperty("ySuelo").floatValue = 1.9f;
        so.FindProperty("separacion").floatValue = 1.15f;
        so.FindProperty("velocidad").floatValue = 1.15f;
        so.FindProperty("fpsAnimacion").floatValue = 8f;
        so.FindProperty("mirandoDerecha").boolValue = true;
        so.FindProperty("esperaAntesDeSalir").floatValue = 2.6f;
        so.FindProperty("siguienteEscena").stringValue = "MenuPrincipal";
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena, RutaFinal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string[] escenas =
        {
            "Assets/Scenes/MenuPrincipal.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
            RutaFinal
        };

        EditorBuildSettings.scenes = escenas
            .Select(ruta =>
            {
                EditorBuildSettingsScene entrada = new EditorBuildSettingsScene(ruta, true);
                entrada.guid = new GUID(AssetDatabase.AssetPathToGUID(ruta));
                return entrada;
            })
            .ToArray();

        AssetDatabase.SaveAssets();

        Debug.Log("CrearEscenaFinal: escena creada en " + RutaFinal);
        Validar();
    }

    [MenuItem("HomePaws/Validar escenas")]
    public static void Validar()
    {
        string[] rutas =
        {
            "Assets/Scenes/MenuPrincipal.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
            RutaFinal
        };

        int totalFaltantes = 0;

        foreach (string ruta in rutas)
        {
            if (!System.IO.File.Exists(ruta))
            {
                Debug.LogError("VALIDACION: falta la escena " + ruta);
                continue;
            }

            Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            int faltantes = 0;

            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                foreach (Component componente in raiz.GetComponentsInChildren<Component>(true))
                {
                    if (componente == null)
                        faltantes++;
                }
            }

            totalFaltantes += faltantes;
            Debug.Log("VALIDACION_ESCENA=" + ruta + " FALTANTES=" + faltantes);
        }

        Debug.Log("VALIDACION_FALTANTES=" + totalFaltantes);

        foreach (EditorBuildSettingsScene entrada in EditorBuildSettings.scenes)
        {
            string guidReal = AssetDatabase.AssetPathToGUID(entrada.path);
            bool coincide = guidReal == entrada.guid.ToString();
            Debug.Log("VALIDACION_BUILD=" + entrada.path + " enabled=" + entrada.enabled + " guid_ok=" + coincide + " existe=" + System.IO.File.Exists(entrada.path));
        }
    }

    private static void AsignarArray(SerializedProperty propiedad, Sprite[] sprites)
    {
        propiedad.arraySize = sprites.Length;

        for (int i = 0; i < sprites.Length; i++)
            propiedad.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }

    private static GameObject CrearGato(string nombre, Sprite sprite, int orden)
    {
        GameObject go = new GameObject(nombre);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
        go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        go.transform.position = new Vector3(1.4f, 1.9f, 0f);
        return go;
    }

    /// <summary>
    /// Escala el fondo para que cubra toda la camara (1920x1080) y lo centra
    /// en ella, sin depender del pivote con el que se importo el sprite.
    /// </summary>
    private static void ColocarFondo(Transform fondo, Sprite sprite)
    {
        if (fondo == null || sprite == null)
            return;

        const float anchoCamara = 18.2044f;
        const float altoCamara = 10.24f;

        Vector2 tamano = sprite.bounds.size;

        if (tamano.x <= 0f || tamano.y <= 0f)
            return;

        float escala = Mathf.Max(anchoCamara / tamano.x, altoCamara / tamano.y);
        fondo.localScale = new Vector3(escala, escala, 1f);
        fondo.position = new Vector3(7.68f, 5.12f, 0f);
    }

    private static Sprite[] Frames(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    private static Sprite PrimerSprite(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .FirstOrDefault();
    }
}
