using System;
using UnityEngine;

public class ProgresoJugador : MonoBehaviour
{
    [Header("Economía disponible")]
    public int dineroTotal = 0;

    [Header("Progreso de extracción")]
    public int valorExtraidoParaCuota = 0;
    public int cuotaObjetivo = 300;

    [Header("Estado")]
    [SerializeField] private bool cuotaYaCumplida = false;

    // Se ejecuta cuando cambia el dinero o el progreso de cuota.
    public event Action AlCambiarDinero;

    // Se ejecuta una sola vez cuando se alcanza la cuota.
    public event Action AlCumplirCuota;

    // Permite consultar desde otros scripts si la cuota ya está cumplida.
    public bool CuotaCumplida
    {
        get
        {
            return cuotaYaCumplida ||
                   valorExtraidoParaCuota >= cuotaObjetivo;
        }
    }

    private void Awake()
    {
        // Sincronizamos el estado por si los valores fueron cambiados
        // directamente desde el Inspector.
        cuotaYaCumplida =
            valorExtraidoParaCuota >= cuotaObjetivo;
    }

    public void AgregarDinero(int cantidad)
    {
        // No aceptamos cantidades inválidas.
        if (cantidad <= 0)
        {
            return;
        }

        // El valor extraído se suma al dinero disponible.
        dineroTotal += cantidad;

        // También se suma al progreso acumulado de cuota.
        valorExtraidoParaCuota += cantidad;

        Debug.Log(
            "Extracción recibida: $" + cantidad +
            "\nDinero disponible: $" + dineroTotal +
            "\nProgreso de cuota: $" +
            valorExtraidoParaCuota + " / $" + cuotaObjetivo
        );

        // Avisamos a todas las interfaces.
        AlCambiarDinero?.Invoke();

        // La cuota solamente se marca una vez.
        if (!cuotaYaCumplida &&
            valorExtraidoParaCuota >= cuotaObjetivo)
        {
            cuotaYaCumplida = true;

            Debug.Log(
                "¡Cuota cumplida! La reja de salida está disponible."
            );

            AlCumplirCuota?.Invoke();
        }
    }

    public bool GastarDinero(int cantidad)
    {
        // Gastar cero no produce cambios.
        if (cantidad <= 0)
        {
            return true;
        }

        // Comprobamos que exista dinero suficiente.
        if (dineroTotal < cantidad)
        {
            Debug.Log(
                "No tienes suficiente dinero. Necesitas $" + cantidad
            );

            return false;
        }

        // Gastar dinero NO reduce el progreso de cuota.
        dineroTotal -= cantidad;

        Debug.Log(
            "Gastaste $" + cantidad +
            ". Dinero restante: $" + dineroTotal
        );

        AlCambiarDinero?.Invoke();

        return true;
    }

    public float ObtenerProgresoCuota()
    {
        // Evitamos divisiones entre cero.
        if (cuotaObjetivo <= 0)
        {
            return 1f;
        }

        return Mathf.Clamp01(
            (float)valorExtraidoParaCuota / cuotaObjetivo
        );
    }

    public int ObtenerCantidadFaltante()
    {
        return Mathf.Max(
            0,
            cuotaObjetivo - valorExtraidoParaCuota
        );
    }

    public void ReiniciarProgresoPartida()
    {
        // Útil si después deseas reiniciar sin recargar escena.
        dineroTotal = 0;
        valorExtraidoParaCuota = 0;
        cuotaYaCumplida = false;

        AlCambiarDinero?.Invoke();
    }
}