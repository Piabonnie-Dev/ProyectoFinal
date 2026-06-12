using TMPro;
using UnityEngine;

public class ZonaExtraccion : MonoBehaviour
{
    [Header("Referencias")]
    public InventarioLoot inventario;
    public ProgresoJugador progreso;

    [Header("Interfaz")]
    public TMP_Text textoPrompt;
    public TMP_Text textoResultado;

    [Header("Entrada")]
    public KeyCode teclaExtraer = KeyCode.X;

    [Header("Mensajes")]
    public float tiempoMensajeResultado = 3f;

    private bool jugadorDentro = false;

    private void Start()
    {
        // Ocultamos mensajes al iniciar.
        if (textoPrompt != null)
        {
            textoPrompt.gameObject.SetActive(false);
        }

        if (textoResultado != null)
        {
            textoResultado.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // No permitimos extraer cuando la partida terminó.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // Solo escuchamos la tecla estando dentro.
        if (!jugadorDentro)
        {
            return;
        }

        if (Input.GetKeyDown(teclaExtraer))
        {
            IntentarExtraer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorDentro = true;

        ActualizarPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorDentro = false;

        if (textoPrompt != null)
        {
            textoPrompt.gameObject.SetActive(false);
        }
    }

    private void ActualizarPrompt()
    {
        if (textoPrompt == null)
        {
            return;
        }

        textoPrompt.gameObject.SetActive(true);

        if (inventario == null || inventario.EstaVacio)
        {
            textoPrompt.text =
                "No tienes loot para extraer";
        }
        else
        {
            textoPrompt.text =
                "Presiona X para EXTRAER\n" +
                inventario.CantidadObjetos +
                " objetos | $" +
                inventario.valorTotal;
        }
    }

    private void IntentarExtraer()
    {
        // Comprobamos referencias.
        if (inventario == null || progreso == null)
        {
            Debug.LogWarning(
                "Falta asignar InventarioLoot o ProgresoJugador " +
                "en ZonaExtraccion."
            );

            return;
        }

        // No permitimos extraer inventario vacío.
        if (inventario.EstaVacio ||
            inventario.valorTotal <= 0)
        {
            MostrarResultado(
                "No tienes nada que extraer."
            );

            ActualizarPrompt();

            return;
        }

        // Guardamos los datos antes de vaciarlo.
        int valorExtraido = inventario.valorTotal;
        int cantidadObjetos = inventario.CantidadObjetos;

        // Sumamos dinero y progreso de cuota.
        progreso.AgregarDinero(valorExtraido);

        // Eliminamos el contenido entregado.
        inventario.VaciarInventario();

        // Mostramos un resultado dependiendo de la cuota.
        if (progreso.CuotaCumplida)
        {
            MostrarResultado(
                "EXTRACCIÓN EXITOSA\n" +
                "+" + valorExtraido + "$\n" +
                cantidadObjetos + " objetos entregados\n\n" +
                "CUOTA CUMPLIDA\n" +
                "REGRESA A LA REJA"
            );
        }
        else
        {
            MostrarResultado(
                "EXTRACCIÓN EXITOSA\n" +
                "+" + valorExtraido + "$\n" +
                cantidadObjetos + " objetos entregados\n\n" +
                "Faltan $" +
                progreso.ObtenerCantidadFaltante() +
                " para la cuota"
            );
        }

        ActualizarPrompt();

        Debug.Log(
            "Extracción exitosa. Valor: $" + valorExtraido
        );
    }

    private void MostrarResultado(string mensaje)
    {
        if (textoResultado == null)
        {
            return;
        }

        textoResultado.text = mensaje;
        textoResultado.gameObject.SetActive(true);

        CancelInvoke(nameof(OcultarResultado));

        Invoke(
            nameof(OcultarResultado),
            tiempoMensajeResultado
        );
    }

    private void OcultarResultado()
    {
        if (textoResultado != null)
        {
            textoResultado.gameObject.SetActive(false);
        }
    }
}