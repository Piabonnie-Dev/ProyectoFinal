
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;
    public bool estaMuerto = false;

    void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDanio(float cantidad)
    {
        if (estaMuerto)
            return;

        vidaActual -= cantidad;

        Debug.Log("Jugador recibió " + cantidad + " de daño. Vida actual: " + vidaActual);

        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    void Morir()
    {
        estaMuerto = true;
        vidaActual = 0f;

        Debug.Log("El jugador murió.");
    }
}