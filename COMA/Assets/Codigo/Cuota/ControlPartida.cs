using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlPartida : MonoBehaviour
{
    public static bool PartidaTerminada
    {
        get;
        private set;
    }

    [Header("Referencias")]
    public ProgresoJugador progreso;

    [Header("Victoria final")]
    public GameObject panelVictoria;
    public TMP_Text textoVictoria;

    [Header("Derrota")]
    public GameObject panelDerrota;
    public TMP_Text textoDerrota;

    [Header("Interfaces que deben cerrarse")]
    public UIInventarioLoot uiInventario;
    public SuitUpgradeUI uiTraje;

    private void Start()
    {
        // Reiniciamos estado global.
        PartidaTerminada = false;
        Time.timeScale = 1f;

        // Ocultamos pantallas finales.
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(false);
        }
    }

    private void CerrarPanelesAbiertos()
    {
        // Cerramos inventario.
        if (uiInventario != null)
        {
            uiInventario.CerrarInventarioForzado();
        }

        // Cerramos mejoras del traje.
        if (uiTraje != null)
        {
            uiTraje.CerrarPanelForzado();
        }
    }

    public void TerminarPartidaPorVictoria()
    {
        // Evitamos terminar dos veces.
        if (PartidaTerminada)
        {
            return;
        }

        // Por seguridad comprobamos que exista progreso
        // y que realmente se haya cumplido la cuota.
        if (progreso != null && !progreso.CuotaCumplida)
        {
            Debug.LogWarning(
                "Se intentó finalizar sin cumplir la cuota."
            );

            return;
        }

        PartidaTerminada = true;

        CerrarPanelesAbiertos();

        // Mostramos victoria.
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        if (textoVictoria != null)
        {
            textoVictoria.text =
                "¡FELICIDADES!\n\n" +
                "ACABASTE EL JUEGO\n\n" +
                "Lograste cumplir la cuota y escapar " +
                "de las instalaciones.";
        }

        // Liberamos el cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Congelamos la partida.
        Time.timeScale = 0f;
    }

    public void TerminarPartidaPorMuerte()
    {
        if (PartidaTerminada)
        {
            return;
        }

        PartidaTerminada = true;

        CerrarPanelesAbiertos();

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }

        if (textoDerrota != null)
        {
            textoDerrota.text =
                "HAS MUERTO\n\n" +
                "No lograste escapar de las instalaciones.";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        PartidaTerminada = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void IrAlMenuInicio()
    {
        Time.timeScale = 1f;
        PartidaTerminada = false;

        SceneManager.LoadScene("MenuInicio");
    }
}