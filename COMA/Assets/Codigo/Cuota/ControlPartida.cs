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

    void TerminarPartidaPorCuota()
    {
        if (PartidaTerminada)
            return;

        PartidaTerminada = true;

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

        Debug.Log("Se ejecutó TerminarPartidaPorMuerte()");

        PartidaTerminada = true;

        if (panelDerrota != null)
            panelDerrota.SetActive(true);
        else
            Debug.LogWarning("panelDerrota no está asignado.");

        if (textoDerrota != null)
            textoDerrota.text = "HAS MUERTO\nPartida terminada";
        else
            Debug.LogWarning("textoDerrota no está asignado.");

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