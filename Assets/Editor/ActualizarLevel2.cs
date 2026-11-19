using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Deja el Nivel 2 con el mismo sistema que el Nivel 1 y el Nivel 3:
/// HUD_Duo (OnGUI) en lugar del Canvas antiguo, sin barra de energia,
/// encuentro Sol-Estrella con dialogo y carteles de indicaciones.
///
/// Se puede ejecutar desde el menu "HomePaws > Actualizar Level2" o en batch:
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ActualizarLevel2.Actualizar
/// </summary>
public static class ActualizarLevel2
{
    private const string RutaEscena = "Assets/Scenes/Level2.unity";

    [MenuItem("HomePaws/Actualizar Level2")]
    public static void Actualizar()
    {
        Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        GameObject sol = Buscar("PlayerSol");
        GameObject estrella = Buscar("PlayerEstrella");
        GameObject trigger = Buscar("TriggerAparicionEstrella");

        if (sol == null || estrella == null || trigger == null)
        {
            Debug.LogError(
                "ActualizarLevel2: faltan PlayerSol, PlayerEstrella o TriggerAparicionEstrella en " +
                RutaEscena + "."
            );
            return;
        }

        BorrarHudHeredado();

        PrepararPersonaje(sol, "Sol", 4.5f, 9f, 4f, 2f, 3f);
        PrepararPersonaje(estrella, "Estrella", 6.2f, 10.5f, 1f, 1.6f, 0.8f);
        LimpiarJugadores(sol, estrella);

        ConfigurarMenuPausa();
        ConfigurarMeta();

        GameObject sistemas = CrearSistemas(sol, estrella);
        ConfigurarEncuentro(trigger, sistemas, estrella);
        CrearIndicadores();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("ActualizarLevel2: Level2 actualizado con HUD_Duo, dialogo e indicaciones.");
    }

    // ------------------------------------------------------------------ busqueda

    private static GameObject Buscar(string nombre)
    {
        foreach (GameObject raiz in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == nombre)
                    return t.gameObject;
            }
        }

        return null;
    }

    // ------------------------------------------------------------ HUD heredado

    private static void BorrarHudHeredado()
    {
        string[] nombres =
        {
            "HUD_Colectibles",
            "energia",
            "PanelNivelCompleto",
            "PanelNarrativaEstrella",
            "BotonPausa"
        };

        foreach (string nombre in nombres)
        {
            GameObject go = Buscar(nombre);

            if (go != null)
            {
                Object.DestroyImmediate(go);
                Debug.Log("ActualizarLevel2: eliminado " + nombre + ".");
            }
        }
    }

    // -------------------------------------------------------------- personajes

    private static void PrepararPersonaje(
        GameObject go,
        string nombre,
        float velocidad,
        float salto,
        float masa,
        float gravedad,
        float empuje)
    {
        PerfilPersonaje perfil = go.GetComponent<PerfilPersonaje>();

        if (perfil == null)
            perfil = go.AddComponent<PerfilPersonaje>();

        SerializedObject so = new SerializedObject(perfil);
        so.FindProperty("nombre").stringValue = nombre;
        so.FindProperty("velocidad").floatValue = velocidad;
        so.FindProperty("salto").floatValue = salto;
        so.FindProperty("masa").floatValue = masa;
        so.FindProperty("escalaGravedad").floatValue = gravedad;
        so.FindProperty("tamanoCollider").vector2Value = Vector2.zero;
        so.FindProperty("offsetCollider").vector2Value = Vector2.zero;
        so.FindProperty("fuerzaEmpuje").floatValue = empuje;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (go.GetComponent<CompaneroSeguir>() == null)
            go.AddComponent<CompaneroSeguir>();
    }

    private static void LimpiarJugadores(GameObject sol, GameObject estrella)
    {
        foreach (GameObject go in new[] { sol, estrella })
        {
            PlayerController control = go.GetComponent<PlayerController>();

            if (control == null)
                continue;

            SerializedObject so = new SerializedObject(control);
            so.FindProperty("textAtun").objectReferenceValue = null;
            so.FindProperty("groundCheckRadius").floatValue = 0.4f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ------------------------------------------------------------- menu y meta

    private static void ConfigurarMenuPausa()
    {
        MenuPausa menu = Object.FindFirstObjectByType<MenuPausa>(FindObjectsInactive.Include);

        if (menu == null)
        {
            Debug.LogWarning("ActualizarLevel2: no se encontro MenuPausa.");
            return;
        }

        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("mostrarMenuAlIniciar").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigurarMeta()
    {
        OnTriggerFin meta = Object.FindFirstObjectByType<OnTriggerFin>(FindObjectsInactive.Include);

        if (meta == null)
        {
            Debug.LogWarning("ActualizarLevel2: no se encontro OnTriggerFin.");
            return;
        }

        SerializedObject so = new SerializedObject(meta);
        so.FindProperty("m_gameObject").objectReferenceValue = null;
        so.FindProperty("m_nextSceneName").stringValue = "Level3";
        so.FindProperty("mensajeFinal").stringValue = "Rumbo a los tejados. El Nivel 3 espera.";
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ----------------------------------------------------------------- sistemas

    private static GameObject CrearSistemas(GameObject sol, GameObject estrella)
    {
        GameObject anterior = Buscar("Sistemas");

        if (anterior != null)
            Object.DestroyImmediate(anterior);

        GameObject sistemas = new GameObject("Sistemas");

        GameObject controlGO = new GameObject("ControlDuo");
        controlGO.transform.SetParent(sistemas.transform);
        ControlDuo control = controlGO.AddComponent<ControlDuo>();

        GameObject dialogoGO = new GameObject("DialogoUI");
        dialogoGO.transform.SetParent(sistemas.transform);
        dialogoGO.AddComponent<DialogoUI>();

        GameObject hudGO = new GameObject("HUD_Duo");
        hudGO.transform.SetParent(sistemas.transform);
        HUD_Duo hud = hudGO.AddComponent<HUD_Duo>();

        CameraFollow camara = Object.FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Include);
        MenuPausa menu = Object.FindFirstObjectByType<MenuPausa>(FindObjectsInactive.Include);

        SerializedObject so = new SerializedObject(control);
        so.FindProperty("sol").objectReferenceValue = sol.GetComponent<PlayerController>();
        so.FindProperty("estrella").objectReferenceValue = estrella.GetComponent<PlayerController>();
        so.FindProperty("camara").objectReferenceValue = camara;
        so.FindProperty("empezarConDuo").boolValue = false;
        so.FindProperty("companeroSigue").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        so = new SerializedObject(hud);
        so.FindProperty("controlDuo").objectReferenceValue = control;
        so.FindProperty("menuPausa").objectReferenceValue = menu;
        so.FindProperty("objetivoSinDuo").stringValue = "Recoge los atunes y llega a la salida.";
        so.FindProperty("objetivoEncuentro").stringValue = "Encuentra a Estrella al final del barrio.";
        so.FindProperty("habilidadSinDuo").stringValue = "Empuja objetos pesados";
        so.FindProperty("habilidadSol").stringValue = "Empuja objetos pesados y mueve palancas";
        so.FindProperty("habilidadEstrella").stringValue = "Salto alto: alcanza la llave";
        so.ApplyModifiedPropertiesWithoutUndo();

        return sistemas;
    }

    // --------------------------------------------------------------- encuentro

    private static void ConfigurarEncuentro(GameObject trigger, GameObject sistemas, GameObject estrella)
    {
        SpawnEstrellaTrigger viejo = trigger.GetComponent<SpawnEstrellaTrigger>();

        if (viejo != null)
            Object.DestroyImmediate(viejo);

        EncuentroEstrella encuentro = trigger.GetComponent<EncuentroEstrella>();

        if (encuentro == null)
            encuentro = trigger.AddComponent<EncuentroEstrella>();

        SerializedObject so = new SerializedObject(encuentro);
        so.FindProperty("controlDuo").objectReferenceValue = sistemas.GetComponentInChildren<ControlDuo>();
        so.FindProperty("estrella").objectReferenceValue = estrella;
        so.FindProperty("colocarJuntoAlJugador").boolValue = true;
        so.FindProperty("desplazamientoAparicion").vector3Value = new Vector3(1.5f, 0.15f, 0f);
        so.FindProperty("sonidoEncuentro").objectReferenceValue = trigger.GetComponent<AudioSource>();
        so.FindProperty("mensajeFinal").stringValue =
            "Estrella se uni\u00f3 al equipo. Pulsa TAB para cambiar de personaje.";

        ConfigurarLineas(
            so,
            new[] { "Sol", "Estrella", "Sol", "Estrella" },
            new[]
            {
                "Estrella, te encontr\u00e9. \u00bfEst\u00e1s bien?",
                "\u00a1Sol! Te estaba esperando. La salida a los tejados est\u00e1 al final del barrio.",
                "Entonces vamos juntos: yo empujo lo que estorbe.",
                "Y yo salto m\u00e1s alto. \u00a1No me pierdas de vista!"
            }
        );

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigurarLineas(SerializedObject so, string[] hablantes, string[] lineas)
    {
        SerializedProperty propiedadHablantes = so.FindProperty("hablantes");
        propiedadHablantes.arraySize = hablantes.Length;

        for (int i = 0; i < hablantes.Length; i++)
            propiedadHablantes.GetArrayElementAtIndex(i).stringValue = hablantes[i];

        SerializedProperty propiedadLineas = so.FindProperty("lineas");
        propiedadLineas.arraySize = lineas.Length;

        for (int i = 0; i < lineas.Length; i++)
            propiedadLineas.GetArrayElementAtIndex(i).stringValue = lineas[i];
    }

    // ------------------------------------------------------------- indicaciones

    private static void CrearIndicadores()
    {
        CrearIndicador(
            "Indicador_Atunes",
            new Vector3(2.5f, 0.2f, 0f),
            "RECOGE LOS ATUNES",
            new Color(1f, 0.68f, 0.22f, 1f),
            2f,
            260f
        );

        CrearIndicador(
            "Indicador_Encuentro",
            new Vector3(26.48f, -0.73f, 0f),
            "ENCUENTRO CON ESTRELLA",
            new Color(0.4f, 0.85f, 1f, 1f),
            2.4f,
            300f
        );

        CrearIndicador(
            "Indicador_Salida",
            new Vector3(122.28f, -12.61f, 0f),
            "SALIDA A LOS TEJADOS",
            new Color(0.45f, 0.95f, 0.55f, 1f),
            2.8f,
            280f
        );
    }

    private static void CrearIndicador(
        string nombre,
        Vector3 posicion,
        string texto,
        Color color,
        float altura,
        float ancho)
    {
        GameObject anterior = Buscar(nombre);

        if (anterior != null)
            Object.DestroyImmediate(anterior);

        GameObject go = new GameObject(nombre);
        go.transform.position = posicion;

        IndicadorFlotante indicador = go.AddComponent<IndicadorFlotante>();

        SerializedObject so = new SerializedObject(indicador);
        so.FindProperty("texto").stringValue = texto;
        so.FindProperty("color").colorValue = color;
        so.FindProperty("altura").floatValue = altura;
        so.FindProperty("ancho").floatValue = ancho;
        so.FindProperty("tamanoTexto").intValue = 19;
        so.FindProperty("distanciaMaxima").floatValue = 20f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
