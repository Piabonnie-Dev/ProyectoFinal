
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioGlobalCOMA : MonoBehaviour
{
    public static AudioGlobalCOMA Instancia { get; private set; }

    [Header("Fuentes de audio")]
    public AudioSource fuenteMusica;
    public AudioSource fuenteUI;
    public AudioSource fuenteEfectos;

    [Header("Nombres de escenas")]
    public string nombreEscenaMenu = "MenuPrincipal";
    public string nombreEscenaMapa = "Mapa";

    [Header("Música")]
    public AudioClip musicaMenu;
    public AudioClip musicaMapa;

    [Range(0f, 1f)]
    public float volumenMusica = 0.45f;

    [Header("Sonidos de interfaz")]
    public AudioClip sonidoSeleccion;
    public AudioClip sonidoCancelar;
    public AudioClip sonidoConfirmar;

    [Range(0f, 1f)]
    public float volumenUI = 0.8f;

    [Header("Sonidos generales")]
    public AudioClip sonidoAtaqueRecibido;
    public AudioClip sonidoMuerte;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoExplosion;

    [Range(0f, 1f)]
    public float volumenEfectos = 1f;

    private void Awake()
    {
        // Evita que aparezcan dos administradores de audio.
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        // Conserva el objeto al cambiar del menú al mapa.
        DontDestroyOnLoad(gameObject);

        PrepararFuentes();

        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void Start()
    {
        CambiarMusicaSegunEscena(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            Instancia = null;
        }
    }

    private void PrepararFuentes()
    {
        if (fuenteMusica == null)
        {
            fuenteMusica = gameObject.AddComponent<AudioSource>();
        }

        if (fuenteUI == null)
        {
            fuenteUI = gameObject.AddComponent<AudioSource>();
        }

        if (fuenteEfectos == null)
        {
            fuenteEfectos = gameObject.AddComponent<AudioSource>();
        }

        fuenteMusica.playOnAwake = false;
        fuenteMusica.loop = true;
        fuenteMusica.spatialBlend = 0f;
        fuenteMusica.volume = volumenMusica;

        fuenteUI.playOnAwake = false;
        fuenteUI.loop = false;
        fuenteUI.spatialBlend = 0f;
        fuenteUI.volume = volumenUI;

        fuenteEfectos.playOnAwake = false;
        fuenteEfectos.loop = false;
        fuenteEfectos.spatialBlend = 0f;
        fuenteEfectos.volume = volumenEfectos;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        CambiarMusicaSegunEscena(escena.name);
    }

    private void CambiarMusicaSegunEscena(string nombreEscena)
    {
        string nombreMinusculas = nombreEscena.ToLower();

        if (nombreEscena == nombreEscenaMenu ||
            nombreMinusculas.Contains("menu"))
        {
            ReproducirMusica(musicaMenu);
            return;
        }

        if (nombreEscena == nombreEscenaMapa ||
            nombreMinusculas.Contains("mapa"))
        {
            ReproducirMusica(musicaMapa);
        }
    }

    public void ReproducirMusica(AudioClip nuevaMusica)
    {
        if (nuevaMusica == null || fuenteMusica == null)
        {
            return;
        }

        // No reinicia la canción si ya es la misma.
        if (fuenteMusica.clip == nuevaMusica &&
            fuenteMusica.isPlaying)
        {
            return;
        }

        fuenteMusica.Stop();
        fuenteMusica.clip = nuevaMusica;
        fuenteMusica.volume = volumenMusica;
        fuenteMusica.loop = true;
        fuenteMusica.Play();
    }

    public void DetenerMusica()
    {
        if (fuenteMusica != null)
        {
            fuenteMusica.Stop();
        }
    }

    public void ReproducirSeleccion()
    {
        ReproducirUI(sonidoSeleccion);
    }

    public void ReproducirCancelar()
    {
        ReproducirUI(sonidoCancelar);
    }

    public void ReproducirConfirmacion()
    {
        ReproducirUI(sonidoConfirmar);
    }

    public void ReproducirAtaqueRecibido()
    {
        ReproducirEfecto(sonidoAtaqueRecibido);
    }

    public void ReproducirMuerte()
    {
        ReproducirEfecto(sonidoMuerte);
    }

    public void ReproducirVictoria()
    {
        ReproducirEfecto(sonidoVictoria);
    }

    public void ReproducirExplosionGlobal()
    {
        ReproducirEfecto(sonidoExplosion);
    }

    public void ReproducirUI(AudioClip clip)
    {
        if (clip == null || fuenteUI == null)
        {
            return;
        }

        fuenteUI.PlayOneShot(clip, volumenUI);
    }

    public void ReproducirEfecto(AudioClip clip)
    {
        if (clip == null || fuenteEfectos == null)
        {
            return;
        }

        fuenteEfectos.PlayOneShot(clip, volumenEfectos);
    }

    public void ReproducirEfecto3D(
        AudioClip clip,
        Vector3 posicion,
        float volumen = 1f)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            clip,
            posicion,
            volumen * volumenEfectos
        );
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);

        if (fuenteMusica != null)
        {
            fuenteMusica.volume = volumenMusica;
        }
    }

    public void CambiarVolumenEfectos(float nuevoVolumen)
    {
        volumenEfectos = Mathf.Clamp01(nuevoVolumen);

        if (fuenteEfectos != null)
        {
            fuenteEfectos.volume = volumenEfectos;
        }
    }

    public void CambiarVolumenUI(float nuevoVolumen)
    {
        volumenUI = Mathf.Clamp01(nuevoVolumen);

        if (fuenteUI != null)
        {
            fuenteUI.volume = volumenUI;
        }
    }
}

