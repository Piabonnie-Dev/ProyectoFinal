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

    [Header("Puntos de vision")]

    // Punto desde donde salen los rayos de visión del enemigo.
    // Si lo dejas vacío, el script usará la altura de los ojos.
    public Transform puntoOjos;

    // Punto del jugador al que el enemigo intenta mirar.
    // Si lo dejas vacío, usará la posición del jugador con una altura aproximada.
    public Transform puntoObjetivoJugador;

    [Header("Capas de detección")]

    // Aquí van las paredes, columnas, puertas, cajas grandes, etc.
    // Todo lo que bloquee la vista del enemigo.
    public LayerMask capasQueBloqueanVision;

    [Header("Vision")]
    public float distanciaVision = 12f;
    public float anguloVision = 70f;
    public float alturaOjos = 1.6f;
    public float alturaObjetivoJugador = 1.0f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2.5f;
    public float velocidadPersecucion = 4.2f;
    public float tiempoEsperaPatrulla = 1f;
    public float tiempoBusqueda = 4f;

    [Header("Ajustes anti-trabe")]

    // Cada cuánto actualiza el destino cuando persigue.
    // Evita estar llamando SetDestination en cada frame.
    public float intervaloActualizarDestino = 0.18f;

    // Qué tan rápido gira el enemigo hacia su objetivo.
    public float velocidadRotacion = 420f;

    // Distancia máxima para buscar una posición válida sobre el NavMesh.
    public float distanciaBusquedaNavMesh = 2f;

    [Header("Ataque")]
    public float distanciaAtaque = 2.2f;
    public float danio = 10f;
    public float enfriamientoAtaque = 1f;

    [Header("Debug")]
    public bool mostrarDebugVision = true;

    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private PlayerHealth playerHealth;

    private Estado estadoActual = Estado.Patrulla;
    private int indicePatrulla = 0;
    private float temporizadorEspera = 0f;
    private float temporizadorBusqueda = 0f;
    private float ultimoAtaque = -999f;
    private float proximaActualizacionDestino = 0f;

    private Vector3 ultimaPosicionVista;

    void Start()
    {
        // Tomamos el NavMeshAgent del enemigo.
        agent = GetComponent<NavMeshAgent>();

        // Tomamos la vida del enemigo.
        enemyHealth = GetComponent<EnemyHealth>();

        // Si no asignaste jugador manualmente, lo buscamos por tag.
        if (jugador == null)
        {
            GameObject objJugador = GameObject.FindGameObjectWithTag("Player");

            if (objJugador != null)
                jugador = objJugador.transform;
        }

        // Buscamos la vida del jugador.
        if (jugador != null)
        {
            playerHealth = jugador.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = jugador.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = jugador.GetComponentInChildren<PlayerHealth>();
        }

        // Validaciones para encontrar errores rápido.
        if (agent == null)
            Debug.LogError("Falta NavMeshAgent en " + gameObject.name);

        if (jugador == null)
            Debug.LogError("No se encontró el jugador en " + gameObject.name);

        if (playerHealth == null)
            Debug.LogWarning("No se encontró PlayerHealth en " + gameObject.name);

        // Dejamos que el script controle la rotación.
        // Esto evita peleas entre NavMeshAgent y transform.LookAt.
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.isStopped = false;
        }

        // Iniciamos en patrulla.
        CambiarEstado(Estado.Patrulla);
    }

    void Update()
    {
        // Si está muerto o aturdido, no hace nada.
        if (enemyHealth != null && (enemyHealth.estaMuerto || enemyHealth.estaAturdido))
        {
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;

            return;
        }

        // Si faltan referencias importantes, salimos.
        if (jugador == null || agent == null)
            return;

        // Si el agente no está sobre NavMesh, no puede moverse.
        if (!agent.isOnNavMesh)
            return;

        // Revisamos si puede ver al jugador.
        bool veJugador = PuedeVerAlJugador();

        // Si lo ve, guardamos su última posición.
        if (veJugador)
        {
            ultimaPosicionVista = jugador.position;

            float distancia = DistanciaHorizontalAlJugador();

            if (distancia <= distanciaAtaque)
            {
                CambiarEstado(Estado.Ataque);
            }
            else
            {
                CambiarEstado(Estado.Persecucion);
            }
        }

        // Ejecutamos comportamiento según estado.
        switch (estadoActual)
        {
            case Estado.Patrulla:
                ActualizarPatrulla();
                break;

            case Estado.Persecucion:
                ActualizarPersecucion(veJugador);
                break;

            case Estado.Busqueda:
                ActualizarBusqueda(veJugador);
                break;

            case Estado.Ataque:
                ActualizarAtaque(veJugador);
                break;
        }

        // Rotamos suavemente según el estado.
        ActualizarRotacion(veJugador);
    }

    float DistanciaHorizontalAlJugador()
    {
        // Calculamos distancia ignorando altura.
        Vector3 a = transform.position;
        Vector3 b = jugador.position;

        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    void CambiarEstado(Estado nuevoEstado)
    {
        // Si ya estamos en ese estado, no repetimos configuración.
        if (estadoActual == nuevoEstado)
            return;

        estadoActual = nuevoEstado;

        Debug.Log(gameObject.name + " cambió a estado: " + estadoActual);

        switch (estadoActual)
        {
            case Estado.Patrulla:
                agent.isStopped = false;
                agent.speed = velocidadPatrulla;
                agent.stoppingDistance = 0f;
                IrAlSiguientePunto();
                break;

            case Estado.Persecucion:
                agent.isStopped = false;
                agent.speed = velocidadPersecucion;
                agent.stoppingDistance = distanciaAtaque * 0.8f;
                proximaActualizacionDestino = 0f;
                break;

            case Estado.Busqueda:
                agent.isStopped = false;
                agent.speed = velocidadPatrulla;
                agent.stoppingDistance = 0f;
                temporizadorBusqueda = tiempoBusqueda;
                MoverHaciaPuntoValido(ultimaPosicionVista);
                break;

            case Estado.Ataque:
                agent.isStopped = true;
                agent.ResetPath();
                break;
        }
    }

    void ActualizarPatrulla()
    {
        // Si no hay puntos de patrulla, no patrulla.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            return;

        // Si llegó al punto actual, espera y luego va al siguiente.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.25f)
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
        // Validamos que existan puntos.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            return;

        // Validamos que el punto actual exista.
        if (puntosPatrulla[indicePatrulla] == null)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            return;
        }

        // Movemos al enemigo hacia un punto válido del NavMesh.
        MoverHaciaPuntoValido(puntosPatrulla[indicePatrulla].position);

        // Avanzamos al siguiente punto.
        indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
    }

    void ActualizarPersecucion(bool veJugador)
    {
        // Si ya no lo ve, va a buscar a la última posición donde lo vio.
        if (!veJugador)
        {
            CambiarEstado(Estado.Busqueda);
            return;
        }

        float distancia = DistanciaHorizontalAlJugador();

        // Si está en rango, pasa a ataque.
        if (distancia <= distanciaAtaque)
        {
            CambiarEstado(Estado.Ataque);
            return;
        }

        // Actualizamos destino cada cierto tiempo para evitar trabes.
        if (Time.time >= proximaActualizacionDestino)
        {
            proximaActualizacionDestino = Time.time + intervaloActualizarDestino;
            MoverHaciaPuntoValido(jugador.position);
        }
    }

    void ActualizarBusqueda(bool veJugador)
    {
        // Si lo vuelve a ver, lo persigue.
        if (veJugador)
        {
            CambiarEstado(Estado.Persecucion);
            return;
        }

        // Reduce el tiempo de búsqueda.
        temporizadorBusqueda -= Time.deltaTime;

        // Si llegó a la última posición vista y se acabó el tiempo, vuelve a patrullar.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.25f)
        {
            if (temporizadorBusqueda <= 0f)
            {
                CambiarEstado(Estado.Patrulla);
            }
        }
    }

    void ActualizarAtaque(bool veJugador)
    {
        float distancia = DistanciaHorizontalAlJugador();

        // Si ya no lo ve, deja de atacar y busca.
        if (!veJugador)
        {
            CambiarEstado(Estado.Busqueda);
            return;
        }

        // Si el jugador se alejó, vuelve a perseguir.
        if (distancia > distanciaAtaque + 0.4f)
        {
            CambiarEstado(Estado.Persecucion);
            return;
        }

        // Ataca usando enfriamiento.
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
        // Calculamos origen del rayo.
        Vector3 origen = ObtenerPosicionOjos();

        // Calculamos objetivo del rayo.
        Vector3 destino = ObtenerPosicionObjetivoJugador();

        // Calculamos dirección.
        Vector3 direccion = destino - origen;

        // Calculamos distancia real.
        float distancia = direccion.magnitude;

        // Si está fuera de rango, no lo ve.
        if (distancia > distanciaVision)
            return false;

        // Normalizamos dirección.
        direccion.Normalize();

        // Revisamos el ángulo de visión.
        float angulo = Vector3.Angle(transform.forward, direccion);

        if (angulo > anguloVision * 0.5f)
            return false;

        // Raycast contra paredes/obstáculos.
        // Si algo bloquea antes de llegar al jugador, NO lo puede ver.
        if (Physics.Raycast(origen, direccion, out RaycastHit hitBloqueo, distancia, capasQueBloqueanVision, QueryTriggerInteraction.Ignore))
        {
            if (mostrarDebugVision)
                Debug.DrawLine(origen, hitBloqueo.point, Color.blue);

            return false;
        }

        // Si no hay pared entre ambos, sí lo ve.
        if (mostrarDebugVision)
            Debug.DrawLine(origen, destino, Color.red);

        return true;
    }

    Vector3 ObtenerPosicionOjos()
    {
        // Si asignaste un punto de ojos, usa ese punto.
        if (puntoOjos != null)
            return puntoOjos.position;

        // Si no, usa la posición del enemigo con altura.
        return transform.position + Vector3.up * alturaOjos;
    }

    Vector3 ObtenerPosicionObjetivoJugador()
    {
        // Si asignaste un punto objetivo en el jugador, usa ese punto.
        if (puntoObjetivoJugador != null)
            return puntoObjetivoJugador.position;

        // Si no, usa la posición del jugador con altura.
        return jugador.position + Vector3.up * alturaObjetivoJugador;
    }

    void MoverHaciaPuntoValido(Vector3 destino)
    {
        // Si el agente no existe o no está en NavMesh, no movemos.
        if (agent == null || !agent.isOnNavMesh)
            return;

        // Buscamos el punto más cercano en NavMesh.
        if (NavMesh.SamplePosition(destino, out NavMeshHit hit, distanciaBusquedaNavMesh, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " no encontró punto válido en NavMesh cerca de: " + destino);
        }
    }

    void ActualizarRotacion(bool veJugador)
    {
        // Si está atacando o ve al jugador, mira hacia el jugador.
        if (veJugador || estadoActual == Estado.Ataque)
        {
            RotarHacia(jugador.position);
            return;
        }

        // Si está moviéndose, mira hacia la dirección de movimiento.
        if (agent != null && agent.velocity.sqrMagnitude > 0.05f)
        {
            Vector3 direccionMovimiento = transform.position + agent.velocity;
            RotarHacia(direccionMovimiento);
        }
    }

    void RotarHacia(Vector3 objetivo)
    {
        // Ignoramos altura para que no incline el cuerpo hacia arriba o abajo.
        objetivo.y = transform.position.y;

        // Calculamos dirección.
        Vector3 direccion = objetivo - transform.position;

        // Si la dirección es demasiado pequeña, no rotamos.
        if (direccion.sqrMagnitude < 0.001f)
            return;

        // Calculamos rotación objetivo.
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion.normalized);

        // Rotamos suavemente.
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        // Dibujamos radio de visión.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaVision);

        // Dibujamos radio de ataque.
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        // Dibujamos cono de visión básico.
        Vector3 origen = Application.isPlaying ? ObtenerPosicionOjos() : transform.position + Vector3.up * alturaOjos;

        Vector3 izquierda = Quaternion.Euler(0, -anguloVision * 0.5f, 0) * transform.forward;
        Vector3 derecha = Quaternion.Euler(0, anguloVision * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origen, origen + izquierda * distanciaVision);
        Gizmos.DrawLine(origen, origen + derecha * distanciaVision);

        // Línea a la última posición vista.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ultimaPosicionVista, 0.3f);
    }
}