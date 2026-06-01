using UnityEngine.AI;
using UnityEngine;

public class EnemyV37AI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public NavMeshAgent agente;

    [Header("Puntos de patrulla")]
    public Transform[] puntosPatrulla;

    [Header("Capas")]
    public LayerMask capaObstaculos;

    [Header("Deteccion ")]
    public float distanciaVision = 18f;
    public float anguloVision = 90f;
    public float distanciaAcecho = 14f;
    public float tiempoAntesDeEmboscada = 4f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadAcecho = 1.4f;
    public float velocidadEmboscada = 10f;

    [Header("Ataque")]
    public float distanciaAtaque = 2.2f;
    public float danioAtaque = 75f;
    public float tiempoEntreAtaques = 1.5f;

    [Header("Recuperacion")]
    public float tiempoRecuperacionDespuesEmboscada =2f;

    [Header("Estado actual")]
public EstadoV37 estadoActual = EstadoV37.Patrulla;

private int indicePatrulla = 0;
private float contadorAcecho = 0f;
private float contadorAtaque = 0f;
private float contadorRecuperacion = 0f;
private bool estaStuneado = false;
private float contadorStun = 0f;
private bool estaMuerto = false;

public enum EstadoV37
    {
        Patrulla,
        Acecho,
        Emboscada,
        Recuperacion
    }
    private void Start()
    {
        if(agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
        }
        if(jugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if(objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
        }

        agente.speed = velocidadPatrulla;
        IrAlSiguientePuntoPatrulla();


    }

private void Update()
    {
        if(estaMuerto) return;

        if (estaStuneado)
        {
            ProcesarStun();
            return;

        }
        if(contadorAtaque > 0f)
        {
            contadorAtaque -= Time.deltaTime;
        }

        switch (estadoActual)
        {
            case EstadoV37.Patrulla :
                EstadoPatrulla();
                break;

                case EstadoV37.Acecho:
                EstadoAcecho();
                break;

                case EstadoV37.Emboscada:
                EstadoEmboscada();
                break;

            case EstadoV37.Recuperacion:
                EstadoRecuperacion();
                break;



        }
    }

    private void EstadoPatrulla()
    {
        // Durante patrulla, V37 se mueve lento.
        agente.speed = velocidadPatrulla;

        // Si ya llegó a su punto, cambia al siguiente.
        if (!agente.pathPending && agente.remainingDistance <= 0.5f)
        {
            IrAlSiguientePuntoPatrulla();
        }

        // Si ve al jugador o está suficientemente cerca, empieza a acechar.
        if (PuedeVerJugador() || Vector3.Distance(transform.position, jugador.position) <= distanciaAcecho)
        {
            CambiarEstado(EstadoV37.Acecho);
        }
    }

    private void EstadoAcecho()
    {
        // En acecho, V37 se mueve lento para dar tensión.
        agente.speed = velocidadAcecho;

        // V37 no corre todavía: se acerca poco a poco al jugador.
        agente.SetDestination(jugador.position);

        // Aumentamos el tiempo de acecho.
        contadorAcecho += Time.deltaTime;

        // Si acechó suficiente tiempo, entra en emboscada.
        if (contadorAcecho >= tiempoAntesDeEmboscada)
        {
            CambiarEstado(EstadoV37.Emboscada);
        }

        // Si el jugador se aleja mucho y no lo puede ver, regresa a patrulla.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaVision + 5f && !PuedeVerJugador())
        {
            CambiarEstado(EstadoV37.Patrulla);
        }
    }

    private void EstadoEmboscada()
    {
        // En emboscada, V37 hace un ataque rápido.
        agente.speed = velocidadEmboscada;

        // Corre directo hacia el jugador.
        agente.SetDestination(jugador.position);

        // Si está cerca, ataca.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque)
        {
            AtacarJugador();
            CambiarEstado(EstadoV37.Recuperacion);
        }
    }

    private void EstadoRecuperacion()
    {
        // Después de emboscar, se detiene un momento.
        agente.speed = 0f;

        contadorRecuperacion += Time.deltaTime;

        // Al terminar la recuperación, vuelve a acechar si el jugador sigue cerca.
        if (contadorRecuperacion >= tiempoRecuperacionDespuesEmboscada)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);

            if (distancia <= distanciaVision)
            {
                CambiarEstado(EstadoV37.Acecho);
            }
            else
            {
                CambiarEstado(EstadoV37.Patrulla);
            }
        }
    }

    private bool PuedeVerJugador()
    {
        // Si no hay jugador, no puede verlo.
        if (jugador == null) return false;

        // Calculamos dirección del enemigo hacia el jugador.
        Vector3 direccionAlJugador = jugador.position - transform.position;

        // Revisamos distancia máxima.
        if (direccionAlJugador.magnitude > distanciaVision)
        {
            return false;
        }

        // Revisamos si el jugador está dentro del cono de visión.
        float angulo = Vector3.Angle(transform.forward, direccionAlJugador);

        if (angulo > anguloVision * 0.5f)
        {
            return false;
        }

        // Lanzamos un raycast para comprobar si hay pared entre enemigo y jugador.
        Vector3 origenRayo = transform.position + Vector3.up * 1.5f;
        Vector3 direccionRayo = direccionAlJugador.normalized;

        if (Physics.Raycast(origenRayo, direccionRayo, out RaycastHit hit, distanciaVision, capaObstaculos))
        {
            // Si el rayo pega con una pared antes que con el jugador, no lo ve.
            return false;
        }

        // Si pasó todas las pruebas, sí puede ver al jugador.
        return true;
    }

    private void AtacarJugador()
    {
        // Evitamos atacar muchas veces seguidas.
        if (contadorAtaque > 0f) return;

        // Reiniciamos el contador de ataque.
        contadorAtaque = tiempoEntreAtaques;

        // Mandamos daño al PlayerHealth sin depender del nombre exacto de tu función.
        if (jugador != null)
        {
            jugador.SendMessage("RecibirDaño", danioAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("RecibirDanio", danioAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("TakeDamage", danioAtaque, SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("V37 atacó al jugador.");
    }

    private void IrAlSiguientePuntoPatrulla()
    {
        // Si no hay puntos de patrulla, no hacemos nada.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;

        // Mandamos al agente al punto actual.
        agente.SetDestination(puntosPatrulla[indicePatrulla].position);

        // Avanzamos al siguiente punto.
        indicePatrulla++;

        // Si llegamos al final, volvemos al primer punto.
        if (indicePatrulla >= puntosPatrulla.Length)
        {
            indicePatrulla = 0;
        }
    }

    private void CambiarEstado(EstadoV37 nuevoEstado)
    {
        // Cambiamos el estado.
        estadoActual = nuevoEstado;

        // Reiniciamos contadores importantes según el nuevo estado.
        if (nuevoEstado == EstadoV37.Acecho)
        {
            contadorAcecho = 0f;
        }

        if (nuevoEstado == EstadoV37.Recuperacion)
        {
            contadorRecuperacion = 0f;
        }

        Debug.Log("V37 cambió a estado: " + nuevoEstado);
    }

    public void Stun(float duracion)
    {
        // Esta función sirve para que tus armas o trampas puedan aturdir a V37.
        estaStuneado = true;

        // Guardamos cuánto tiempo durará el stun.
        contadorStun = duracion;

        // Detenemos al agente.
        agente.isStopped = true;
    }

    private void ProcesarStun()
    {
        // Bajamos el contador de stun.
        contadorStun -= Time.deltaTime;

        // Cuando termina el stun, V37 vuelve a moverse.
        if (contadorStun <= 0f)
        {
            estaStuneado = false;

            agente.isStopped = false;

            CambiarEstado(EstadoV37.Recuperacion);
        }
    }

    public void Morir()
    {
        // Marcamos al enemigo como muerto.
        estaMuerto = true;

        // Detenemos el NavMeshAgent.
        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        // Desactivamos este script.
        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujamos la distancia de visión.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaVision);

        // Dibujamos la distancia de acecho.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAcecho);

        // Dibujamos la distancia de ataque.
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }

}
