using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuInicioCOMA : MonoBehaviour
{
    [Header("Escena del juego")]
    public string nombreEscenaJuego = "Mapa";

    [Header("Paneles Principales")]
    public GameObject panelMenu;
    public GameObject panelTutorial;
    public GameObject panelOpciones;

    [Header("Opciones")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    [Header("Textos de opciones")]
    public TMP_Text textoVolumen;
    public TMP_Text textoSensibilidad;

    private void Start()
    {
        // En el menú principal el tiempo debe estar normal.
        Time.timeScale = 1f;

        // En el menú principal el cursor debe verse.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Cargamos los valores guardados de volumen y sensibilidad.
        CargarOpcionesGuardadas();

        // Conectamos los sliders por código para evitar errores en el OnValueChanged.
        ConectarSliders();

        // Mostramos el menú principal.
        MostrarMenuPrincipal();
    }

    private void ConectarSliders()
    {
        // Limpiamos y conectamos el slider de volumen.
        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.RemoveAllListeners();
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }

        // Limpiamos y conectamos el slider de sensibilidad.
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.onValueChanged.RemoveAllListeners();
            sliderSensibilidad.onValueChanged.AddListener(CambiarSensibilidad);
        }
    }

    public void Jugar()
    {
        // Aseguramos que el juego no cargue pausado.
        Time.timeScale = 1f;

        // Cargamos la escena del mapa.
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void MostrarMenuPrincipal()
    {
        // Activamos el panel del menú.
        if (panelMenu != null)
            panelMenu.SetActive(true);

        // Ocultamos tutorial.
        if (panelTutorial != null)
            panelTutorial.SetActive(false);

        // Ocultamos opciones.
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
    }

    public void MostrarTutorial()
    {
        // Ocultamos menú.
        if (panelMenu != null)
            panelMenu.SetActive(false);

        // Mostramos tutorial.
        if (panelTutorial != null)
            panelTutorial.SetActive(true);

        // Ocultamos opciones.
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
    }

    public void MostrarOpciones()
    {
        // Ocultamos menú.
        if (panelMenu != null)
            panelMenu.SetActive(false);

        // Ocultamos tutorial.
        if (panelTutorial != null)
            panelTutorial.SetActive(false);

        // Mostramos opciones.
        if (panelOpciones != null)
            panelOpciones.SetActive(true);
    }

    public void CambiarVolumen(float valor)
    {
        // Cambiamos el volumen general.
        AudioListener.volume = valor;

        // Guardamos el volumen.
        PlayerPrefs.SetFloat("VolumenGeneral", valor);
        PlayerPrefs.Save();

        // Actualizamos texto.
        if (textoVolumen != null)
        {
            int porcentaje = Mathf.RoundToInt(valor * 100f);
            textoVolumen.text = "Volumen: " + porcentaje + "%";
        }
    }

    public void CambiarSensibilidad(float valor)
    {
        // Guardamos la sensibilidad para usarla en el mapa.
        PlayerPrefs.SetFloat("SensibilidadMouse", valor);
        PlayerPrefs.Save();

        // Actualizamos texto.
        if (textoSensibilidad != null)
        {
            textoSensibilidad.text = "Sensibilidad: " + Mathf.RoundToInt(valor);
        }
    }

    private void CargarOpcionesGuardadas()
    {
        // Si no hay volumen guardado, usamos 1.
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenGeneral", 1f);

        // Si no hay sensibilidad guardada, usamos 180.
        float sensibilidadGuardada = PlayerPrefs.GetFloat("SensibilidadMouse", 180f);

        // Aplicamos el volumen.
        AudioListener.volume = volumenGuardado;

        // Colocamos el valor en el slider de volumen.
        if (sliderVolumen != null)
            sliderVolumen.value = volumenGuardado;

        // Colocamos el valor en el slider de sensibilidad.
        if (sliderSensibilidad != null)
            sliderSensibilidad.value = sensibilidadGuardada;

        // Actualizamos textos.
        CambiarVolumen(volumenGuardado);
        CambiarSensibilidad(sensibilidadGuardada);
    }

    public void Salir()
    {
        // Mensaje visible solo en Unity.
        Debug.Log("Saliendo de COMA...");

        // Cierra el juego en build.
        Application.Quit();
    }
}