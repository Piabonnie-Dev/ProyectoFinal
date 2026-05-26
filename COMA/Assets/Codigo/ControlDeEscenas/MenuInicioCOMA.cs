
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicioCOMA : MonoBehaviour
{
    
    [Header("Escena del juego")]
    public string nombreEscenaJuego = "Mapa";

    [Header("Paneles Principales")]
    public GameObject panelMenu;
    public GameObject panelTutorial;
    public GameObject panelOpciones;


    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;

        Cursor.lockState = CursorLockMode.None;

        MostrarMenuPrincipal();
    }

    public void Jugar()
    {
        
        SceneManager.LoadScene(nombreEscenaJuego);
    }

public void MostrarMenuPrincipal()
    {
        if(panelMenu != null)
        panelMenu.SetActive(true);


        if(panelTutorial != null)
     panelTutorial.SetActive(false);

        if(panelOpciones != null)
            panelOpciones.SetActive(false);
    }

public void MostrarTutorial()
    {
        if(panelMenu != null)
        panelMenu.SetActive(false);

        if(panelTutorial != null)
     panelTutorial.SetActive(true);

        if(panelOpciones != null)
            panelOpciones.SetActive(false);
    }
    public void MostrarOpciones()
    {
        // Ocultamos el menú principal.
        if (panelMenu != null)
            panelMenu.SetActive(false);

        // Ocultamos el tutorial.
        if (panelTutorial != null)
            panelTutorial.SetActive(false);

        // Mostramos las opciones.
        if (panelOpciones != null)
            panelOpciones.SetActive(true);
    
     
    }


    public void Salir()
    {
        //Este mensaje solo aparece dentro del editor de unity
        Debug.Log("Saliendo de COMA...");
       
        //Cierra el juego cuando este compilado
        Application.Quit();
    }
}
