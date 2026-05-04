
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;
    public bool estaMuerto = false;

    [Header("Referencias opcionales")]
    public ControlPartida controlPartida;
    public GameObject visualJugador;
    public CharacterController controllerJugador;

    void Awake()
    {
        vidaActual = vidaMaxima;

        if (controllerJugador == null)
            controllerJugador = GetComponent<CharacterController>();

        if (controlPartida == null)
            controlPartida = FindObjectOfType<ControlPartida>();
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
        if (estaMuerto)
            return;

        estaMuerto = true;
        vidaActual = 0f;

        Debug.Log("El jugador murió.");
        Debug.Log("Intentando avisar a ControlPartida...");

        if (controllerJugador != null)
            controllerJugador.enabled = false;

        if (visualJugador != null)
            visualJugador.SetActive(false);

        if (controlPartida != null)
        {
            controlPartida.TerminarPartidaPorMuerte();
        }
        else
        {
            Debug.LogWarning("No se encontró ControlPartida.");
        }
    }
}