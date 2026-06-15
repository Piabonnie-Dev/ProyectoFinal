
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEnemigoCOMA : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyV19AI enemigoV19;
    public EnemyV37AI enemigoV37;
    public EnemyHealth vida;
    public AudioSource fuente;

    [Header("Voces")]
    public AudioClip[] vocesAmbiente;
    public AudioClip sonidoDeteccion;
    public AudioClip sonidoPersecucion;
    public AudioClip sonidoCongelado;
    public AudioClip sonidoMuerte;

    [Header("Ambiente aleatorio")]
    public float intervaloMinimo = 7f;
    public float intervaloMaximo = 14f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    public float volumenVoces = 0.85f;

    private string estadoAnterior;
    private float siguienteVoz;
    private bool muerteReproducida;

    private void Awake()
    {
        if (enemigoV19 == null)
        {
            enemigoV19 = GetComponent<EnemyV19AI>();
        }

        if (enemigoV37 == null)
        {
            enemigoV37 = GetComponent<EnemyV37AI>();
        }

        if (vida == null)
        {
            vida = GetComponent<EnemyHealth>();
        }

        if (fuente == null)
        {
            fuente = GetComponent<AudioSource>();
        }

        fuente.playOnAwake = false;
        fuente.loop = false;

        // Audio 3D para saber desde dónde viene el enemigo.
        fuente.spatialBlend = 1f;
        fuente.minDistance = 2f;
        fuente.maxDistance = 28f;
        fuente.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void Start()
    {
        estadoAnterior = ObtenerEstado();
        ProgramarSiguienteVoz();
    }

    private void Update()
    {
        if (vida != null && vida.estaMuerto)
        {
            ReproducirMuerte();
            return;
        }

        string estadoActual = ObtenerEstado();

        if (estadoActual != estadoAnterior)
        {
            ProcesarCambioEstado(
                estadoAnterior,
                estadoActual
            );

            estadoAnterior = estadoActual;
        }

        bool estaPersiguiendo =
            estadoActual.Contains("Persecucion") ||
            estadoActual.Contains("Acecho");

        bool estaCongelado =
            estadoActual.Contains("Congelado");

        if (!estaPersiguiendo &&
            !estaCongelado &&
            Time.time >= siguienteVoz)
        {
            ReproducirVozAleatoria();
            ProgramarSiguienteVoz();
        }
    }

    private string ObtenerEstado()
    {
        if (enemigoV19 != null)
        {
            return enemigoV19.estadoActual.ToString();
        }

        if (enemigoV37 != null)
        {
            return enemigoV37.estadoActual.ToString();
        }

        return "SinEstado";
    }

    private void ProcesarCambioEstado(
        string estadoViejo,
        string estadoNuevo)
    {
        if (estadoNuevo.Contains("Muerto"))
        {
            ReproducirMuerte();
            return;
        }

        if (estadoNuevo.Contains("Acecho"))
        {
            Reproducir(
                sonidoDeteccion,
                volumenVoces
            );

            return;
        }

        if (estadoNuevo.Contains("Persecucion"))
        {
            Reproducir(
                sonidoPersecucion,
                volumenVoces
            );

            return;
        }

        if (estadoNuevo.Contains("Congelado"))
        {
            Reproducir(
                sonidoCongelado,
                volumenVoces
            );
        }
    }

    private void ReproducirVozAleatoria()
    {
        if (vocesAmbiente == null ||
            vocesAmbiente.Length == 0)
        {
            return;
        }

        AudioClip clip =
            vocesAmbiente[
                Random.Range(0, vocesAmbiente.Length)
            ];

        Reproducir(clip, volumenVoces);
    }

    private void ReproducirMuerte()
    {
        if (muerteReproducida)
        {
            return;
        }

        muerteReproducida = true;

        Reproducir(
            sonidoMuerte,
            volumenVoces
        );
    }

    private void Reproducir(
        AudioClip clip,
        float volumen)
    {
        if (clip == null || fuente == null)
        {
            return;
        }

        fuente.pitch = Random.Range(0.96f, 1.04f);
        fuente.PlayOneShot(clip, volumen);
    }

    private void ProgramarSiguienteVoz()
    {
        siguienteVoz =
            Time.time +
            Random.Range(
                intervaloMinimo,
                intervaloMaximo
            );
    }
}
