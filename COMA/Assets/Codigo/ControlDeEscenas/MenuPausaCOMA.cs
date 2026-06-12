using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPausaCOMA : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuPausa;
    public GameObject panelOpcionesPausa;

    [Header("Escenas")]
    public string nombreEscenaMenuInicio = "MenuInicio";

    [Header("Opciones")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    [Header("Textos")]
    public TMP_Text textoVolumen;
    public TMP_Text textoSensibilidad;

    [Header("Camara FPS")]
    public CamaraPrimeraPersona camaraPrimeraPersona;

    private bool juegoPausado = false;

    private void Start()
    {
        // Cerramos el panel de pausa al iniciar.
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }

        // Cerramos el panel de opciones al iniciar.
        if (panelOpcionesPausa != null)
        {
            panelOpcionesPausa.SetActive(false);
        }

        // El juego inicia sin pausa.
        juegoPausado = false;
        Time.timeScale = 1f;

        // Bloqueamos el cursor para jugar.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Cargamos las opciones guardadas.
        CargarOpcionesGuardadas();
    }

    private void Update()
    {
        // Si la partida terminó, no abrimos pausa.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // ESC abre o cierra pausa.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }

        // Esto hace que las opciones funcionen aunque el OnValueChanged del slider esté mal conectado.
        if (juegoPausado && panelOpcionesPausa != null && panelOpcionesPausa.activeSelf)
        {
            AplicarOpcionesDesdeSliders();
        }
    }

    public void Pausar()
    {
        // Marcamos que el juego está pausado.
        juegoPausado = true;

        // Congelamos el juego.
        Time.timeScale = 0f;

        // Liberamos el mouse para usar botones.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Mostramos el panel de pausa.
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(true);
        }

        // Ocultamos opciones.
        if (panelOpcionesPausa != null)
        {
            panelOpcionesPausa.SetActive(false);
        }
    }

    public void Reanudar()
    {
        // Quitamos pausa.
        juegoPausado = false;

        // Restauramos el tiempo.
        Time.timeScale = 1f;

        // Bloqueamos cursor para seguir jugando.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Ocultamos pausa.
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }

        // Ocultamos opciones.
        if (panelOpcionesPausa != null)
        {
            panelOpcionesPausa.SetActive(false);
        }

        // Guardamos las opciones actuales.
        GuardarOpcionesActuales();
    }

    public void AbrirOpciones()
    {
        // Ocultamos menú de pausa.
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }

        // Mostramos opciones.
        if (panelOpcionesPausa != null)
        {
            panelOpcionesPausa.SetActive(true);
        }

        // Refrescamos valores al abrir.
        CargarOpcionesGuardadas();
    }

    public void VolverDesdeOpciones()
    {
        // Guardamos cambios antes de volver.
        GuardarOpcionesActuales();

        // Ocultamos opciones.
        if (panelOpcionesPausa != null)
        {
            panelOpcionesPausa.SetActive(false);
        }

        // Mostramos pausa.
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(true);
        }
    }

    public void Reiniciar()
    {
        // Restauramos tiempo antes de recargar.
        Time.timeScale = 1f;

        // Recargamos la escena actual.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenuInicio()
    {
        // Restauramos tiempo antes de cambiar escena.
        Time.timeScale = 1f;

        // Cargamos menú de inicio.
        SceneManager.LoadScene(nombreEscenaMenuInicio);
    }

    public void Salir()
    {
        // Mensaje para probar dentro del editor.
        Debug.Log("Saliendo de COMA...");

        // Cierra el juego compilado.
        Application.Quit();
    }

    private void AplicarOpcionesDesdeSliders()
    {
        // Aplicamos volumen en tiempo real.
        if (sliderVolumen != null)
        {
            float volumen = sliderVolumen.value;
            AudioListener.volume = volumen;

            if (textoVolumen != null)
            {
                int porcentaje = Mathf.RoundToInt(volumen * 100f);
                textoVolumen.text = "Volumen: " + porcentaje + "%";
            }
        }

        // Aplicamos sensibilidad en tiempo real.
        if (sliderSensibilidad != null)
        {
            float sensibilidad = sliderSensibilidad.value;

            // Protección: si por error el slider está entre 0 y 1, forzamos un valor jugable.
            if (sensibilidad <= 1f)
            {
                sensibilidad = 180f;
                sliderSensibilidad.value = sensibilidad;
            }

            if (camaraPrimeraPersona != null)
            {
                camaraPrimeraPersona.sensibilidadX = sensibilidad;
                camaraPrimeraPersona.sensibilidadY = sensibilidad;
            }

            if (textoSensibilidad != null)
            {
                textoSensibilidad.text = "Sensibilidad: " + Mathf.RoundToInt(sensibilidad);
            }
        }
    }

    private void GuardarOpcionesActuales()
    {
        // Guardamos volumen.
        if (sliderVolumen != null)
        {
            PlayerPrefs.SetFloat("VolumenGeneral", sliderVolumen.value);
        }

        // Guardamos sensibilidad.
        if (sliderSensibilidad != null)
        {
            float sensibilidad = sliderSensibilidad.value;

            if (sensibilidad <= 1f)
            {
                sensibilidad = 180f;
            }

            PlayerPrefs.SetFloat("SensibilidadMouse", sensibilidad);
        }

        // Confirmamos guardado.
        PlayerPrefs.Save();
    }

    private void CargarOpcionesGuardadas()
    {
        // Cargamos volumen.
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenGeneral", 1f);

        // Cargamos sensibilidad.
        float sensibilidadGuardada = PlayerPrefs.GetFloat("SensibilidadMouse", 180f);

        // Protección por si antes se guardó sensibilidad 0.
        if (sensibilidadGuardada <= 1f)
        {
            sensibilidadGuardada = 180f;
            PlayerPrefs.SetFloat("SensibilidadMouse", sensibilidadGuardada);
            PlayerPrefs.Save();
        }

        // Aplicamos volumen.
        AudioListener.volume = volumenGuardado;

        // Actualizamos slider de volumen.
        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumenGuardado;
        }

        // Actualizamos slider de sensibilidad.
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.minValue = 80f;
            sliderSensibilidad.maxValue = 300f;
            sliderSensibilidad.value = sensibilidadGuardada;
        }

        // Aplicamos sensibilidad a la cámara.
        if (camaraPrimeraPersona != null)
        {
            camaraPrimeraPersona.sensibilidadX = sensibilidadGuardada;
            camaraPrimeraPersona.sensibilidadY = sensibilidadGuardada;
        }

        // Refrescamos textos.
        AplicarOpcionesDesdeSliders();
    }
}