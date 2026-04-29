using TMPro;
using UnityEngine;

public class ControlPartida : MonoBehaviour
{
    public static bool PartidaTerminada
    {
        get; private set;
    }

   [Header("Referencias")]
   public ProgresoJugador progreso;
   public GameObject panelVictoria;
   public TMP_Text textoVictoria;


   void Start()
    {
        PartidaTerminada = false;

        if(panelVictoria != null)
        panelVictoria.SetActive(false);

        if(progreso!= null)
        progreso.AlCumplirCuota += TerminarPartidaPorCuota;

    } 
    void OnDestroy()
    {
        if(progreso!= null)
        progreso.AlCumplirCuota -= TerminarPartidaPorCuota;
    }

    void TerminarPartidaPorCuota()
    {
        PartidaTerminada = true;
        
        if(panelVictoria != null)
        panelVictoria.SetActive(true);

        if(textoVictoria != null)
        {
            textoVictoria.text = "Cuota CUMPLIDA\n Partida terminada";
     
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
public void ReiniciarTiempo()
    {
        Time.timeScale = 1f;
        PartidaTerminada = false;
    }


}
