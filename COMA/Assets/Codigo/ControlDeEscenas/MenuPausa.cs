
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPausa : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelOpciones;

    [Header("Nombre de escenas")]
    public string nombreEscenaMenuInicio = "MenuInicio";

    [Header("Opciones")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    [Header("Textos de opciones")]
    public TMP_Text textoVolumen;
    public TMP_Text textoSensibilidad;

    [Header("Camara FPS del jugador")]
    public CamaraPrimeraPersona camaraFPS;

    private void Start()
    {
        //El menu de pausa inicia cerrado.
        if(panelPausa != null)
        panelPausa.SetActive(false);
        
        //El panel de opciones tambien inicia cerrado.
        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        //Cargamos valores guardados
CargarOpcionesGuardadas();

//Al iniciar el mapa, el juego debe estar corriendo.
EstadoJuego.ReanudarJuego();    }



private void Update()
    {
        
        if(ControlPartida.PartidaTerminada)
        return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EstadoJuego.JuegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        if(panelPausa != null)
        panelPausa.SetActive(true);

        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        EstadoJuego.PausarJuego();
    }

    public void Reanudar()
    {
        
        if(panelPausa != null)
        panelPausa.SetActive(false);

        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        EstadoJuego.ReanudarJuego();
    }


    public void AbrirOpciones()
    {
        if(panelPausa != null)
        panelPausa.SetActive(false);

        if(panelOpciones != null)
        panelOpciones.SetActive(true);


    }

    public void CerrarOpciones()
    {
        if(panelOpciones != null)
        panelOpciones.SetActive(false);

        if(panelPausa != null)
        panelPausa.SetActive(true);
    }

    public void ReiniciarEscena()
    {
        
        Time.timeScale = 1f;
        EstadoJuego.JuegoPausado = false;

        SceneManager.LoadScene(nombreEscenaMenuInicio);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        EstadoJuego.JuegoPausado = false;

        SceneManager.LoadScene(nombreEscenaMenuInicio);
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
        
        PlayerPrefs.SetFloat("SensibilidadMouse", valor);
        PlayerPrefs.Save();


        if(camaraFPS != null)
        {
            camaraFPS.sensibilidadX = valor;
            camaraFPS.sensibilidadY = valor;
        }

        if(textoSensibilidad != null)
        {
           
            textoSensibilidad.text = "Sensibilidad:" + Mathf.RoundToInt(valor);
        }
    }


    private void CargarOpcionesGuardadas()
    {
        // Cargamos volumen guardado. Si no existe, usamos 1.
        float volumen = PlayerPrefs.GetFloat("VolumenGeneral", 1f);

        // Cargamos sensibilidad guardada. Si no existe, usamos 180.
        float sensibilidad = PlayerPrefs.GetFloat("SensibilidadMouse", 180f);

        // Aplicamos volumen.
        AudioListener.volume = volumen;

        // Aplicamos sensibilidad a la cámara.
        if (camaraFPS != null)
        {
            camaraFPS.sensibilidadX = sensibilidad;
            camaraFPS.sensibilidadY = sensibilidad;
        }

        // Actualizamos slider de volumen.
        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumen;
        }

        // Actualizamos slider de sensibilidad.
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = sensibilidad;
        }

        // Actualizamos textos.
        CambiarVolumen(volumen);
        CambiarSensibilidad(sensibilidad);
    }
}
