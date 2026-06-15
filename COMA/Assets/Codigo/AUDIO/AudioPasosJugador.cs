

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPasosJugador : MonoBehaviour
{
    [Header("Referencias")]
    public Mover mover;
    public CharacterController controlador;
    public AudioSource fuentePasos;

    [Header("Clips")]
    public AudioClip[] sonidosPasos;
    public AudioClip sonidoSalto;
    public AudioClip sonidoAterrizaje;

    [Header("Intervalos")]
    public float intervaloCaminando = 0.46f;
    public float intervaloCorriendo = 0.29f;
    public float intervaloAgachado = 0.65f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    public float volumenPasos = 0.7f;

    [Range(0f, 1f)]
    public float volumenSalto = 0.65f;

    [Range(0f, 1f)]
    public float volumenAterrizaje = 0.8f;

    [Header("Variación")]
    public float pitchMinimo = 0.92f;
    public float pitchMaximo = 1.08f;

    private float siguientePaso;
    private bool estabaEnSuelo;
    private int ultimoIndice = -1;

    private void Awake()
    {
        if (mover == null)
        {
            mover = GetComponent<Mover>();
        }

        if (controlador == null)
        {
            controlador = GetComponent<CharacterController>();
        }

        if (fuentePasos == null)
        {
            fuentePasos = GetComponent<AudioSource>();
        }

        fuentePasos.playOnAwake = false;
        fuentePasos.loop = false;

        // Pasos en primera persona: sonido 2D.
        fuentePasos.spatialBlend = 0f;
    }

    private void Start()
    {
        if (controlador != null)
        {
            estabaEnSuelo = controlador.isGrounded;
        }
    }

    private void Update()
    {
        if (mover == null ||
            controlador == null ||
            fuentePasos == null)
        {
            return;
        }

        if (Time.timeScale == 0f ||
            ControlPartida.PartidaTerminada ||
            UIInventarioLoot.InventarioAbierto ||
            SuitUpgradeUI.PanelAbierto)
        {
            estabaEnSuelo = controlador.isGrounded;
            return;
        }

        bool estaEnSuelo = controlador.isGrounded;

        // Sonido cuando el jugador intenta saltar.
        if (estabaEnSuelo &&
            Input.GetButtonDown("Jump"))
        {
            ReproducirClip(
                sonidoSalto,
                volumenSalto,
                false
            );
        }

        // Sonido al volver a tocar el suelo.
        if (!estabaEnSuelo && estaEnSuelo)
        {
            ReproducirClip(
                sonidoAterrizaje,
                volumenAterrizaje,
                false
            );
        }

        ProcesarPasos(estaEnSuelo);

        estabaEnSuelo = estaEnSuelo;
    }

    private void ProcesarPasos(bool estaEnSuelo)
    {
        if (!estaEnSuelo)
        {
            return;
        }

        if (!mover.EstaMoviendose)
        {
            return;
        }

        Vector3 velocidadPlana = new Vector3(
            controlador.velocity.x,
            0f,
            controlador.velocity.z
        );

        if (velocidadPlana.magnitude < 0.2f)
        {
            return;
        }

        if (Time.time < siguientePaso)
        {
            return;
        }

        float intervalo = intervaloCaminando;

        if (mover.EstaCorriendo)
        {
            intervalo = intervaloCorriendo;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            intervalo = intervaloAgachado;
        }

        siguientePaso = Time.time + intervalo;

        ReproducirPasoAleatorio();
    }

    private void ReproducirPasoAleatorio()
    {
        if (sonidosPasos == null ||
            sonidosPasos.Length == 0)
        {
            return;
        }

        int indice = Random.Range(0, sonidosPasos.Length);

        // Evita repetir inmediatamente el mismo clip.
        if (sonidosPasos.Length > 1 &&
            indice == ultimoIndice)
        {
            indice = (indice + 1) % sonidosPasos.Length;
        }

        ultimoIndice = indice;

        AudioClip clip = sonidosPasos[indice];

        ReproducirClip(
            clip,
            volumenPasos,
            true
        );
    }

    private void ReproducirClip(
        AudioClip clip,
        float volumen,
        bool variarPitch)
    {
        if (clip == null)
        {
            return;
        }

        fuentePasos.pitch = variarPitch
            ? Random.Range(pitchMinimo, pitchMaximo)
            : 1f;

        fuentePasos.PlayOneShot(clip, volumen);
    }
}
