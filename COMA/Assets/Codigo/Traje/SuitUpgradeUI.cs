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

    void Start()
    {
        PanelAbierto = false;

        if (panelMejoras != null)
            panelMejoras.SetActive(false);

        if (traje == null)
            traje = FindObjectOfType<PlayerSuitStats>();

        if (progreso == null)
            progreso = FindObjectOfType<ProgresoJugador>();

        if (progreso != null)
            progreso.AlCambiarDinero += ActualizarUI;

        ActualizarUI();
    }

    void OnDestroy()
    {
        if (progreso != null)
            progreso.AlCambiarDinero -= ActualizarUI;
    }

    void Update()
    {
        if (ControlPartida.PartidaTerminada)
            return;

        if (Input.GetKeyDown(teclaPanel))
            AlternarPanel();
    }

    public void AlternarPanel()
    {
        PanelAbierto = !PanelAbierto;

        if (panelMejoras != null)
            panelMejoras.SetActive(PanelAbierto);

        if (PanelAbierto)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ActualizarUI();
    }

    int ObtenerCostoVelocidad()
    {
        if (traje == null) return 999999;
        return costoBaseVelocidad + (traje.nivelVelocidad * incrementoVelocidad);
    }

    int ObtenerCostoVida()
    {
        if (traje == null) return 999999;
        return costoBaseVida + (traje.nivelVida * incrementoVida);
    }

    int ObtenerCostoCapacidad()
    {
        if (traje == null) return 999999;
        return costoBaseCapacidad + (traje.nivelCapacidad * incrementoCapacidad);
    }

    public void MejorarVelocidad()
    {
        if (traje == null || progreso == null)
            return;

        if (!traje.PuedeMejorarVelocidad())
            return;

        int costo = ObtenerCostoVelocidad();

        if (!progreso.GastarDinero(costo))
            return;

        traje.MejorarVelocidad();
        ActualizarUI();
    }

    public void MejorarVida()
    {
        if (traje == null || progreso == null)
            return;

        if (!traje.PuedeMejorarVida())
            return;

        int costo = ObtenerCostoVida();

        if (!progreso.GastarDinero(costo))
            return;

        traje.MejorarVida();
        ActualizarUI();
    }

    public void MejorarCapacidad()
    {
        if (traje == null || progreso == null)
            return;

        if (!traje.PuedeMejorarCapacidad())
            return;

        int costo = ObtenerCostoCapacidad();

        if (!progreso.GastarDinero(costo))
            return;

        traje.MejorarCapacidad();
        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (traje == null)
            return;

        int costoVelocidad = ObtenerCostoVelocidad();
        int costoVida = ObtenerCostoVida();
        int costoCapacidad = ObtenerCostoCapacidad();

        if (textoVelocidad != null)
        {
            if (traje.PuedeMejorarVelocidad())
            {
                textoVelocidad.text =
                    "Velocidad: Nivel " + traje.nivelVelocidad +
                    "\nActual: x" + traje.ObtenerMultiplicadorVelocidad().ToString("0.00") +
                    "\nCosto: $" + costoVelocidad;
            }
            else
            {
                textoVelocidad.text =
                    "Velocidad: MAX" +
                    "\nActual: x" + traje.ObtenerMultiplicadorVelocidad().ToString("0.00");
            }
        }

        if (textoVida != null)
        {
            string vidaActual = traje.playerHealth != null ? traje.playerHealth.vidaMaxima.ToString("0") : "0";

            if (traje.PuedeMejorarVida())
            {
                textoVida.text =
                    "Vida: Nivel " + traje.nivelVida +
                    "\nActual: " + vidaActual + " HP" +
                    "\nCosto: $" + costoVida;
            }
            else
            {
                textoVida.text =
                    "Vida: MAX" +
                    "\nActual: " + vidaActual + " HP";
            }
        }

        if (textoCapacidad != null)
        {
            string pesoMax = traje.inventarioLoot != null ? traje.inventarioLoot.pesoMaximo.ToString("0.0") : "0";
            string slotsMax = traje.inventarioLoot != null ? traje.inventarioLoot.capacidadMaximaObjetos.ToString() : "0";

            if (traje.PuedeMejorarCapacidad())
            {
                textoCapacidad.text =
                    "Capacidad: Nivel " + traje.nivelCapacidad +
                    "\nActual: " + pesoMax + " kg / " + slotsMax + " slots" +
                    "\nCosto: $" + costoCapacidad;
            }
            else
            {
                textoCapacidad.text =
                    "Capacidad: MAX" +
                    "\nActual: " + pesoMax + " kg / " + slotsMax + " slots";
            }
        }

        if (textoDineroDisponible != null && progreso != null)
        {
            textoDineroDisponible.text = "Dinero: $" + progreso.dineroTotal;
        }

        ActualizarBotones();
    }

    void ActualizarBotones()
    {
        if (traje == null || progreso == null)
            return;

        if (botonVelocidad != null)
            botonVelocidad.interactable = traje.PuedeMejorarVelocidad() && progreso.dineroTotal >= ObtenerCostoVelocidad();

        if (botonVida != null)
            botonVida.interactable = traje.PuedeMejorarVida() && progreso.dineroTotal >= ObtenerCostoVida();

        if (botonCapacidad != null)
            botonCapacidad.interactable = traje.PuedeMejorarCapacidad() && progreso.dineroTotal >= ObtenerCostoCapacidad();
    }

    public void CerrarPanelForzado()
    {
        PanelAbierto = false;
        if(panelMejoras != null)
        panelMejoras.SetActive(false);
    }
}
