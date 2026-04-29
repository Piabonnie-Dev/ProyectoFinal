using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgresoJugador : MonoBehaviour
{
[Header("Economia")]
public int dineroTotal = 0;

public void AgregarDinero (int cantidad)
    {
        dineroTotal += cantidad;
        Debug.Log("Dinero total:" + dineroTotal);
    }
}
