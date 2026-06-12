using UnityEngine;

public class ZonaVictoriaFinal : MonoBehaviour
{
    [Header("Referencias")]
    public RejaSalidaFinal rejaSalida;
    public ControlPartida controlPartida;
    public ProgresoJugador progreso;

    [Header("Estado")]
    [SerializeField] private bool victoriaActivada = false;

    private void OnTriggerEnter(Collider other)
    {
        IntentarActivarVictoria(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Sirve como respaldo si el jugador ya estaba dentro
        // cuando la reja terminó de abrirse.
        IntentarActivarVictoria(other);
    }

    private void IntentarActivarVictoria(Collider other)
    {
        // Solo aceptamos al Player o colliders hijos del Player.
        if (!EsJugador(other))
        {
            return;
        }

        // Evitamos finalizar varias veces.
        if (victoriaActivada)
        {
            return;
        }

        // Comprobamos que la reja exista y esté abierta.
        if (rejaSalida == null)
        {
            Debug.LogWarning(
                "ZonaVictoriaFinal no tiene RejaSalidaFinal asignada."
            );

            return;
        }

        if (!rejaSalida.EstaAbierta)
        {
            Debug.Log(
                "El jugador llegó a la salida, pero la reja sigue cerrada."
            );

            return;
        }

        // Comprobación adicional de cuota.
        if (progreso != null && !progreso.CuotaCumplida)
        {
            Debug.Log(
                "El jugador llegó a la salida sin cumplir la cuota."
            );

            return;
        }

        if (controlPartida == null)
        {
            Debug.LogWarning(
                "ZonaVictoriaFinal no tiene ControlPartida asignado."
            );

            return;
        }

        victoriaActivada = true;

        Debug.Log(
            "Jugador cruzó la salida. Activando victoria final."
        );

        controlPartida.TerminarPartidaPorVictoria();
    }

    private bool EsJugador(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        if (other.transform.root.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }
}