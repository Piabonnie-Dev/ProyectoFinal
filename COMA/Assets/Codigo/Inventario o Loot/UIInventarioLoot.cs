using TMPro;
using UnityEngine;

public class UIInventarioLoot : MonoBehaviour
{
    public static bool InventarioAbierto
    {
        get;
        private set;
    }

    [Header("Inventario")]
    public InventarioLoot inventario;
    public GameObject panelInventario;

    [Header("Lista visual")]
    public Transform contenedorItems;
    public GameObject prefabItemInventario;

    [Header("Textos")]
    public TMP_Text textoResumen;
    public TMP_Text textoEstado;

    [Header("Control")]
    public KeyCode teclaInventario = KeyCode.Tab;

    [Header("Paneles incompatibles")]
    public GameObject panelMejorasDelTraje;
    public GameObject panelMonitorCamaras;

    [Header("Mensajes que se ocultan")]
    public GameObject textoPromptExtraccion;
    public GameObject textoResultadoExtraccion;
    public GameObject textoRejaFinal;

    [Header("Pausa")]
    public GameObject panelMenuPausa;
    public GameObject panelOpcionesPausa;

    [Header("HUD que se oculta al abrir inventario")]
    public GameObject[] hudOcultarAlAbrir;

    private void Start()
    {
        // El inventario inicia cerrado.
        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }

        InventarioAbierto = false;

        // Actualizamos automáticamente al recoger o extraer loot.
        if (inventario != null)
        {
            inventario.AlCambiarInventario += ActualizarUI;
        }

        ActualizarUI();
    }

    private void Update()
    {
        // No permitimos abrir inventario al terminar la partida.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // Si se abre la pausa, cerramos el inventario.
        if (InventarioAbierto && PausaAbierta())
        {
            CerrarInventarioForzado();
            return;
        }

        if (Input.GetKeyDown(teclaInventario))
        {
            if (InventarioAbierto)
            {
                CerrarInventario();
            }
            else
            {
                // La pausa siempre tiene prioridad.
                if (PausaAbierta())
                {
                    return;
                }

                AbrirInventario();
            }
        }
    }

    private void OnDestroy()
    {
        // Evitamos dejar una suscripción activa al cambiar de escena.
        if (inventario != null)
        {
            inventario.AlCambiarInventario -= ActualizarUI;
        }
    }

    private bool PausaAbierta()
    {
        bool menuPausaActivo =
            panelMenuPausa != null &&
            panelMenuPausa.activeInHierarchy;

        bool opcionesPausaActivas =
            panelOpcionesPausa != null &&
            panelOpcionesPausa.activeInHierarchy;

        return menuPausaActivo ||
               opcionesPausaActivas ||
               Time.timeScale == 0f;
    }

    public void AbrirInventario()
    {
        if (panelInventario == null)
        {
            return;
        }

        // Cerramos las demás ventanas grandes.
        CerrarInterfacesIncompatibles();

        // Ocultamos el HUD normal para que no se encime.
        CambiarVisibilidadHUD(false);

        panelInventario.SetActive(true);
        InventarioAbierto = true;

        // Liberamos el mouse.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ActualizarUI();
    }

    public void CerrarInventario()
    {
        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }

        InventarioAbierto = false;

        // Recuperamos el HUD.
        CambiarVisibilidadHUD(true);

        // Solo bloqueamos el cursor si no está la pausa.
        if (!PausaAbierta() &&
            !ControlPartida.PartidaTerminada)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void CerrarInventarioForzado()
    {
        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }

        InventarioAbierto = false;

        // El HUD puede quedar activo detrás de la pausa,
        // porque la pausa tiene mayor Sorting Order.
        CambiarVisibilidadHUD(true);
    }

    private void CambiarVisibilidadHUD(bool visible)
    {
        if (hudOcultarAlAbrir == null)
        {
            return;
        }

        foreach (GameObject elemento in hudOcultarAlAbrir)
        {
            if (elemento != null)
            {
                elemento.SetActive(visible);
            }
        }
    }

    private void CerrarInterfacesIncompatibles()
    {
        if (panelMejorasDelTraje != null)
        {
            panelMejorasDelTraje.SetActive(false);
        }

        if (panelMonitorCamaras != null)
        {
            panelMonitorCamaras.SetActive(false);
        }

        if (textoPromptExtraccion != null)
        {
            textoPromptExtraccion.SetActive(false);
        }

        if (textoResultadoExtraccion != null)
        {
            textoResultadoExtraccion.SetActive(false);
        }

        if (textoRejaFinal != null)
        {
            textoRejaFinal.SetActive(false);
        }
    }

    public void ActualizarUI()
    {
        if (inventario == null ||
            contenedorItems == null ||
            prefabItemInventario == null)
        {
            return;
        }

        // Borramos las tarjetas anteriores.
        for (int i = contenedorItems.childCount - 1; i >= 0; i--)
        {
            Destroy(
                contenedorItems.GetChild(i).gameObject
            );
        }

        // Mostramos estado vacío o creamos las tarjetas.
        if (inventario.EstaVacio)
        {
            if (textoEstado != null)
            {
                textoEstado.gameObject.SetActive(true);
                textoEstado.text = "Inventario vacío.";
            }
        }
        else
        {
            if (textoEstado != null)
            {
                textoEstado.gameObject.SetActive(false);
            }

            foreach (LootGuardado loot in inventario.ObtenerLoot())
            {
                GameObject nuevaTarjeta = Instantiate(
                    prefabItemInventario,
                    contenedorItems
                );

                ItemInventarioUI itemUI =
                    nuevaTarjeta.GetComponent<ItemInventarioUI>();

                if (itemUI != null)
                {
                    itemUI.Configurar(loot);
                }
            }
        }

        // Actualizamos el resumen general.
        if (textoResumen != null)
        {
            textoResumen.text =
                "Objetos: " +
                inventario.CantidadObjetos +
                "/" +
                inventario.capacidadMaximaObjetos +
                "\nPeso: " +
                inventario.pesoActual.ToString("0.0") +
                "/" +
                inventario.pesoMaximo.ToString("0.0") +
                " kg" +
                "\nValor total: $" +
                inventario.valorTotal;
        }
    }
}