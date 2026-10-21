using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de efecto de sonido del juego. Cada uno corresponde a un archivo
/// en Assets/Resources/Audio/sfx_*.wav.
/// </summary>
public enum Sonido
{
    Salto,
    Aterrizaje,
    Moneda,
    Puerta,
    Checkpoint,
    Meta,
    Boton,
    Pausa
}

/// <summary>
/// Gestor de audio global y persistente. Se crea solo la primera vez que
/// arranca el juego (no hace falta colocarlo en ninguna escena) y sobrevive
/// a los cambios de escena, de forma que la musica no se corta al pasar de
/// nivel.
///
/// Guarda los volumenes en PlayerPrefs, asi que se recuerdan entre partidas.
/// Uso desde cualquier script:  GestorAudio.Sonar(Sonido.Salto);
/// </summary>
[DisallowMultipleComponent]
public class GestorAudio : MonoBehaviour
{
    private const string ClaveMusica = "volumen_musica";
    private const string ClaveEfectos = "volumen_efectos";
    private const string RutaMusica = "Audio/musica_fondo";
    private const int CanalesEfectos = 6;

    private static GestorAudio instancia;

    /// <summary>Instancia activa. Nunca es null: si no existe, se crea.</summary>
    public static GestorAudio Instancia
    {
        get
        {
            if (instancia == null)
                Crear();

            return instancia;
        }
    }

    private AudioSource fuenteMusica;
    private readonly List<AudioSource> canalesEfectos = new List<AudioSource>();
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private float volumenMusica = 0.7f;
    private float volumenEfectos = 0.8f;
    private int siguienteCanal;

    public float VolumenMusica
    {
        get { return volumenMusica; }
    }

    public float VolumenEfectos
    {
        get { return volumenEfectos; }
    }

    /// <summary>Crea el gestor automaticamente al arrancar el juego.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Arrancar()
    {
        if (instancia == null)
            Crear();
    }

    private static void Crear()
    {
        if (instancia != null)
            return;

        GameObject objeto = new GameObject("GestorAudio");
        DontDestroyOnLoad(objeto);
        instancia = objeto.AddComponent<GestorAudio>();
    }

    private void Awake()
    {
        // Si ya habia otra instancia (por ejemplo al recargar el dominio en el
        // editor), esta se descarta para no duplicar la musica.
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        volumenMusica = PlayerPrefs.GetFloat(ClaveMusica, 0.7f);
        volumenEfectos = PlayerPrefs.GetFloat(ClaveEfectos, 0.8f);

        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.playOnAwake = false;
        fuenteMusica.loop = true;
        fuenteMusica.spatialBlend = 0f;
        fuenteMusica.volume = volumenMusica;

        for (int i = 0; i < CanalesEfectos; i++)
        {
            AudioSource canal = gameObject.AddComponent<AudioSource>();
            canal.playOnAwake = false;
            canal.loop = false;
            canal.spatialBlend = 0f;
            canal.volume = volumenEfectos;
            canalesEfectos.Add(canal);
        }

        ReproducirMusica();
    }

    // ------------------------------------------------------------------ musica

    private void ReproducirMusica()
    {
        if (fuenteMusica == null || fuenteMusica.isPlaying)
            return;

        AudioClip clip = Cargar(RutaMusica);

        if (clip == null)
        {
            Debug.LogWarning("GestorAudio: no se encuentra " + RutaMusica + " en Resources.");
            return;
        }

        fuenteMusica.clip = clip;
        fuenteMusica.volume = volumenMusica;
        fuenteMusica.Play();
    }

    // ----------------------------------------------------------------- efectos

    /// <summary>Reproduce un efecto de sonido del catalogo.</summary>
    public static void Sonar(Sonido sonido, float volumen = 1f)
    {
        Instancia.Reproducir(sonido, volumen);
    }

    private void Reproducir(Sonido sonido, float volumen)
    {
        if (volumenEfectos <= 0.001f)
            return;

        AudioClip clip = Cargar(RutaSonido(sonido));

        if (clip == null)
            return;

        AudioSource canal = canalesEfectos[siguienteCanal];
        siguienteCanal = (siguienteCanal + 1) % canalesEfectos.Count;

        canal.PlayOneShot(clip, Mathf.Clamp01(volumen) * volumenEfectos);
    }

    private static string RutaSonido(Sonido sonido)
    {
        switch (sonido)
        {
            case Sonido.Salto: return "Audio/sfx_salto";
            case Sonido.Aterrizaje: return "Audio/sfx_aterrizaje";
            case Sonido.Moneda: return "Audio/sfx_moneda";
            case Sonido.Puerta: return "Audio/sfx_puerta";
            case Sonido.Checkpoint: return "Audio/sfx_checkpoint";
            case Sonido.Meta: return "Audio/sfx_meta";
            case Sonido.Boton: return "Audio/sfx_boton";
            case Sonido.Pausa: return "Audio/sfx_pausa";
            default: return string.Empty;
        }
    }

    private AudioClip Cargar(string ruta)
    {
        if (string.IsNullOrEmpty(ruta))
            return null;

        AudioClip clip;

        if (cache.TryGetValue(ruta, out clip))
            return clip;

        clip = Resources.Load<AudioClip>(ruta);
        cache[ruta] = clip;

        return clip;
    }

    // -------------------------------------------------------------- volumenes

    /// <summary>Ajusta el volumen de la musica (0 a 1) y lo guarda.</summary>
    public static void AjustarVolumenMusica(float volumen)
    {
        Instancia.FijarVolumenMusica(volumen);
    }

    /// <summary>Ajusta el volumen de los efectos (0 a 1) y lo guarda.</summary>
    public static void AjustarVolumenEfectos(float volumen)
    {
        Instancia.FijarVolumenEfectos(volumen);
    }

    private void FijarVolumenMusica(float volumen)
    {
        volumenMusica = Mathf.Clamp01(volumen);

        if (fuenteMusica != null)
            fuenteMusica.volume = volumenMusica;

        PlayerPrefs.SetFloat(ClaveMusica, volumenMusica);
        PlayerPrefs.Save();
    }

    private void FijarVolumenEfectos(float volumen)
    {
        volumenEfectos = Mathf.Clamp01(volumen);

        foreach (AudioSource canal in canalesEfectos)
            canal.volume = volumenEfectos;

        PlayerPrefs.SetFloat(ClaveEfectos, volumenEfectos);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Volumen general 0 a 1: se aplica a musica y efectos a la vez.
    /// Es lo que usa el deslizador del menu de pausa.
    /// </summary>
    public static void AjustarVolumenGeneral(float volumen)
    {
        AjustarVolumenMusica(volumen);
        AjustarVolumenEfectos(volumen);
    }
}
