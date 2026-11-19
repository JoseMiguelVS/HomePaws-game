using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nombreEscenaSiguiente = "MenuPrincipal";
    public TMP_Text textoControles;

    private bool yaCargoEscena = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += AlTerminarVideo;
        }

        ActualizarTexto();
    }

    void Update()
    {
        if (yaCargoEscena || videoPlayer == null) return;

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            AlternarPausa();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            CargarSiguienteEscena();
        }
    }

    public void AlternarPausa()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }

        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoControles == null || videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            textoControles.text = "[P] Pausar   |   [Espacio] Omitir";
        }
        else
        {
            textoControles.text = "[P] Continuar   |   [Espacio] Omitir";
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarSiguienteEscena();
    }

    public void CargarSiguienteEscena()
    {
        if (yaCargoEscena) return;
        yaCargoEscena = true;
        SceneManager.LoadScene(nombreEscenaSiguiente);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= AlTerminarVideo;
    }
}