
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuInicio : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    [Header("Nombre de la escena del juego")]
    public string nombreEscenaJuego = "Mapa";

    [Header("Opciones")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    [Header("Textos")]
    public TMP_Text textoVolumen;
    public TMP_Text textoSensibilidad;

    private void Start()
    {
        
        EstadoJuego.PrepararMenuPrincipal();

        if(panelPrincipal != null)
        panelPrincipal.SetActive(true);

        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        CargarOpcionesGuardadas();
    }

    public void Jugar()
    {
        Time.timeScale = 1f;
        EstadoJuego.JuegoPausado = false;

        SceneManager.LoadScene(nombreEscenaJuego);

    }

    public void AbrirOpciones()
    {
        
        if(panelPrincipal != null)
        panelPrincipal.SetActive(false);

        if(panelOpciones != null)
        panelOpciones.SetActive(true);

    }

    public void CerrarOpciones()
    {
        
        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        if(panelPrincipal != null)
        panelPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        Debug.Log("Salir del juego.");
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;

        PlayerPrefs.SetFloat("VolumenGeneral", valor);
        PlayerPrefs.Save();

        if(textoVolumen != null)
        {
            int porcentaje = Mathf.RoundToInt(valor * 100f);
            textoVolumen.text = "Volumen:" + porcentaje + "%";
        }

    }

    public void CambiarSensibilidad(float valor)
    {
        PlayerPrefs.SetFloat("Sensiblidad", valor);
        PlayerPrefs.Save();

        if(textoSensibilidad != null)
        {
            textoSensibilidad.text = "Sensibildad:" + Mathf.RoundToInt(valor);

        }

    }

    private void CargarOpcionesGuardadas()
    {
        float volumen = PlayerPrefs.GetFloat("VolumenGeneral", 1f);
        float sensibilidad = PlayerPrefs.GetFloat("SensiblidadMouse", 180f);
    AudioListener.volume = volumen;

    if(sliderVolumen != null)
    sliderVolumen.value = volumen;

    if(sliderSensibilidad != null)
    sliderSensibilidad.value = sensibilidad;

    CambiarVolumen(volumen);
    CambiarSensibilidad(sensibilidad);
    
    }

}
