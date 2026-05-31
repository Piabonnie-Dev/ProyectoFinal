
using UnityEngine;

public class CamaraColocable : MonoBehaviour
{
    [Header("Datos de la camara")]
    public string nombreCamara = "Camara colocada";

    [Header("Referencias a la camara interna")]
    public Camera camaraInterna;

    [Header("Estado")]
    public bool camaraActiva = true;

    private void Awake()
    {
        
        if(camaraInterna == null)
        {
            camaraInterna = GetComponentInChildren<Camera>();
        }

        if(camaraInterna != null)
        {
            camaraInterna.enabled = false;

        }

    }

   public void EncenderCamara(RenderTexture texturaMonitor)
    {
        // Si la cámara está rota o desactivada, no hacemos nada.
        if (!camaraActiva) return;

        // Asignamos la Render Texture para que la vista salga en la UI.
        camaraInterna.targetTexture = texturaMonitor;

        // Encendemos esta cámara.
        camaraInterna.enabled = true;
    }

    public void ApagarCamara()
    {
        if(camaraInterna == null) return;
        camaraInterna.enabled = false;
        camaraInterna.targetTexture = null;
    }

    public void RomperCamara()
    {
        camaraActiva = false;
        ApagarCamara();
    }
}
