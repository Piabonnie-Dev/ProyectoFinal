
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioArmaFPS : MonoBehaviour
{
    [Header("Referencias")]
    public WeaponFPS arma;
    public AudioSource fuenteArma;

    [Header("Disparos")]
    public AudioClip[] disparosFuertes;
    public AudioClip[] sonidosBala;

    [Header("Otros sonidos")]
    public AudioClip sonidoRecarga;
    public AudioClip sonidoCambioModo;
    public AudioClip sonidoSinMunicion;

    [Header("Volúmenes")]
    [Range(0f, 1f)]
    public float volumenDisparo = 0.9f;

    [Range(0f, 1f)]
    public float volumenBala = 0.55f;

    [Range(0f, 1f)]
    public float volumenRecarga = 0.8f;

    [Range(0f, 1f)]
    public float volumenCambioModo = 0.6f;

    private int municionAnterior;
    private WeaponFPS.TipoDisparo modoAnterior;
    private float siguienteSonidoRecarga;
    private float siguienteSonidoSinMunicion;

    private void Awake()
    {
        if (arma == null)
        {
            arma = GetComponent<WeaponFPS>();
        }

        if (fuenteArma == null)
        {
            fuenteArma = GetComponent<AudioSource>();
        }

        fuenteArma.playOnAwake = false;
        fuenteArma.loop = false;
        fuenteArma.spatialBlend = 0f;
    }

    private void Start()
    {
        if (arma == null)
        {
            return;
        }

        municionAnterior = arma.municionEnCargador;
        modoAnterior = arma.tipoDisparo;
    }

    private void Update()
    {
        if (arma == null || fuenteArma == null)
        {
            return;
        }

        bool interfazBloqueando =
            Time.timeScale == 0f ||
            ControlPartida.PartidaTerminada ||
            UIInventarioLoot.InventarioAbierto ||
            SuitUpgradeUI.PanelAbierto;

        // Detecta un disparo exitoso:
        // la munición realmente bajó.
        if (arma.municionEnCargador < municionAnterior)
        {
            ReproducirDisparo();
        }

        // Detecta cambio entre Normal, Stun y Explosiva.
        if (arma.tipoDisparo != modoAnterior)
        {
            if (sonidoCambioModo != null)
            {
                fuenteArma.PlayOneShot(
                    sonidoCambioModo,
                    volumenCambioModo
                );
            }

            modoAnterior = arma.tipoDisparo;
        }

        if (!interfazBloqueando)
        {
            RevisarRecarga();
            RevisarArmaVacia();
        }

        municionAnterior = arma.municionEnCargador;
    }

    private void RevisarRecarga()
    {
        bool puedeRecargar =
            arma.municionEnCargador < arma.balasPorCargador &&
            arma.municionReserva > 0;

        bool recargaManual =
            Input.GetKeyDown(KeyCode.R);

        bool recargaAutomatica =
            Input.GetMouseButtonDown(0) &&
            arma.municionEnCargador <= 0;

        if (!puedeRecargar)
        {
            return;
        }

        if (!recargaManual && !recargaAutomatica)
        {
            return;
        }

        if (Time.unscaledTime < siguienteSonidoRecarga)
        {
            return;
        }

        siguienteSonidoRecarga =
            Time.unscaledTime +
            Mathf.Max(0.2f, arma.tiempoRecarga);

        if (sonidoRecarga != null)
        {
            fuenteArma.PlayOneShot(
                sonidoRecarga,
                volumenRecarga
            );
        }
    }

    private void RevisarArmaVacia()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (arma.municionEnCargador > 0)
        {
            return;
        }

        // Si todavía hay reserva, se reproducirá la recarga,
        // no el sonido de arma completamente vacía.
        if (arma.municionReserva > 0)
        {
            return;
        }

        if (Time.unscaledTime < siguienteSonidoSinMunicion)
        {
            return;
        }

        siguienteSonidoSinMunicion =
            Time.unscaledTime + 0.35f;

        if (sonidoSinMunicion != null)
        {
            fuenteArma.PlayOneShot(
                sonidoSinMunicion,
                0.7f
            );
        }
    }

    private void ReproducirDisparo()
    {
        AudioClip disparo =
            ObtenerClipAleatorio(disparosFuertes);

        AudioClip bala =
            ObtenerClipAleatorio(sonidosBala);

        if (disparo != null)
        {
            fuenteArma.PlayOneShot(
                disparo,
                volumenDisparo
            );
        }

        if (bala != null)
        {
            fuenteArma.PlayOneShot(
                bala,
                volumenBala
            );
        }
    }

    private AudioClip ObtenerClipAleatorio(
        AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }
}


