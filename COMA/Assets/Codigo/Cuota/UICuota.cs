using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UICuota : MonoBehaviour
{
    [Header("Referencias")]
    public ProgresoJugador progreso;
    public TMP_Text textoCuota;
    public Slider barraCuota;

    void Start()
    {
        if(progreso != null)
        {
            progreso.AlCambiarDinero += ActualizarUI;
            ActualizarUI();
        }

    }
    void OnDestroy()
    {
        if(progreso != null)
        progreso.AlCambiarDinero -= ActualizarUI;

    }

    public void ActualizarUI()
    {
        if(progreso == null)
        return;

        if(textoCuota != null){
        textoCuota.text = "Cuota: " + progreso.dineroTotal + " / $" + progreso.cuotaObjetivo;


    }
    if(barraCuota!= null)
        {
            barraCuota.value = progreso.ObtenerProgresoCuota();
        }

}
}