#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Mecanica del parque para el Nivel 2: coloca tres interruptores de piedra en
/// las plataformas del inicio y un camino de losas que aparece al encenderlos
/// todos. Usa los sprites de "Assets/sprites/items/park" y detecta el suelo del
/// tilemap, de modo que todo queda apoyado antes de la aparicion de Estrella.
///
/// Menu: "HomePaws > Herramientas > Mecanica del parque (Nivel 2)"
/// Batch:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod MecanicaParque.DiagnosticoBatch
///   Unity.exe -batchmode -quit -projectPath . -executeMethod MecanicaParque.AplicarBatch
/// </summary>
public static class MecanicaParque
{
    private const string Escena = "Assets/Scenes/Level2.unity";
    private const string CarpetaSprites = "Assets/sprites/items/park/";

    private const string Raiz = "MecanicaParque";
    private const string PrefijoInterruptor = "Interruptor_Parque_";
    private const string PrefijoLosa = "Losa_Parque_";

    // Interruptores sobre las plataformas del inicio, antes del trigger de
    // Estrella (x = 24.11): 3..5, 8..10 y 12..13. El primero va un poco a la
    // derecha (4.3) para que el jugador, que aparece en x = 3.06, no lo pise al
    // empezar: hay que caminar hasta el.
    private static readonly float[] ObjetivosInterruptores = { 4.3f, 9f, 12.5f };

    // Losas que rellenan los huecos mas grandes del tramo inicial.
    private static readonly float[] ObjetivosLosas = { 2f, 6.5f, 11f, 16f, 19f };

    private const float EscalaInterruptor = 0.45f;
    private const float EscalaLosa = 0.55f;
    private const int OrdenDibujo = 9;

    private const float YMin = -30f;
    private const float YMax = 15f;

    [MenuItem("HomePaws/Herramientas/Mecanica del parque (Nivel 2)")]
    public static void EjecutarMenu()
    {
        Ejecutar(true);

        EditorUtility.DisplayDialog(
            "Mecanica del parque",
            "Interruptores y camino de piedra revisados en el Nivel 2. Mira la consola.",
            "Vale"
        );
    }

    public static void DiagnosticoBatch()
    {
        EjecutarConSalida(false);
    }

    public static void AplicarBatch()
    {
        EjecutarConSalida(true);
    }

    private static void EjecutarConSalida(bool aplicar)
    {
        try
        {
            Ejecutar(aplicar);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("MecanicaParque: " + e);
            EditorApplication.Exit(1);
        }
    }

    public static void Ejecutar(bool aplicar)
    {
        Scene escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);

        Tilemap mapa = BuscarTilemap();

        if (mapa == null)
            throw new Exception("No se encontro un Tilemap con collider en " + Escena);

        EscribirPerfil(mapa);

        List<float> posiciones = new List<float>();
        List<float> superficies = new List<float>();

        foreach (float objetivo in ObjetivosInterruptores)
        {
            float x = BuscarConSuelo(mapa, objetivo, 1.5f);
            float superficie = SuperficieEn(mapa, x);

            if (float.IsNaN(superficie))
                throw new Exception("No hay suelo cerca de x=" + objetivo + " para un interruptor.");

            posiciones.Add(x);
            superficies.Add(superficie);
            Debug.Log("MecanicaParque: interruptor en x=" + x.ToString("0.##") +
                      " superficieY=" + superficie.ToString("0.##"));
        }

        List<float> losasY = new List<float>();

        foreach (float x in ObjetivosLosas)
        {
            float izquierda = SuperficieEn(mapa, x - 1.5f);
            float derecha = SuperficieEn(mapa, x + 1.5f);

            if (float.IsNaN(izquierda) && float.IsNaN(derecha))
                throw new Exception("No hay plataformas junto a la losa x=" + x);

            // La losa queda al nivel de la plataforma mas baja para que sea un
            // escalon comodo y nunca tape el salto.
            float superficie = float.IsNaN(izquierda) ? derecha :
                               float.IsNaN(derecha) ? izquierda :
                               Mathf.Min(izquierda, derecha);

            losasY.Add(superficie);
            Debug.Log("MecanicaParque: losa en x=" + x.ToString("0.##") +
                      " superficieY=" + superficie.ToString("0.##"));
        }

        if (!aplicar)
        {
            Debug.Log("MecanicaParque: diagnostico sin cambios.");
            return;
        }

        Sprite interruptorApagado = CargarSprite("stone_switch_off.png");
        Sprite interruptorEncendido = CargarSprite("stone_switch_on.png");
        Sprite losa = CargarSprite("stone_block.png");

        BorrarAnteriores(escena);

        GameObject raiz = new GameObject(Raiz);
        SceneManager.MoveGameObjectToScene(raiz, escena);

        GestorParque gestor = raiz.AddComponent<GestorParque>();

        List<InterruptorParque> interruptores = new List<InterruptorParque>();

        for (int i = 0; i < posiciones.Count; i++)
        {
            float x = posiciones[i];
            float superficie = superficies[i];

            GameObject objeto = new GameObject(PrefijoInterruptor + (i + 1));
            objeto.transform.SetParent(raiz.transform, false);

            float altura = interruptorApagado.bounds.size.y * EscalaInterruptor;

            objeto.transform.position = new Vector3(x, superficie + altura * 0.5f, 0f);
            objeto.transform.localScale = Vector3.one * EscalaInterruptor;

            SpriteRenderer sprite = objeto.AddComponent<SpriteRenderer>();
            sprite.sprite = interruptorApagado;
            sprite.sortingOrder = OrdenDibujo;

            BoxCollider2D collider = objeto.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 2.2f);
            collider.offset = new Vector2(0f, 0.5f);

            InterruptorParque interruptor = objeto.AddComponent<InterruptorParque>();

            SerializedObject serializado = new SerializedObject(interruptor);
            serializado.FindProperty("spriteRenderer").objectReferenceValue = sprite;
            serializado.FindProperty("spriteApagado").objectReferenceValue = interruptorApagado;
            serializado.FindProperty("spriteEncendido").objectReferenceValue = interruptorEncendido;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            interruptores.Add(interruptor);
        }

        List<GameObject> losas = new List<GameObject>();

        for (int i = 0; i < ObjetivosLosas.Length; i++)
        {
            float x = ObjetivosLosas[i];
            float superficie = losasY[i];

            GameObject objeto = new GameObject(PrefijoLosa + (i + 1));
            objeto.transform.SetParent(raiz.transform, false);

            float altura = losa.bounds.size.y * EscalaLosa;

            objeto.transform.position = new Vector3(x, superficie - altura * 0.5f, 0f);
            objeto.transform.localScale = Vector3.one * EscalaLosa;

            SpriteRenderer sprite = objeto.AddComponent<SpriteRenderer>();
            sprite.sprite = losa;
            sprite.sortingOrder = OrdenDibujo;

            BoxCollider2D collider = objeto.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;

            losas.Add(objeto);

            // El camino solo aparece cuando el jugador enciende los interruptores.
            objeto.SetActive(false);
        }

        SerializedObject serializadoGestor = new SerializedObject(gestor);

        SerializedProperty listaInterruptores = serializadoGestor.FindProperty("interruptores");
        listaInterruptores.arraySize = interruptores.Count;

        for (int i = 0; i < interruptores.Count; i++)
            listaInterruptores.GetArrayElementAtIndex(i).objectReferenceValue = interruptores[i];

        SerializedProperty listaObjetos = serializadoGestor.FindProperty("objetosAlCompletar");
        listaObjetos.arraySize = losas.Count;

        for (int i = 0; i < losas.Count; i++)
            listaObjetos.GetArrayElementAtIndex(i).objectReferenceValue = losas[i];

        serializadoGestor.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(gestor);
        EditorSceneManager.MarkSceneDirty(escena);
        bool guardada = EditorSceneManager.SaveScene(escena);

        Debug.Log("MecanicaParque: " + interruptores.Count + " interruptores y " +
                  losas.Count + " losas creados, guardada=" + guardada);
    }

    private static Tilemap BuscarTilemap()
    {
        Tilemap[] mapas = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

        foreach (Tilemap mapa in mapas)
        {
            if (mapa.GetComponent<TilemapCollider2D>() != null)
                return mapa;
        }

        return mapas.Length > 0 ? mapas[0] : null;
    }

    /// <summary>Busca en x la columna con suelo mas cercana al objetivo.</summary>
    private static float BuscarConSuelo(Tilemap mapa, float objetivo, float radio)
    {
        float mejor = objetivo;
        float mejorDistancia = float.MaxValue;

        for (float desplazamiento = 0f; desplazamiento <= radio; desplazamiento += 0.25f)
        {
            foreach (float signo in new[] { 1f, -1f })
            {
                float x = objetivo + desplazamiento * signo;

                if (float.IsNaN(SuperficieEn(mapa, x)))
                    continue;

                if (desplazamiento < mejorDistancia)
                {
                    mejorDistancia = desplazamiento;
                    mejor = x;
                }
            }

            if (mejorDistancia < float.MaxValue)
                break;
        }

        return mejor;
    }

    private static float SuperficieEn(Tilemap mapa, float mundoX)
    {
        int celdaX = mapa.WorldToCell(new Vector3(mundoX, 0f, 0f)).x;

        for (int celdaY = mapa.WorldToCell(new Vector3(0f, YMax, 0f)).y;
             celdaY >= mapa.WorldToCell(new Vector3(0f, YMin, 0f)).y;
             celdaY--)
        {
            Vector3Int celda = new Vector3Int(celdaX, celdaY, 0);

            if (!EsSolido(mapa, celda))
                continue;

            if (EsSolido(mapa, celda + Vector3Int.up) ||
                EsSolido(mapa, celda + Vector3Int.up * 2))
                continue;

            return mapa.GetCellCenterWorld(celda).y + mapa.cellSize.y * 0.5f;
        }

        return float.NaN;
    }

    private static bool EsSolido(Tilemap mapa, Vector3Int celda)
    {
        if (mapa.GetTile(celda) == null)
            return false;

        return mapa.GetColliderType(celda) != Tile.ColliderType.None;
    }

    private static void EscribirPerfil(Tilemap mapa)
    {
        StringBuilder texto = new StringBuilder();
        texto.AppendLine("MecanicaParque: perfil solido del inicio del Nivel 2 (x: celdas y)");

        int desdeX = mapa.WorldToCell(new Vector3(-1f, 0f, 0f)).x;
        int hastaX = mapa.WorldToCell(new Vector3(26f, 0f, 0f)).x;
        int desdeY = mapa.WorldToCell(new Vector3(0f, YMin, 0f)).y;
        int hastaY = mapa.WorldToCell(new Vector3(0f, YMax, 0f)).y;

        for (int x = desdeX; x <= hastaX; x++)
        {
            List<int> solidas = new List<int>();

            for (int y = desdeY; y <= hastaY; y++)
            {
                if (EsSolido(mapa, new Vector3Int(x, y, 0)))
                    solidas.Add(y);
            }

            float mundoX = mapa.GetCellCenterWorld(new Vector3Int(x, 0, 0)).x;
            texto.Append("  x=" + mundoX.ToString("0.##") + ":");

            foreach (int y in solidas)
                texto.Append(" " + y);

            texto.AppendLine();
        }

        Debug.Log(texto.ToString());
    }

    private static Sprite CargarSprite(string archivo)
    {
        string ruta = CarpetaSprites + archivo;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);

        if (sprite == null)
        {
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }

        // Los PNG de la carpeta llegan sin configurar y Unity los puede trocear
        // en varias sub-imagenes con el pivote abajo a la izquierda. Para usarlos
        // como objetos del nivel conviene una sola imagen centrada.
        AsegurarSpriteCompleto(ruta);

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);

        if (sprite == null)
            throw new Exception("No se pudo cargar el sprite " + ruta);

        return sprite;
    }

    private static void AsegurarSpriteCompleto(string ruta)
    {
        TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;

        if (importador == null)
            return;

        bool cambiar = false;

        if (importador.textureType != TextureImporterType.Sprite)
        {
            importador.textureType = TextureImporterType.Sprite;
            cambiar = true;
        }

        TextureImporterSettings ajustes = new TextureImporterSettings();
        importador.ReadTextureSettings(ajustes);

        if (ajustes.spriteMode != (int)SpriteImportMode.Single)
        {
            ajustes.spriteMode = (int)SpriteImportMode.Single;
            cambiar = true;
        }

        if (ajustes.spriteAlignment != (int)SpriteAlignment.Center)
        {
            ajustes.spriteAlignment = (int)SpriteAlignment.Center;
            cambiar = true;
        }

        if (Mathf.Abs(ajustes.spritePixelsPerUnit - 100f) > 0.001f)
        {
            ajustes.spritePixelsPerUnit = 100f;
            cambiar = true;
        }

        if (!cambiar)
            return;

        Debug.Log("MecanicaParque: ajustando importacion de " + ruta);
        importador.SetTextureSettings(ajustes);
        importador.SaveAndReimport();
    }

    private static void BorrarAnteriores(Scene escena)
    {
        List<GameObject> sobrantes = new List<GameObject>();

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name == Raiz)
                sobrantes.Add(raiz);
        }

        foreach (GameObject sobrante in sobrantes)
        {
            Debug.Log("MecanicaParque: se elimina " + sobrante.name + " para volver a crearlo.");
            UnityEngine.Object.DestroyImmediate(sobrante);
        }
    }
}
#endif
