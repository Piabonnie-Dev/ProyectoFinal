using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlPartida : MonoBehaviour
{
    public static bool PartidaTerminada { get; private set; }

    [Header("Referencias")]
    public ProgresoJugador progreso;

    [Header("Victoria")]
    public GameObject panelVictoria;
    public TMP_Text textoVictoria;

    [Header("Derrota")]
    public GameObject panelDerrota;
    public TMP_Text textoDerrota;

    [Header("UI a cerrar")]
    public UIInventarioLoot uiInventario;
    public SuitUpgradeUI uiTraje;

    void Start()
    {
        PartidaTerminada = false;
        Time.timeScale = 1f;

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (progreso != null)
            progreso.AlCumplirCuota += TerminarPartidaPorCuota;
    }

    void OnDestroy()
    {
        if (progreso != null)
            progreso.AlCumplirCuota -= TerminarPartidaPorCuota;
    }

    void CerrarPanelesAbiertos()
    {
        if (uiInventario != null)
            uiInventario.CerrarInventarioForzado();

        if (uiTraje != null)
            uiTraje.CerrarPanelForzado();
    }

    void TerminarPartidaPorCuota()
    {
        if (PartidaTerminada)
            return;

        PartidaTerminada = true;

        CerrarPanelesAbiertos();

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        if (textoVictoria != null)
            textoVictoria.text = "CUOTA CUMPLIDA\nPartida terminada";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void TerminarPartidaPorMuerte()
    {
        if (PartidaTerminada)
            return;

        PartidaTerminada = true;

        CerrarPanelesAbiertos();

        if (panelDerrota != null)
            panelDerrota.SetActive(true);

        if (textoDerrota != null)
            textoDerrota.text = "HAS MUERTO\nPartida terminada";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        PartidaTerminada = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}