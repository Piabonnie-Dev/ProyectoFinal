using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICuota : MonoBehaviour
{
    [Header("Referencias")]
    public ProgresoJugador progreso;
    public TMP_Text textoCuota;
    public Slider barraCuota;

    private void Start()
    {
        // Nos suscribimos a los cambios de progreso.
        if (progreso != null)
        {
            progreso.AlCambiarDinero += ActualizarUI;
            progreso.AlCumplirCuota += ActualizarUI;

            ActualizarUI();
        }
    }

    private void OnDestroy()
    {
        // Retiramos las suscripciones al cambiar de escena.
        if (progreso != null)
        {
            progreso.AlCambiarDinero -= ActualizarUI;
            progreso.AlCumplirCuota -= ActualizarUI;
        }
    }

    public void ActualizarUI()
    {
        if (progreso == null)
        {
            return;
        }

        // Configuramos la barra entre cero y uno.
        if (barraCuota != null)
        {
            barraCuota.minValue = 0f;
            barraCuota.maxValue = 1f;
            barraCuota.value = progreso.ObtenerProgresoCuota();
        }

        if (textoCuota == null)
        {
            return;
        }

        // Cuando cumple la cuota, damos una instrucción útil.
        if (progreso.CuotaCumplida)
        {
            textoCuota.text =
                "CUOTA CUMPLIDA\nREGRESA A LA REJA";
        }
        else
        {
            textoCuota.text =
                "Cuota: $" +
                progreso.valorExtraidoParaCuota +
                " / $" +
                progreso.cuotaObjetivo;
        }
    }
}