using UnityEngine.AI;
using UnityEngine;

public class EnemyV19AI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Camera camaraJugador;
    public NavMeshAgent agente;

    [Header("Puntos de patrulla")]
    public Transform[] puntosPatrulla;

    [Header("Capas")]
    public LayerMask capaObstaculos;

    [Header("Deteccion por ruido")]
    public float distanciaEscuchaNormal = 8f;
    public float distanciaEscuchaCorriendo = 18f;
    public float sensibilidadRuido =1f;

    [Header("Deteccion por luz / mirada")]
    public float distanciaReaccionMirada = 16f;
    public float anguloMiradaJugador = 25f;

[Header("Movimiento")]
public float velocidadPatrulla = 1.8f;
public float velocidadAlerta = 3.2f;
public float velocidadPersecucion = 7f;

    [Header(" Ataque")]
    public float distanciaAtaque = 1.8f;
    public float danioAtaque = 25f;
    public float tiempoEntreAtaques = 1.2f;

    [Header("Calma")]
    public float tiempoParaCalmarse = 5f;

    [Header("Estado actual")]
    public EstadoV19 estadoActual = EstadoV19.Patrulla;

    private int indicePatrulla = 0;
    private float contadorSinEstimulo = 0f;
    private float contadorAtaque = 0f;
    private Vector3 ultimaPosicionRuido;
    private bool estaStuneado = false;
    private float contadorStun = 0f;
    private bool estaMuerto = false;

    public enum EstadoV19
    {
        Patrulla,
        Alerta,
        Persecucion
    }

    private void Start()
    {
        if(agente == null)
        {
            agente = GetComponent<NavMeshAgent>();

        }

        if(jugador != null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if(objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
        }

        if(camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        agente.speed = velocidadPatrulla;
        IrAlSiguientePuntoPatrulla();
    }


private void Update()
    {
        // Si está muerto, no hace nada.
        if (estaMuerto) return;

        // Si está stuneado, pausamos su comportamiento.
        if (estaStuneado)
        {
            ProcesarStun();
            return;
        }

        // Bajamos el contador de ataque.
        if (contadorAtaque > 0f)
        {
            contadorAtaque -= Time.deltaTime;
        }

        // Evaluamos estímulos del jugador.
        bool escuchoJugador = DetectarRuidoDelJugador();
        bool reaccionoAMirada = DetectarMiradaDelJugador();

        // Si detecta ruido o mirada, se altera.
        if (escuchoJugador || reaccionoAMirada)
        {
            contadorSinEstimulo = 0f;

            if (estadoActual != EstadoV19.Persecucion)
            {
                CambiarEstado(EstadoV19.Persecucion);
            }
        }
        else
        {
            contadorSinEstimulo += Time.deltaTime;
        }

        // Ejecutamos el comportamiento del estado actual.
        switch (estadoActual)
        {
            case EstadoV19.Patrulla:
                EstadoPatrulla();
                break;

            case EstadoV19.Alerta:
                EstadoAlerta();
                break;

            case EstadoV19.Persecucion:
                EstadoPersecucion();
                break;
        }
    }

    private void EstadoPatrulla()
    {
        // En patrulla, V19 se mueve lento.
        agente.speed = velocidadPatrulla;

        // Si llegó al punto de patrulla, va al siguiente.
        if (!agente.pathPending && agente.remainingDistance <= 0.5f)
        {
            IrAlSiguientePuntoPatrulla();
        }
    }

    private void EstadoAlerta()
    {
        // En alerta, V19 se mueve hacia el último ruido.
        agente.speed = velocidadAlerta;

        // Va a revisar la última posición del ruido.
        agente.SetDestination(ultimaPosicionRuido);

        // Si pasa suficiente tiempo sin estímulos, se calma.
        if (contadorSinEstimulo >= tiempoParaCalmarse)
        {
            CambiarEstado(EstadoV19.Patrulla);
        }
    }

    private void EstadoPersecucion()
    {
        // En persecución, V19 corre rápido.
        agente.speed = velocidadPersecucion;

        // Persigue directamente al jugador.
        agente.SetDestination(jugador.position);

        // Si está cerca, ataca.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque)
        {
            AtacarJugador();
        }

        // Si deja de recibir estímulos, baja a alerta.
        if (contadorSinEstimulo >= tiempoParaCalmarse)
        {
            CambiarEstado(EstadoV19.Alerta);
        }
    }

    private bool DetectarRuidoDelJugador()
    {
        // Si no hay jugador, no puede detectar ruido.
        if (jugador == null) return false;

        // Medimos distancia al jugador.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Detectamos si el jugador está corriendo con Shift.
        bool jugadorCorriendo = Input.GetKey(KeyCode.LeftShift);

        // Detectamos si el jugador se está moviendo.
        bool jugadorMoviendose =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        // Si está quieto, genera menos ruido.
        if (!jugadorMoviendose)
        {
            return false;
        }

        // Si corre, V19 lo escucha desde más lejos.
        float distanciaEscucha = jugadorCorriendo ? distanciaEscuchaCorriendo : distanciaEscuchaNormal;

        // Si está dentro de la distancia de escucha, detecta el ruido.
        if (distancia <= distanciaEscucha * sensibilidadRuido)
        {
            ultimaPosicionRuido = jugador.position;
            return true;
        }

        return false;
    }

    private bool DetectarMiradaDelJugador()
    {
        // Si no hay cámara del jugador, no detectamos mirada.
        if (camaraJugador == null) return false;

        // Medimos distancia entre jugador y enemigo.
        float distancia = Vector3.Distance(camaraJugador.transform.position, transform.position);

        // Si está demasiado lejos, no reacciona.
        if (distancia > distanciaReaccionMirada)
        {
            return false;
        }

        // Calculamos dirección de la cámara hacia V19.
        Vector3 direccionHaciaV19 = transform.position - camaraJugador.transform.position;

        // Calculamos si el jugador lo está mirando de frente.
        float angulo = Vector3.Angle(camaraJugador.transform.forward, direccionHaciaV19);

        if (angulo > anguloMiradaJugador)
        {
            return false;
        }

        // Revisamos si hay pared entre la cámara y V19.
        if (Physics.Raycast(camaraJugador.transform.position, direccionHaciaV19.normalized, out RaycastHit hit, distanciaReaccionMirada, capaObstaculos))
        {
            return false;
        }

        // Si el jugador lo mira dentro del ángulo, V19 se altera.
        ultimaPosicionRuido = jugador.position;
        return true;
    }

    public void RecibirRuido(Vector3 posicionRuido)
    {
        // Esta función sirve para que después tus objetos lanzados o trampas hagan ruido.
        ultimaPosicionRuido = posicionRuido;

        // Si estaba patrullando, pasa a alerta.
        if (estadoActual == EstadoV19.Patrulla)
        {
            CambiarEstado(EstadoV19.Alerta);
        }
    }

    private void AtacarJugador()
    {
        // Evitamos ataques demasiado rápidos.
        if (contadorAtaque > 0f) return;

        // Reiniciamos contador de ataque.
        contadorAtaque = tiempoEntreAtaques;

        // Mandamos daño al PlayerHealth sin depender del nombre exacto de tu función.
        if (jugador != null)
        {
            jugador.SendMessage("RecibirDaño", danioAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("RecibirDanio", danioAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("TakeDamage", danioAtaque, SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("V19 atacó al jugador.");
    }

    private void IrAlSiguientePuntoPatrulla()
    {
        // Si no hay puntos de patrulla, no patrulla.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;

        // Mandamos al agente al punto actual.
        agente.SetDestination(puntosPatrulla[indicePatrulla].position);

        // Avanzamos al siguiente punto.
        indicePatrulla++;

        // Si se pasa del último, regresa al primero.
        if (indicePatrulla >= puntosPatrulla.Length)
        {
            indicePatrulla = 0;
        }
    }

    private void CambiarEstado(EstadoV19 nuevoEstado)
    {
        // Cambiamos estado.
        estadoActual = nuevoEstado;

        // Reiniciamos calma cuando cambia de estado.
        contadorSinEstimulo = 0f;

        Debug.Log("V19 cambió a estado: " + nuevoEstado);
    }

    public void Stun(float duracion)
    {
        // Esta función permite que armas o trampas aturdan a V19.
        estaStuneado = true;

        // Guardamos duración del stun.
        contadorStun = duracion;

        // Detenemos al agente.
        agente.isStopped = true;
    }

    private void ProcesarStun()
    {
        // Bajamos el tiempo de stun.
        contadorStun -= Time.deltaTime;

        // Cuando termina el stun, vuelve a moverse.
        if (contadorStun <= 0f)
        {
            estaStuneado = false;

            agente.isStopped = false;

            CambiarEstado(EstadoV19.Alerta);
        }
    }

    public void Morir()
    {
        // Marcamos al enemigo como muerto.
        estaMuerto = true;

        // Apagamos el NavMeshAgent.
        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        // Apagamos este script.
        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Distancia de escucha normal.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaEscuchaNormal);

        // Distancia de escucha corriendo.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaEscuchaCorriendo);

        // Distancia de ataque.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}
