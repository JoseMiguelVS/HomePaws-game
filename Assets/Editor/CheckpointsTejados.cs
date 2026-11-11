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
/// Anade checkpoints (puntos de reaparicion) a la seccion de los tejados del
/// Nivel 3. Detecta las superficies solidas del tilemap y coloca copias del
/// checkpoint existente encima, ademas de registrarlas en GestorReaparicion.
///
/// Menu: "HomePaws > Herramientas > Checkpoints en los tejados"
/// Batch:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod CheckpointsTejados.DiagnosticoBatch
///   Unity.exe -batchmode -quit -projectPath . -executeMethod CheckpointsTejados.AplicarBatch
/// </summary>
public static class CheckpointsTejados
{
    private const string Escena = "Assets/Scenes/Level3.unity";
    private const string NombreModelo = "Checkpoint";
    private const string Prefijo = "Checkpoint_Tejados_";

    private static readonly float XMin = 82f;
    private static readonly float XMax = 188f;
    private static readonly float YMin = 20f;
    private static readonly float YMax = 45f;

    private const int LongitudMinima = 5;

    private class Plataforma
    {
        public int CeldaInicio;
        public int CeldaFin;
        public float SuperficieY;

        public float CentroX
        {
            get { return (CeldaInicio + CeldaFin) * 0.5f; }
        }

        public int Longitud
        {
            get { return CeldaFin - CeldaInicio + 1; }
        }
    }

    [MenuItem("HomePaws/Herramientas/Checkpoints en los tejados")]
    public static void EjecutarMenu()
    {
        Ejecutar(true);
        EditorUtility.DisplayDialog(
            "Checkpoints en los tejados",
            "Checkpoints revisados en el Nivel 3. Mira la consola para ver las posiciones.",
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
            Debug.LogError("CheckpointsTejados: " + e);
            EditorApplication.Exit(1);
        }
    }

    public static void Ejecutar(bool aplicar)
    {
        Scene escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);

        Tilemap mapa = BuscarTilemap();

        if (mapa == null)
            throw new Exception("No se encontro un Tilemap con collider en " + Escena);

        GameObject modelo = BuscarRaiz(escena, NombreModelo);

        if (modelo == null)
            throw new Exception("No se encontro el objeto '" + NombreModelo + "' en " + Escena);

        float desfase = CalcularDesfase(mapa, modelo);

        Debug.Log("CheckpointsTejados: tilemap='" + mapa.name + "' desfase del checkpoint=" + desfase.ToString("0.###"));

        List<Plataforma> plataformas = DetectarPlataformas(mapa);

        EscribirResumen(mapa, plataformas);
        EscribirPerfil(mapa);

        List<Plataforma> elegidas = ElegirPlataformas(plataformas);

        if (elegidas.Count == 0)
        {
            Debug.LogWarning("CheckpointsTejados: no hay plataformas validas en los tejados.");
            return;
        }

        foreach (Plataforma p in elegidas)
            Debug.Log("CheckpointsTejados: elegida x=" + p.CentroX.ToString("0.##") +
                      " superficieY=" + p.SuperficieY.ToString("0.##") +
                      " longitud=" + p.Longitud);

        if (!aplicar)
        {
            Debug.Log("CheckpointsTejados: diagnostico sin cambios.");
            return;
        }

        BorrarCheckpointsAnteriores(escena);

        List<Checkpoint> nuevos = new List<Checkpoint>();

        for (int i = 0; i < elegidas.Count; i++)
        {
            Plataforma p = elegidas[i];

            float x = p.CentroX;
            float y = p.SuperficieY + desfase;

            GameObject copia = UnityEngine.Object.Instantiate(modelo, new Vector3(x, y, 0f), Quaternion.identity);
            copia.name = Prefijo + (i + 1);
            copia.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(copia, escena);

            Checkpoint punto = copia.GetComponent<Checkpoint>();

            if (punto == null)
                throw new Exception("La copia del checkpoint no tiene el componente Checkpoint.");

            nuevos.Add(punto);

            Debug.Log("CheckpointsTejados: creado " + copia.name + " en x=" + x.ToString("0.##") +
                      " y=" + y.ToString("0.##"));
        }

        RegistrarEnGestor(nuevos);

        EditorSceneManager.MarkSceneDirty(escena);
        bool guardada = EditorSceneManager.SaveScene(escena);

        Debug.Log("CheckpointsTejados: " + nuevos.Count + " checkpoints creados, guardada=" + guardada);
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
    /// Diferencia entre la altura del checkpoint existente y la superficie del
    /// tilemap bajo el, para colocar los nuevos a la misma altura relativa.
    /// </summary>
    private static float CalcularDesfase(Tilemap mapa, GameObject modelo)
    {
        float superficie = SuperficieEn(mapa, modelo.transform.position.x);

        if (float.IsNaN(superficie))
            return 0.45f;

        return modelo.transform.position.y - superficie;
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

    private static List<Plataforma> DetectarPlataformas(Tilemap mapa)
    {
        List<Plataforma> plataformas = new List<Plataforma>();

        int desdeX = mapa.WorldToCell(new Vector3(XMin, 0f, 0f)).x;
        int hastaX = mapa.WorldToCell(new Vector3(XMax, 0f, 0f)).x;

        Plataforma actual = null;

        for (int x = desdeX; x <= hastaX; x++)
        {
            float superficie = SuperficieEn(mapa, mapa.GetCellCenterWorld(new Vector3Int(x, 0, 0)).x);

            if (float.IsNaN(superficie))
            {
                Cerrar(plataformas, ref actual);
                continue;
            }

            if (actual == null || Mathf.Abs(actual.SuperficieY - superficie) > 0.05f)
            {
                Cerrar(plataformas, ref actual);
                actual = new Plataforma
                {
                    CeldaInicio = x,
                    CeldaFin = x,
                    SuperficieY = superficie
                };
            }
            else
            {
                actual.CeldaFin = x;
            }
        }

        Cerrar(plataformas, ref actual);

        return plataformas;
    }

    private static void Cerrar(List<Plataforma> lista, ref Plataforma actual)
    {
        if (actual != null && actual.Longitud >= LongitudMinima)
            lista.Add(actual);

        actual = null;
    }

    private static void EscribirResumen(Tilemap mapa, List<Plataforma> plataformas)
    {
        StringBuilder texto = new StringBuilder();
        texto.AppendLine("CheckpointsTejados: plataformas detectadas en los tejados (" + plataformas.Count + ")");

        foreach (Plataforma p in plataformas)
        {
            float xInicio = mapa.GetCellCenterWorld(new Vector3Int(p.CeldaInicio, 0, 0)).x;
            float xFin = mapa.GetCellCenterWorld(new Vector3Int(p.CeldaFin, 0, 0)).x;

            texto.AppendLine("  x=" + xInicio.ToString("0.##") + ".." + xFin.ToString("0.##") +
                             " y=" + p.SuperficieY.ToString("0.##") +
                             " largo=" + p.Longitud);
        }

        Debug.Log(texto.ToString());
    }

    /// <summary>
    /// Vuelca las celdas solidas por columna para revisar la forma del tejado.
    /// </summary>
    private static void EscribirPerfil(Tilemap mapa)
    {
        StringBuilder texto = new StringBuilder();
        texto.AppendLine("CheckpointsTejados: perfil solido (x: celdas y)");

        int desdeX = mapa.WorldToCell(new Vector3(XMin, 0f, 0f)).x;
        int hastaX = mapa.WorldToCell(new Vector3(XMax, 0f, 0f)).x;
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

    /// <summary>
    /// Elige como maximo una plataforma por tramo, prefiriendo las mas largas
    /// para que el checkpoint quede en una zona comoda de tocar.
    /// </summary>
    private static List<Plataforma> ElegirPlataformas(List<Plataforma> plataformas)
    {
        List<Plataforma> elegidas = new List<Plataforma>();

        AnadirMejor(plataformas, elegidas, 85f, 135f);
        AnadirMejor(plataformas, elegidas, 135f, 185f);

        return elegidas;
    }

    private static void AnadirMejor(List<Plataforma> plataformas, List<Plataforma> elegidas, float desde, float hasta)
    {
        Plataforma mejor = null;

        foreach (Plataforma p in plataformas)
        {
            if (p.CentroX < desde || p.CentroX >= hasta)
                continue;

            if (mejor == null || p.Longitud > mejor.Longitud)
                mejor = p;
        }

        if (mejor != null)
            elegidas.Add(mejor);
    }

    private static void BorrarCheckpointsAnteriores(Scene escena)
    {
        List<GameObject> sobrantes = new List<GameObject>();

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name.StartsWith(Prefijo, StringComparison.Ordinal))
                sobrantes.Add(raiz);
        }

        foreach (GameObject sobrante in sobrantes)
        {
            Debug.Log("CheckpointsTejados: se elimina " + sobrante.name + " para volver a crearlo.");
            UnityEngine.Object.DestroyImmediate(sobrante);
        }
    }

    private static void RegistrarEnGestor(List<Checkpoint> nuevos)
    {
        GestorReaparicion gestor = UnityEngine.Object.FindFirstObjectByType<GestorReaparicion>(FindObjectsInactive.Include);

        if (gestor == null)
        {
            Debug.LogWarning("CheckpointsTejados: no hay GestorReaparicion; los checkpoints se registran al tocarlos.");
            return;
        }

        SerializedObject serializado = new SerializedObject(gestor);
        SerializedProperty lista = serializado.FindProperty("checkpoints");

        if (lista == null)
        {
            Debug.LogWarning("CheckpointsTejados: GestorReaparicion no tiene la lista 'checkpoints'.");
            return;
        }

        foreach (Checkpoint punto in nuevos)
        {
            if (YaEstaEnLista(lista, punto))
                continue;

            lista.arraySize++;
            lista.GetArrayElementAtIndex(lista.arraySize - 1).objectReferenceValue = punto;
        }

        serializado.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(gestor);
    }

    private static bool YaEstaEnLista(SerializedProperty lista, Checkpoint punto)
    {
        for (int i = 0; i < lista.arraySize; i++)
        {
            if (lista.GetArrayElementAtIndex(i).objectReferenceValue == punto)
                return true;
        }

        return false;
    }
}
#endif
