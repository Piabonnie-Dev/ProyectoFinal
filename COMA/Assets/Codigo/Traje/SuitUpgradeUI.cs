
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuitUpgradeUI : MonoBehaviour
{
    public static bool PanelAbierto { get; private set; }

    [Header("Referencias")]
    public PlayerSuitStats traje;
    public ProgresoJugador progreso;
    public GameObject panelMejoras;

    [Header("Textos")]
    public TMP_Text textoVelocidad;
    public TMP_Text textoVida;
    public TMP_Text textoCapacidad;
    public TMP_Text textoDineroDisponible;

    [Header("Botones")]
    public Button botonVelocidad;
    public Button botonVida;
    public Button botonCapacidad;

    [Header("Entrada")]
    public KeyCode teclaPanel = KeyCode.U;

    [Header("Costos base")]
    public int costoBaseVelocidad = 100;
    public int costoBaseVida = 80;
    public int costoBaseCapacidad = 90;

    [Header("Incremento por nivel")]
    public int incrementoVelocidad = 50;
    public int incrementoVida = 40;
    public int incrementoCapacidad = 45;

    [Header("Ajuste visual de los textos")]

    [Tooltip("Agrega una línea vacía entre el título y los detalles.")]
    public bool separarTituloDelDetalle = true;

    [Tooltip("Espacio adicional entre las líneas de TextMeshPro.")]
    public float espacioEntreLineas = 8f;

    [Tooltip("Evita que el texto se parta automáticamente en varias líneas.")]
    public bool evitarSaltoAutomatico = true;

    [Tooltip("Evita que Auto Size cambie el tamaño del texto durante el juego.")]
    public bool desactivarAutoSize = true;

    [Tooltip("Garantiza un tamaño mínimo para los rectángulos de texto.")]
    public bool forzarTamanoMinimo = true;

    [Min(1f)]
    public float anchoMinimoTexto = 500f;

    [Min(1f)]
    public float altoMinimoTexto = 170f;


    // Se ejecuta al comenzar la escena.
    void Start()
    {
        PanelAbierto = false;

        // El panel comienza cerrado.
        if (panelMejoras != null)
        {
            panelMejoras.SetActive(false);
        }

        // Busca automáticamente las referencias si no fueron asignadas.
        if (traje == null)
        {
            traje = FindObjectOfType<PlayerSuitStats>();
        }

        if (progreso == null)
        {
            progreso = FindObjectOfType<ProgresoJugador>();
        }

        // Actualiza la interfaz cuando cambia el dinero.
        if (progreso != null)
        {
            progreso.AlCambiarDinero += ActualizarUI;
        }

        // Configura los textos para evitar que se encimen.
        ConfigurarTextos();

        ActualizarUI();
    }


    // Se ejecuta cuando este objeto se destruye.
    void OnDestroy()
    {
        if (progreso != null)
        {
            progreso.AlCambiarDinero -= ActualizarUI;
        }
    }


    // Revisa constantemente si se presiona la tecla U.
    void Update()
    {
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        if (Input.GetKeyDown(teclaPanel))
        {
            AlternarPanel();
        }
    }


    // Abre o cierra el panel de mejoras.
    public void AlternarPanel()
    {
        PanelAbierto = !PanelAbierto;

        if (panelMejoras != null)
        {
            panelMejoras.SetActive(PanelAbierto);
        }

        if (PanelAbierto)
        {
            // Libera el cursor para poder presionar botones.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Vuelve a bloquear el cursor al cerrar.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ActualizarUI();
    }


    // Calcula el precio de la siguiente mejora de velocidad.
    int ObtenerCostoVelocidad()
    {
        if (traje == null)
        {
            return 999999;
        }

        return costoBaseVelocidad +
               (traje.nivelVelocidad * incrementoVelocidad);
    }


    // Calcula el precio de la siguiente mejora de vida.
    int ObtenerCostoVida()
    {
        if (traje == null)
        {
            return 999999;
        }

        return costoBaseVida +
               (traje.nivelVida * incrementoVida);
    }


    // Calcula el precio de la siguiente mejora de capacidad.
    int ObtenerCostoCapacidad()
    {
        if (traje == null)
        {
            return 999999;
        }

        return costoBaseCapacidad +
               (traje.nivelCapacidad * incrementoCapacidad);
    }


    // Método conectado al botón de velocidad.
    public void MejorarVelocidad()
    {
        if (traje == null || progreso == null)
        {
            return;
        }

        if (!traje.PuedeMejorarVelocidad())
        {
            return;
        }

        int costo = ObtenerCostoVelocidad();

        if (!progreso.GastarDinero(costo))
        {
            return;
        }

        traje.MejorarVelocidad();

        ActualizarUI();
    }


    // Método conectado al botón de vida.
    public void MejorarVida()
    {
        if (traje == null || progreso == null)
        {
            return;
        }

        if (!traje.PuedeMejorarVida())
        {
            return;
        }

        int costo = ObtenerCostoVida();

        if (!progreso.GastarDinero(costo))
        {
            return;
        }

        traje.MejorarVida();

        ActualizarUI();
    }


    // Método conectado al botón de capacidad.
    public void MejorarCapacidad()
    {
        if (traje == null || progreso == null)
        {
            return;
        }

        if (!traje.PuedeMejorarCapacidad())
        {
            return;
        }

        int costo = ObtenerCostoCapacidad();

        if (!progreso.GastarDinero(costo))
        {
            return;
        }

        traje.MejorarCapacidad();

        ActualizarUI();
    }


    // Configura todos los textos de mejoras.
    void ConfigurarTextos()
    {
        PrepararTexto(textoVelocidad);
        PrepararTexto(textoVida);
        PrepararTexto(textoCapacidad);
    }


    // Evita que TextMeshPro comprima, corte o parta el texto.
    void PrepararTexto(TMP_Text texto)
    {
        if (texto == null)
        {
            return;
        }

        /*
         * Este código NO cambia:
         *
         * - La posición.
         * - Los anchors.
         * - La escala.
         * - La rotación.
         *
         * Únicamente prepara el texto y garantiza
         * que tenga suficiente espacio.
         */

        if (desactivarAutoSize)
        {
            texto.enableAutoSizing = false;
        }

        // Si evitarSaltoAutomatico está activado,
        // el texto no se dividirá solo.
        texto.enableWordWrapping = !evitarSaltoAutomatico;

        // Permite que el texto use el espacio disponible.
        texto.overflowMode = TextOverflowModes.Overflow;

        // Alineación superior izquierda.
        texto.alignment = TextAlignmentOptions.TopLeft;

        // Separación entre las líneas.
        texto.lineSpacing = espacioEntreLineas;

        if (!forzarTamanoMinimo)
        {
            return;
        }

        RectTransform rectTexto = texto.rectTransform;

        float anchoActual = rectTexto.rect.width;
        float altoActual = rectTexto.rect.height;

        // Solo aumenta el ancho si actualmente es demasiado pequeño.
        if (anchoActual < anchoMinimoTexto)
        {
            rectTexto.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                anchoMinimoTexto
            );
        }

        // Solo aumenta la altura si actualmente es demasiado pequeña.
        if (altoActual < altoMinimoTexto)
        {
            rectTexto.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                altoMinimoTexto
            );
        }
    }


    // Construye el texto de cada mejora.
    string CrearBloque(
        string titulo,
        string actual,
        string costo = null
    )
    {
        string separacion;

        if (separarTituloDelDetalle)
        {
            // Dos saltos de línea dejan una línea vacía.
            separacion = "\n\n";
        }
        else
        {
            separacion = "\n";
        }

        string resultado = titulo + separacion + actual;

        if (!string.IsNullOrEmpty(costo))
        {
            resultado += "\n" + costo;
        }

        return resultado;
    }


    // Actualiza todos los textos y botones del panel.
    void ActualizarUI()
    {
        if (traje == null)
        {
            return;
        }

        int costoVelocidad = ObtenerCostoVelocidad();
        int costoVida = ObtenerCostoVida();
        int costoCapacidad = ObtenerCostoCapacidad();


        // TEXTO DE VELOCIDAD
        if (textoVelocidad != null)
        {
            if (traje.PuedeMejorarVelocidad())
            {
                textoVelocidad.text = CrearBloque(
                    "Velocidad: Nivel " + traje.nivelVelocidad,

                    "Actual: x" +
                    traje.ObtenerMultiplicadorVelocidad().ToString("0.00"),

                    "Costo: $" + costoVelocidad
                );
            }
            else
            {
                textoVelocidad.text = CrearBloque(
                    "Velocidad: MAX",

                    "Actual: x" +
                    traje.ObtenerMultiplicadorVelocidad().ToString("0.00")
                );
            }
        }


        // TEXTO DE VIDA
        if (textoVida != null)
        {
            string vidaActual = "0";

            if (traje.playerHealth != null)
            {
                vidaActual =
                    traje.playerHealth.vidaMaxima.ToString("0");
            }

            if (traje.PuedeMejorarVida())
            {
                textoVida.text = CrearBloque(
                    "Vida: Nivel " + traje.nivelVida,

                    "Actual: " +
                    vidaActual +
                    " HP",

                    "Costo: $" + costoVida
                );
            }
            else
            {
                textoVida.text = CrearBloque(
                    "Vida: MAX",

                    "Actual: " +
                    vidaActual +
                    " HP"
                );
            }
        }


        // TEXTO DE CAPACIDAD
        if (textoCapacidad != null)
        {
            string pesoMax = "0";
            string slotsMax = "0";

            if (traje.inventarioLoot != null)
            {
                pesoMax =
                    traje.inventarioLoot.pesoMaximo.ToString("0.0");

                slotsMax =
                    traje.inventarioLoot
                    .capacidadMaximaObjetos
                    .ToString();
            }

            if (traje.PuedeMejorarCapacidad())
            {
                textoCapacidad.text = CrearBloque(
                    "Capacidad: Nivel " +
                    traje.nivelCapacidad,

                    "Actual: " +
                    pesoMax +
                    " kg / " +
                    slotsMax +
                    " slots",

                    "Costo: $" +
                    costoCapacidad
                );
            }
            else
            {
                textoCapacidad.text = CrearBloque(
                    "Capacidad: MAX",

                    "Actual: " +
                    pesoMax +
                    " kg / " +
                    slotsMax +
                    " slots"
                );
            }
        }


        // TEXTO DEL DINERO
        if (textoDineroDisponible != null &&
            progreso != null)
        {
            textoDineroDisponible.text =
                "Dinero: $" +
                progreso.dineroTotal;
        }

        ActualizarBotones();
    }


    // Habilita o deshabilita los botones según el dinero.
    void ActualizarBotones()
    {
        if (traje == null || progreso == null)
        {
            return;
        }

        if (botonVelocidad != null)
        {
            botonVelocidad.interactable =
                traje.PuedeMejorarVelocidad() &&
                progreso.dineroTotal >= ObtenerCostoVelocidad();
        }

        if (botonVida != null)
        {
            botonVida.interactable =
                traje.PuedeMejorarVida() &&
                progreso.dineroTotal >= ObtenerCostoVida();
        }

        if (botonCapacidad != null)
        {
            botonCapacidad.interactable =
                traje.PuedeMejorarCapacidad() &&
                progreso.dineroTotal >= ObtenerCostoCapacidad();
        }
    }


    // Cierra el panel desde otro script.
    public void CerrarPanelForzado()
    {
        PanelAbierto = false;

        if (panelMejoras != null)
        {
            panelMejoras.SetActive(false);
        }

        // Evita que el cursor permanezca libre
        // después de cerrar el panel.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

