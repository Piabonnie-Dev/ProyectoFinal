using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine;  

public class CamaraManager : MonoBehaviour
{
    [Header("UI del monitor")]
    public GameObject panelMonitorCamaras;
    public RawImage pantallaCamara;
    public TMP_Text textoCamara;

    [Header("Render Texture")]
    public RenderTexture texturaMonitor;

    [Header("Controles")]
    public KeyCode teclaAbrirMonitor = KeyCode.T;
    public KeyCode teclaSiguienteCamara = KeyCode.RightArrow;
  public KeyCode teclaAnteriorCamara = KeyCode.LeftArrow;

[Header("Camaras registradas")]
public List<CamaraColocable> camarasColocadas = new List<CamaraColocable>();

private int indiceCamaraActual = 0;
private bool monitorAbierto = false;


private void Start()
    {
        if(panelMonitorCamaras != null)
        {
            panelMonitorCamaras.SetActive(false);

        }
        if(pantallaCamara != null)
        {
            pantallaCamara.texture = texturaMonitor;

        }
    }

    private void Update()
    {
        
        if(Input.GetKeyDown(teclaAbrirMonitor))
        {
            AlternarMonitor();
            
        }

        if (monitorAbierto)
        {
            if (Input.GetKeyDown(teclaSiguienteCamara))
            {
                SiguienteCamara();
            }
            else if (Input.GetKeyDown(teclaAnteriorCamara))
            {
                AnteriorCamara();
            }
        }
    }

    public void RegistrarCamara(CamaraColocable nuevaCamara)
    {
        if(nuevaCamara == null) return;

        if(camarasColocadas.Contains(nuevaCamara)) return;

        camarasColocadas.Add(nuevaCamara);
        nuevaCamara.nombreCamara = "Camara" + camarasColocadas.Count;
    }

   private void AlternarMonitor()
    {
        // Si no hay cámaras, mostramos aviso y no abrimos monitor.
        if (camarasColocadas.Count == 0)
        {
            ActualizarTexto("No hay cámaras colocadas");
            return;
        }

        // Cambiamos el estado del monitor.
        monitorAbierto = !monitorAbierto;

        // Activamos o desactivamos el panel.
        if (panelMonitorCamaras != null)
        {
            panelMonitorCamaras.SetActive(monitorAbierto);
        }

        // Si se abre, activamos la cámara actual.
        if (monitorAbierto)
        {
            ActivarCamaraActual();
        }
        else
        {
            ApagarTodasLasCamaras();
        }
    }

    private void ActivarCamaraActual()
    {
        // Primero apagamos todas para que solo una renderice.
        ApagarTodasLasCamaras();

        // Limpiamos cámaras destruidas o nulas.
        LimpiarListaCamaras();

        // Si ya no quedan cámaras, cerramos monitor.
        if (camarasColocadas.Count == 0)
        {
            monitorAbierto = false;

            if (panelMonitorCamaras != null)
            {
                panelMonitorCamaras.SetActive(false);
            }

            ActualizarTexto("No hay cámaras disponibles");
            return;
        }

        // Nos aseguramos de que el índice esté dentro del rango.
        indiceCamaraActual = Mathf.Clamp(indiceCamaraActual, 0, camarasColocadas.Count - 1);

        CamaraColocable camaraActual = camarasColocadas[indiceCamaraActual];

        // Si la cámara existe y está activa, la encendemos.
        if (camaraActual != null && camaraActual.camaraActiva)
        {
            camaraActual.EncenderCamara(texturaMonitor);

            ActualizarTexto(camaraActual.nombreCamara + "  [" + (indiceCamaraActual + 1) + " / " + camarasColocadas.Count + "]");
        }
        else
        {
            ActualizarTexto("Cámara dañada o no disponible");
        }
    }

    private void SiguienteCamara()
    {
        if (camarasColocadas.Count == 0) return;

        // Avanzamos a la siguiente cámara.
        indiceCamaraActual++;

        // Si nos pasamos, regresamos a la primera.
        if (indiceCamaraActual >= camarasColocadas.Count)
        {
            indiceCamaraActual = 0;
        }

        ActivarCamaraActual();
    }

    private void AnteriorCamara()
    {
        if (camarasColocadas.Count == 0) return;

        // Retrocedemos una cámara.
        indiceCamaraActual--;

        // Si bajamos de cero, vamos a la última.
        if (indiceCamaraActual < 0)
        {
            indiceCamaraActual = camarasColocadas.Count - 1;
        }

        ActivarCamaraActual();
    }

    private void ApagarTodasLasCamaras()
    {
        // Apagamos cada cámara registrada.
        foreach (CamaraColocable camara in camarasColocadas)
        {
            if (camara != null)
            {
                camara.ApagarCamara();
            }
        }
    }

    private void LimpiarListaCamaras()
    {
        // Eliminamos referencias vacías por si alguna cámara fue destruida.
        camarasColocadas.RemoveAll(camara => camara == null);
    }

    private void ActualizarTexto(string mensaje)
    {
        // Actualizamos el texto solo si está asignado.
        if (textoCamara != null)
        {
            textoCamara.text = mensaje;
        }
    }
}
