using TMPro;
using UnityEngine;

public class RejaSalidaFinal : MonoBehaviour
{
    [Header("Referencias")]
    public ProgresoJugador progreso;

    [Header("Partes de la reja")]
    public Collider bloqueoReja;
    public Transform visualReja;

    [Header("Interfaz")]
    public TMP_Text textoPrompt;

    [Header("Entrada")]
    public KeyCode teclaAbrir = KeyCode.E;

    [Header("Forma de apertura")]
    public bool ocultarVisualAlAbrir = true;

    public Vector3 rotacionAbiertaRelativa =
        new Vector3(0f, 90f, 0f);

    [Header("Estado")]
    [SerializeField] private bool jugadorDentro = false;
    [SerializeField] private bool rejaAbierta = false;

    private Quaternion rotacionCerrada;

    // Permite que ZonaVictoriaFinal consulte el estado.
    public bool EstaAbierta
    {
        get
        {
            return rejaAbierta;
        }
    }

    private void Start()
    {
        // Guardamos la rotación original.
        if (visualReja != null)
        {
            rotacionCerrada = visualReja.localRotation;
        }

        // La barrera física inicia activa.
        if (bloqueoReja != null)
        {
            bloqueoReja.enabled = true;
        }

        // Ocultamos el texto inicialmente.
        if (textoPrompt != null)
        {
            textoPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // No procesamos interacción si la partida terminó.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // Solo escuchamos E cuando el jugador está dentro del trigger.
        if (!jugadorDentro || rejaAbierta)
        {
            return;
        }

        // Presionamos E para intentar abrir la salida.
        if (Input.GetKeyDown(teclaAbrir))
        {
            IntentarAbrirReja();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos el Player o cualquiera de sus hijos.
        if (!EsJugador(other))
        {
            return;
        }

        jugadorDentro = true;

        Debug.Log("Jugador entró en la zona de interacción de la reja.");

        ActualizarMensaje();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!EsJugador(other))
        {
            return;
        }

        jugadorDentro = false;

        Debug.Log("Jugador salió de la zona de interacción de la reja.");

        if (textoPrompt != null)
        {
            textoPrompt.gameObject.SetActive(false);
        }
    }

    private bool EsJugador(Collider other)
    {
        // Detectamos si el propio collider tiene tag Player.
        if (other.CompareTag("Player"))
        {
            return true;
        }

        // Detectamos si el collider es hijo del objeto Player.
        if (other.transform.root.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    private void IntentarAbrirReja()
    {
        // Comprobamos que ProgresoJugador esté asignado.
        if (progreso == null)
        {
            Debug.LogWarning(
                "RejaSalidaFinal no tiene ProgresoJugador asignado."
            );

            return;
        }

        // Si no ha cumplido la cuota, la salida sigue cerrada.
        if (!progreso.CuotaCumplida)
        {
            Debug.Log(
                "No puedes abrir la salida. Faltan $" +
                progreso.ObtenerCantidadFaltante()
            );

            ActualizarMensaje();

            return;
        }

        // Cumplió la cuota: abrimos la salida.
        AbrirReja();
    }

    private void AbrirReja()
    {
        if (rejaAbierta)
        {
            return;
        }

        rejaAbierta = true;

        // Quitamos el collider que bloquea físicamente la salida.
        if (bloqueoReja != null)
        {
            bloqueoReja.enabled = false;
        }

        if (visualReja != null)
        {
            if (ocultarVisualAlAbrir)
            {
                // Solución provisional más segura:
                // hacemos desaparecer la reja.
                visualReja.gameObject.SetActive(false);
            }
            else
            {
                // Alternativa: rotar la reja.
                visualReja.localRotation =
                    rotacionCerrada *
                    Quaternion.Euler(rotacionAbiertaRelativa);
            }
        }

        ActualizarMensaje();

        Debug.Log(
            "REJA ABIERTA: el jugador ya puede cruzar la salida."
        );
    }

    private void ActualizarMensaje()
    {
        if (textoPrompt == null)
        {
            return;
        }

        textoPrompt.gameObject.SetActive(true);

        if (rejaAbierta)
        {
            textoPrompt.text =
                "SALIDA ABIERTA\nCruza la reja";

            return;
        }

        if (progreso == null)
        {
            textoPrompt.text =
                "Error: falta asignar ProgresoJugador";

            return;
        }

        if (progreso.CuotaCumplida)
        {
            textoPrompt.text =
                "CUOTA CUMPLIDA\n" +
                "Presiona E para abrir la salida";
        }
        else
        {
            textoPrompt.text =
                "SALIDA BLOQUEADA\n" +
                "Faltan $" +
                progreso.ObtenerCantidadFaltante();
        }
    }
}