using UnityEngine;
using UnityEngine.AI;

public class EnemyV37AI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public NavMeshAgent agente;
    public EnemyHealth vida;

    [Header("Puntos de patrulla")]
    public Transform[] puntosPatrulla;

    [Header("Capas")]
    public LayerMask capaObstaculos;

    [Header("Detección")]
    public float distanciaVision = 28f;
    public float anguloVision = 120f;
    public float distanciaAcecho = 20f;
    public float tiempoAntesDeEmboscada = 1.2f;
    public float tiempoMemoriaJugador = 8f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadAcecho = 4f;
    public float velocidadEmboscada = 10f;
    public float aceleracionPersecucion = 35f;

    [Header("Rotación")]
    public float velocidadRotacion = 14f;

    [Header("Ataque")]
    public float distanciaAtaque = 2.5f;
    public float dañoAtaque = 35f;
    public float tiempoEntreAtaques = 1f;

    [Header("Búsqueda")]
    public float tiempoBuscandoUltimaPosicion = 3f;

    [Header("Estado actual")]
    public EstadoV37 estadoActual = EstadoV37.Patrulla;

    private int indicePatrulla = 0;
    private float contadorAcecho = 0f;
    private float contadorAtaque = 0f;
    private float contadorMemoria = 0f;
    private float contadorBusqueda = 0f;
    private float contadorActualizarDestino = 0f;

    private Vector3 ultimaPosicionConocidaJugador;

    private bool estaStuneado = false;
    private float contadorStun = 0f;
    private bool estaMuerto = false;

    public enum EstadoV37
    {
        Patrulla,
        Acecho,
        Persecucion,
        BuscarUltimaPosicion,
        Muerto
    }

    private void Start()
    {
        // Buscamos el NavMeshAgent si no fue asignado.
        if (agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
        }

        // Buscamos la vida del enemigo.
        if (vida == null)
        {
            vida = GetComponent<EnemyHealth>();
        }

        // Buscamos al jugador por tag si no fue asignado.
        if (jugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if (objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
        }

        // Configuración fuerte para que V37 no se sienta torpe.
        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
            agente.baseOffset = 0f;
            agente.stoppingDistance = distanciaAtaque * 0.8f;
            agente.acceleration = aceleracionPersecucion;
            agente.angularSpeed = 720f;
            agente.speed = velocidadPatrulla;
        }

        // Guardamos una posición inicial.
        if (jugador != null)
        {
            ultimaPosicionConocidaJugador = jugador.position;
        }

        // Inicia patrullando.
        IrAlSiguientePuntoPatrulla();
    }

    private void Update()
    {
        // Si EnemyHealth ya marcó muerte, detenemos la IA.
        if (vida != null && vida.estaMuerto)
        {
            DetenerIA();
            return;
        }

        // Si está muerto, no hace nada.
        if (estaMuerto)
        {
            return;
        }

        // Si no hay jugador o agente, no hacemos nada.
        if (jugador == null || agente == null || !agente.enabled)
        {
            return;
        }

        // Forzamos que el cuerpo no se incline en X/Z.
        MantenerRotacionRecta();

        // Si está stuneado, se queda detenido.
        if (estaStuneado)
        {
            ProcesarStun();
            return;
        }

        // Bajamos el cooldown de ataque.
        if (contadorAtaque > 0f)
        {
            contadorAtaque -= Time.deltaTime;
        }

        // Detectamos al jugador.
        bool jugadorVisible = PuedeVerJugador();
        bool jugadorCerca = Vector3.Distance(transform.position, jugador.position) <= distanciaAcecho;

        // Si lo ve o lo siente cerca, actualiza memoria.
        if (jugadorVisible || jugadorCerca)
        {
            ultimaPosicionConocidaJugador = jugador.position;
            contadorMemoria = tiempoMemoriaJugador;
        }
        else
        {
            contadorMemoria -= Time.deltaTime;
        }

        // Ejecutamos el estado actual.
        switch (estadoActual)
        {
            case EstadoV37.Patrulla:
                EstadoPatrulla(jugadorVisible, jugadorCerca);
                break;

            case EstadoV37.Acecho:
                EstadoAcecho(jugadorVisible, jugadorCerca);
                break;

            case EstadoV37.Persecucion:
                EstadoPersecucion(jugadorVisible, jugadorCerca);
                break;

            case EstadoV37.BuscarUltimaPosicion:
                EstadoBuscarUltimaPosicion(jugadorVisible, jugadorCerca);
                break;
        }
    }

    private void EstadoPatrulla(bool jugadorVisible, bool jugadorCerca)
    {
        // Velocidad de patrulla.
        agente.speed = velocidadPatrulla;
        agente.isStopped = false;

        // Rota hacia donde camina.
        RotarHaciaMovimiento();

        // Si llegó a un punto, va al siguiente.
        if (!agente.pathPending && agente.remainingDistance <= 0.6f)
        {
            IrAlSiguientePuntoPatrulla();
        }

        // Si detecta al jugador, pasa a acecho.
        if (jugadorVisible || jugadorCerca)
        {
            CambiarEstado(EstadoV37.Acecho);
        }
    }

    private void EstadoAcecho(bool jugadorVisible, bool jugadorCerca)
    {
        // En acecho se acerca, pero ya no va lentísimo.
        agente.speed = velocidadAcecho;
        agente.isStopped = false;

        // Actualizamos destino cada poco tiempo para que no se sienta arrastrado.
        ActualizarDestinoConIntervalo(jugador.position, 0.12f);

        // Mira al jugador sin inclinarse.
        RotarSoloEnYHacia(jugador.position);

        // Cuenta el tiempo de acecho.
        contadorAcecho += Time.deltaTime;

        // Si el jugador está muy cerca, no espera: entra en persecución.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque * 3f)
        {
            CambiarEstado(EstadoV37.Persecucion);
            return;
        }

        // Después de un tiempo corto, entra en persecución fuerte.
        if (contadorAcecho >= tiempoAntesDeEmboscada)
        {
            CambiarEstado(EstadoV37.Persecucion);
            return;
        }

        // Si perdió completamente al jugador, va a la última posición.
        if (!jugadorVisible && !jugadorCerca && contadorMemoria <= 0f)
        {
            CambiarEstado(EstadoV37.BuscarUltimaPosicion);
        }
    }

    private void EstadoPersecucion(bool jugadorVisible, bool jugadorCerca)
    {
        // En persecución V37 debe sentirse peligroso.
        agente.speed = velocidadEmboscada;
        agente.acceleration = aceleracionPersecucion;
        agente.isStopped = false;

        // Si tiene memoria del jugador, sigue persiguiendo.
        if (contadorMemoria > 0f)
        {
            ActualizarDestinoConIntervalo(ultimaPosicionConocidaJugador, 0.08f);
        }

        // Rota hacia el movimiento o hacia el jugador.
        if (agente.velocity.sqrMagnitude > 0.2f)
        {
            RotarHaciaMovimiento();
        }
        else
        {
            RotarSoloEnYHacia(jugador.position);
        }

        // Ataca solo si realmente está cerca.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque)
        {
            RotarSoloEnYHacia(jugador.position);
            AtacarJugador();
        }

        // Si ya no tiene memoria del jugador, busca la última posición.
        if (!jugadorVisible && !jugadorCerca && contadorMemoria <= 0f)
        {
            CambiarEstado(EstadoV37.BuscarUltimaPosicion);
        }
    }

    private void EstadoBuscarUltimaPosicion(bool jugadorVisible, bool jugadorCerca)
    {
        // Si vuelve a detectar al jugador, regresa a persecución.
        if (jugadorVisible || jugadorCerca)
        {
            CambiarEstado(EstadoV37.Persecucion);
            return;
        }

        // Va a la última posición donde detectó al jugador.
        agente.speed = velocidadAcecho;
        agente.isStopped = false;
        ActualizarDestinoConIntervalo(ultimaPosicionConocidaJugador, 0.2f);

        // Rota hacia el movimiento.
        RotarHaciaMovimiento();

        // Si llegó a la última posición, empieza a buscar.
        if (!agente.pathPending && agente.remainingDistance <= 1.2f)
        {
            contadorBusqueda += Time.deltaTime;
        }

        // Si buscó un rato y no encontró nada, vuelve a patrulla.
        if (contadorBusqueda >= tiempoBuscandoUltimaPosicion)
        {
            CambiarEstado(EstadoV37.Patrulla);
        }
    }

    private void ActualizarDestinoConIntervalo(Vector3 destino, float intervalo)
    {
        // Evitamos llamar SetDestination cada frame sin necesidad.
        contadorActualizarDestino -= Time.deltaTime;

        if (contadorActualizarDestino <= 0f)
        {
            contadorActualizarDestino = intervalo;

            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.SetDestination(destino);
            }
        }
    }

    private bool PuedeVerJugador()
    {
        // Si no hay jugador, no puede verlo.
        if (jugador == null)
        {
            return false;
        }

        // Dirección hacia el jugador.
        Vector3 direccionAlJugador = jugador.position - transform.position;

        // Revisamos distancia.
        if (direccionAlJugador.magnitude > distanciaVision)
        {
            return false;
        }

        // Revisamos ángulo de visión.
        float angulo = Vector3.Angle(transform.forward, direccionAlJugador);

        if (angulo > anguloVision * 0.5f)
        {
            return false;
        }

        // Raycast desde el cuerpo de V37.
        Vector3 origenRayo = transform.position + Vector3.up * 1.5f;
        Vector3 direccionRayo = direccionAlJugador.normalized;

        // Si una pared o estructura bloquea la visión, no ve al jugador.
        if (Physics.Raycast(origenRayo, direccionRayo, direccionAlJugador.magnitude, capaObstaculos))
        {
            return false;
        }

        return true;
    }

    private void AtacarJugador()
    {
        // Evitamos ataques repetidos demasiado rápido.
        if (contadorAtaque > 0f)
        {
            return;
        }

        // Reiniciamos cooldown.
        contadorAtaque = tiempoEntreAtaques;

        // Mandamos daño al jugador.
        if (jugador != null)
        {
            jugador.SendMessage("RecibirDaño", dañoAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("RecibirDanio", dañoAtaque, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("TakeDamage", dañoAtaque, SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("V37 atacó al jugador.");
    }

    private void IrAlSiguientePuntoPatrulla()
    {
        // Si no hay puntos de patrulla, no hacemos nada.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            return;
        }

        // Si el agente no está listo, no hacemos nada.
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        // Mandamos al agente al punto actual.
        agente.SetDestination(puntosPatrulla[indicePatrulla].position);

        // Avanzamos al siguiente.
        indicePatrulla++;

        // Si llega al final, vuelve al primero.
        if (indicePatrulla >= puntosPatrulla.Length)
        {
            indicePatrulla = 0;
        }
    }

    private void CambiarEstado(EstadoV37 nuevoEstado)
    {
        // Si está muerto, no puede cambiar de estado.
        if (estaMuerto)
        {
            return;
        }

        // Cambiamos estado.
        estadoActual = nuevoEstado;

        // Reiniciamos contadores dependiendo del estado.
        if (nuevoEstado == EstadoV37.Acecho)
        {
            contadorAcecho = 0f;
        }

        if (nuevoEstado == EstadoV37.BuscarUltimaPosicion)
        {
            contadorBusqueda = 0f;
        }

        // Si vuelve a patrullar, retomamos puntos.
        if (nuevoEstado == EstadoV37.Patrulla)
        {
            IrAlSiguientePuntoPatrulla();
        }

        Debug.Log("V37 cambió a estado: " + nuevoEstado);
    }

    private void RotarSoloEnYHacia(Vector3 objetivo)
    {
        // Calculamos dirección.
        Vector3 direccion = objetivo - transform.position;

        // Eliminamos altura para evitar inclinación.
        direccion.y = 0f;

        // Si la dirección es muy pequeña, no rotamos.
        if (direccion.sqrMagnitude < 0.01f)
        {
            return;
        }

        // Calculamos rotación horizontal.
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        // Rotamos suavemente.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    private void RotarHaciaMovimiento()
    {
        // Usamos la velocidad del agente.
        Vector3 direccion = agente.velocity;

        // Eliminamos altura.
        direccion.y = 0f;

        // Si casi no se mueve, no rotamos.
        if (direccion.sqrMagnitude < 0.05f)
        {
            return;
        }

        // Calculamos rotación.
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        // Rotamos suavemente.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    private void MantenerRotacionRecta()
    {
        // Conservamos la Y.
        float rotacionY = transform.eulerAngles.y;

        // Forzamos X y Z a cero para que no se incline.
        transform.rotation = Quaternion.Euler(0f, rotacionY, 0f);
    }

    public void Stun(float duracion)
    {
        // Si murió, no se puede aturdir.
        if (estaMuerto)
        {
            return;
        }

        // Activamos stun.
        estaStuneado = true;

        // Guardamos duración.
        contadorStun = duracion;

        // Detenemos agente.
        if (agente != null && agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
    }

    private void ProcesarStun()
    {
        // Bajamos el tiempo.
        contadorStun -= Time.deltaTime;

        // Si terminó, vuelve a perseguir si recuerda al jugador.
        if (contadorStun <= 0f)
        {
            estaStuneado = false;

            if (agente != null && agente.enabled)
            {
                agente.isStopped = false;
            }

            if (contadorMemoria > 0f)
            {
                CambiarEstado(EstadoV37.Persecucion);
            }
            else
            {
                CambiarEstado(EstadoV37.BuscarUltimaPosicion);
            }
        }
    }

    public void DetenerIA()
    {
        // Evitamos detener dos veces.
        if (estaMuerto)
        {
            return;
        }

        // Marcamos muerto.
        estaMuerto = true;
        estadoActual = EstadoV37.Muerto;

        // Detenemos NavMeshAgent.
        if (agente != null && agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.enabled = false;
        }

        // Dejamos el cuerpo recto.
        MantenerRotacionRecta();

        // Apagamos este script.
        enabled = false;

        Debug.Log("IA de V37 detenida por muerte.");
    }

    public void Morir()
    {
        // Compatibilidad por si otro script llama Morir directamente.
        DetenerIA();
    }

    private void OnDrawGizmosSelected()
    {
        // Visión.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaVision);

        // Acecho.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAcecho);

        // Ataque real.
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}