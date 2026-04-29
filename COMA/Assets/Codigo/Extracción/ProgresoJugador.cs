using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgresoJugador : MonoBehaviour
{
[Header("Economia")]
public int dineroTotal = 0;

[Header("Cuota")]
public int cuotaObjetivo = 300;

public event Action AlCambiarDinero;
public event Action AlCumplirCuota;

private bool cuotaYaCumplida = false;


public void AgregarDinero (int cantidad)
    {
        dineroTotal += cantidad;
        Debug.Log("Dinero total:" + dineroTotal);
        AlCambiarDinero?.Invoke();

        if (!cuotaYaCumplida && dineroTotal >= cuotaObjetivo)
        {
            cuotaYaCumplida = true;
            Debug.Log("¡Cuota cumplida!");
            AlCumplirCuota?.Invoke();
        }
    }

    public float ObtenerProgresoCuota()
    {
        if(cuotaObjetivo <= 0)
        return 1f;

        return Mathf.Clamp01((float) dineroTotal/ cuotaObjetivo);
        
    }
}
