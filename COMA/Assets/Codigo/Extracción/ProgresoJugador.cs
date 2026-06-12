using System;
using UnityEngine;

public class ProgresoJugador : MonoBehaviour
{
    [Header("Economía")]
    public int dineroTotal = 0;

    [Header("Cuota")]
    public int cuotaObjetivo = 300;

    public event Action AlCambiarDinero;
    public event Action AlCumplirCuota;

    private bool cuotaYaCumplida = false;

    public void AgregarDinero(int cantidad)
    {
        dineroTotal += cantidad;

        Debug.Log("Dinero total actual: $" + dineroTotal);

        AlCambiarDinero?.Invoke();

        if (!cuotaYaCumplida && dineroTotal >= cuotaObjetivo)
        {
            cuotaYaCumplida = true;
            Debug.Log("¡Cuota cumplida!");
            AlCumplirCuota?.Invoke();
        }
    }

    public bool GastarDinero(int cantidad)
    {
        if (cantidad <= 0)
            return true;

        if (dineroTotal < cantidad)
        {
            Debug.Log("No tienes suficiente dinero. Necesitas $" + cantidad);
            return false;
        }

        dineroTotal -= cantidad;
        Debug.Log("Gastaste $" + cantidad + ". Dinero restante: $" + dineroTotal);

        AlCambiarDinero?.Invoke();
        return true;
    }

    public float ObtenerProgresoCuota()
    {
        if (cuotaObjetivo <= 0)
            return 1f;

        return Mathf.Clamp01((float)dineroTotal / cuotaObjetivo);
    }
}
