using UnityEngine;
using UnityEngine.AI;

public class EnemyVisionAI : MonoBehaviour
{
    public enum Estado
    {
        Patrulla,
        Persecucion,
        Busqueda,
        Ataque
    }

    [Header("Referencias")]
    public Transform[] puntosPatrulla;
    public Transform jugador;
    public LayerMask capasVision = ~0;

    [Header("Vision")]
    public float distanciaVision = 12f;
    public float anguloVision = 70f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2.5f;
    public float velocidadPersecucion = 4.2f;
    public float tiempoEsperaPatrulla = 1f;
    public float tiempoBusqueda = 4f;

    [Header("Ataque")]
    public float distanciaAtaque = 2.2f;
    public float danio = 10f;
    public float enfriamientoAtaque = 1f;

    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private PlayerHealth playerHealth;

    private Estado estadoActual = Estado.Patrulla;
    private int indicePatrulla = 0;
    private float temporizadorEspera = 0f;
    private float temporizadorBusqueda = 0f;
    private float ultimoAtaque = -999f;
    private Vector3 ultimaPosicionVista;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (jugador == null)
        {
            GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
            if (objJugador != null)
                jugador = objJugador.transform;
        }

        if (jugador != null)
        {
            playerHealth = jugador.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = jugador.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = jugador.GetComponentInChildren<PlayerHealth>();
        }

        if (agent == null)
            Debug.LogError("Falta NavMeshAgent en " + gameObject.name);

        if (jugador == null)
            Debug.LogError("No se encontró el jugador en " + gameObject.name);

        if (playerHealth == null)
            Debug.LogError("No se encontró PlayerHealth en " + gameObject.name);

        CambiarEstado(Estado.Patrulla);
    }

    void Update()
    {
        if (enemyHealth != null && (enemyHealth.estaMuerto || enemyHealth.estaAturdido))
            return;

        if (jugador == null || agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (PuedeVerAlJugador())
        {
            ultimaPosicionVista = jugador.position;

            float distancia = DistanciaHorizontalAlJugador();

            if (distancia <= distanciaAtaque)
                CambiarEstado(Estado.Ataque);
            else
                CambiarEstado(Estado.Persecucion);
        }

        switch (estadoActual)
        {
            case Estado.Patrulla:
                ActualizarPatrulla();
                break;

            case Estado.Persecucion:
                ActualizarPersecucion();
                break;

            case Estado.Busqueda:
                ActualizarBusqueda();
                break;

            case Estado.Ataque:
                ActualizarAtaque();
                break;
        }
    }

    float DistanciaHorizontalAlJugador()
    {
        Vector3 a = transform.position;
        Vector3 b = jugador.position;

        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    void CambiarEstado(Estado nuevoEstado)
    {
        if (estadoActual == nuevoEstado)
            return;

        estadoActual = nuevoEstado;
        Debug.Log(gameObject.name + " cambió a estado: " + estadoActual);

        switch (estadoActual)
        {
            case Estado.Patrulla:
                agent.speed = velocidadPatrulla;
                agent.stoppingDistance = 0f;
                IrAlSiguientePunto();
                break;

            case Estado.Persecucion:
                agent.speed = velocidadPersecucion;
                agent.stoppingDistance = distanciaAtaque * 0.8f;
                break;

            case Estado.Busqueda:
                agent.speed = velocidadPatrulla;
                agent.stoppingDistance = 0f;
                temporizadorBusqueda = tiempoBusqueda;
                agent.SetDestination(ultimaPosicionVista);
                break;

            case Estado.Ataque:
                agent.ResetPath();
                break;
        }
    }

    void ActualizarPatrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            temporizadorEspera += Time.deltaTime;

            if (temporizadorEspera >= tiempoEsperaPatrulla)
            {
                temporizadorEspera = 0f;
                IrAlSiguientePunto();
            }
        }
    }

    void IrAlSiguientePunto()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            return;

        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
    }

    void ActualizarPersecucion()
    {
        agent.SetDestination(jugador.position);

        float distancia = DistanciaHorizontalAlJugador();

        if (!PuedeVerAlJugador())
        {
            CambiarEstado(Estado.Busqueda);
            return;
        }

        if (distancia <= distanciaAtaque)
        {
            CambiarEstado(Estado.Ataque);
        }
    }

    void ActualizarBusqueda()
    {
        if (PuedeVerAlJugador())
        {
            CambiarEstado(Estado.Persecucion);
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            if (temporizadorBusqueda <= 0f)
            {
                CambiarEstado(Estado.Patrulla);
            }
        }
    }

    void ActualizarAtaque()
    {
        if (jugador == null)
            return;

        Vector3 objetivoMirada = jugador.position;
        objetivoMirada.y = transform.position.y;
        transform.LookAt(objetivoMirada);

        float distancia = DistanciaHorizontalAlJugador();

        if (distancia > distanciaAtaque + 0.4f)
        {
            CambiarEstado(Estado.Persecucion);
            return;
        }

        if (Time.time >= ultimoAtaque + enfriamientoAtaque)
        {
            ultimoAtaque = Time.time;

            if (playerHealth != null)
            {
                Debug.Log(gameObject.name + " golpeó al jugador.");
                playerHealth.RecibirDanio(danio);
            }
            else
            {
                Debug.LogWarning(gameObject.name + " no encontró PlayerHealth para hacer daño.");
            }
        }
    }

    bool PuedeVerAlJugador()
    {
        Vector3 origen = transform.position + Vector3.up * 1.6f;
        Vector3 destino = jugador.position + Vector3.up * 1f;
        Vector3 direccion = destino - origen;
        float distancia = direccion.magnitude;

        if (distancia > distanciaVision)
            return false;

        direccion.Normalize();

        float angulo = Vector3.Angle(transform.forward, direccion);
        if (angulo > anguloVision * 0.5f)
            return false;

        Debug.DrawRay(origen, direccion * distancia, Color.red);

        if (Physics.Raycast(origen, direccion, out RaycastHit hit, distancia, capasVision))
        {
            if (hit.transform == jugador || hit.transform.root == jugador)
                return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaVision);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        Vector3 izquierda = Quaternion.Euler(0, -anguloVision * 0.5f, 0) * transform.forward;
        Vector3 derecha = Quaternion.Euler(0, anguloVision * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + izquierda * distanciaVision);
        Gizmos.DrawLine(transform.position, transform.position + derecha * distanciaVision);
    }
}