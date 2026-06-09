using UnityEngine;
using UnityEngine.AI;

public class EnemyV19AI : MonoBehaviour
{
    [Header("Referencias principales")]
    public Transform jugador;
    public Camera camaraJugador;
    public NavMeshAgent agente;
    public EnemyHealth vida;

    [Header("Puntos de patrulla")]
    public Transform[] puntosPatrulla;

    [Header("Capas que bloquean la mirada")]
    public LayerMask capaObstaculos;

    [Header("Detección por mirada")]
    public float distanciaMaximaParaDetenerlo = 35f;
    public float anguloMiradaJugador = 60f;
    public float alturaPuntoMirada = 1.3f;

    [Header("Activación")]
    public float distanciaActivacion = 40f;
    public float tiempoMemoriaJugador = 4f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 8f;
    public float aceleracion = 35f;

    [Header("Rotación")]
    public float velocidadRotacion = 14f;

    [Header("Ataque mortal")]
    public float distanciaAtaque = 1.6f;
    public float danioMortal = 9999f;
    public float tiempoEntreAtaques = 0.7f;

    [Header("Daño por objetos")]
    public float fuerzaMinimaObjeto = 9f;

    [Header("Debug de mirada")]
    public bool elJugadorLoEstaMirando = false;
    public bool debugEstaEnPantalla = false;
    public bool debugHayObstaculo = false;
    public float debugAngulo = 0f;
    public float debugDistancia = 0f;
    public float distanciaActualAlJugador = 0f;

    [Header("Estado actual")]
    public EstadoV19 estadoActual = EstadoV19.Patrulla;

    private int indicePatrulla = 0;
    private float contadorAtaque = 0f;
    private float contadorMemoria = 0f;
    private bool estaMuerto = false;

    public enum EstadoV19
    {
        Patrulla,
        CongeladoPorMirada,
        Persecucion,
        Muerto
    }

    private void Start()
    {
        // Buscamos el NavMeshAgent si no fue asignado.
        if (agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
        }

        // Buscamos EnemyHealth para usar su barra de vida.
        if (vida == null)
        {
            vida = GetComponent<EnemyHealth>();
        }

        // Buscamos al jugador si no fue asignado manualmente.
        if (jugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if (objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
        }

        // Buscamos la cámara principal solo como respaldo.
        // Lo ideal es asignar manualmente:
        // Player > Pivote de la camara > Main Camera.
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Configuramos el agente para que no rote solo.
        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
            agente.baseOffset = 0f;
            agente.speed = velocidadPatrulla;
            agente.acceleration = aceleracion;
            agente.angularSpeed = 720f;
            agente.stoppingDistance = distanciaAtaque * 0.8f;
        }

        // Inicia patrullando.
        IrAlSiguientePuntoPatrulla();
    }

    private void Update()
    {
        // Si la partida terminó, V19 no hace nada.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // Si EnemyHealth dice que ya murió, detenemos la IA.
        if (vida != null && vida.estaMuerto)
        {
            DetenerIA();
            return;
        }

        // Si ya está muerto, no hace nada.
        if (estaMuerto)
        {
            return;
        }

        // Si faltan referencias, no hacemos nada.
        if (jugador == null || camaraJugador == null || agente == null || !agente.enabled)
        {
            return;
        }

        // Evitamos que el enemigo se incline.
        MantenerRotacionRecta();

        // Bajamos cooldown de ataque.
        if (contadorAtaque > 0f)
        {
            contadorAtaque -= Time.deltaTime;
        }

        // Calculamos si el jugador está mirando a V19.
        elJugadorLoEstaMirando = JugadorEstaMirandoAV19();

        // ESTA ES LA PARTE MÁS IMPORTANTE:
        // Si el jugador lo está mirando, V19 se congela y salimos del Update.
        // Así evitamos que después se ejecute persecución o patrulla.
        if (elJugadorLoEstaMirando)
        {
            CongelarPorMirada();
            return;
        }

        // Si llegó aquí, significa que el jugador NO lo está mirando.
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);

        // Si está en rango, persigue.
        if (distanciaAlJugador <= distanciaActivacion)
        {
            contadorMemoria = tiempoMemoriaJugador;
            CambiarEstado(EstadoV19.Persecucion);
        }
        else
        {
            contadorMemoria -= Time.deltaTime;

            if (contadorMemoria <= 0f)
            {
                CambiarEstado(EstadoV19.Patrulla);
            }
        }

        // Ejecutamos el estado actual.
        switch (estadoActual)
        {
            case EstadoV19.Patrulla:
                EstadoPatrulla();
                break;

            case EstadoV19.Persecucion:
                EstadoPersecucion();
                break;
        }
    }

    private void EstadoPatrulla()
    {
        // Si el agente no está listo, salimos.
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        // Patrulla lenta.
        agente.isStopped = false;
        agente.speed = velocidadPatrulla;
        agente.acceleration = aceleracion;

        // Rota hacia donde se mueve.
        RotarHaciaMovimiento();

        // Si llegó al punto, va al siguiente.
        if (!agente.pathPending && agente.remainingDistance <= 0.6f)
        {
            IrAlSiguientePuntoPatrulla();
        }
    }

    private void EstadoPersecucion()
{
    // Si no hay agente activo, salimos.
    if (agente == null || !agente.enabled || !agente.isOnNavMesh)
    {
        return;
    }

    // Primero revisamos si V19 ya está suficientemente cerca para atacar.
    // Esto evita que se quede empujando al jugador sin hacer nada.
    if (JugadorEnRangoDeAtaque())
    {
        DetenerMovimientoParaAtacar();
        AtacarJugador();
        return;
    }

    // Si no está en rango de ataque, sigue persiguiendo.
    agente.isStopped = false;
    agente.speed = velocidadPersecucion;
    agente.acceleration = aceleracion;

    // Persigue directamente al jugador.
    agente.SetDestination(jugador.position);

    // Rota hacia su movimiento para no verse arrastrado.
    if (agente.velocity.sqrMagnitude > 0.1f)
    {
        RotarHaciaMovimiento();
    }
    else
    {
        RotarSoloEnYHacia(jugador.position);
    }

    // Baja la memoria del jugador.
    contadorMemoria -= Time.deltaTime;

    // Si pierde memoria y está lejos, vuelve a patrullar.
    if (contadorMemoria <= 0f && distanciaActualAlJugador > distanciaActivacion)
    {
        CambiarEstado(EstadoV19.Patrulla);
    }
}

    private void CongelarPorMirada()
    {
        // Cambiamos estado visual.
        estadoActual = EstadoV19.CongeladoPorMirada;

        // Detenemos al NavMeshAgent de forma fuerte.
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;

            // Sincronizamos la posición del agente con el transform.
            // Esto evita que el NavMeshAgent lo siga empujando.
            agente.nextPosition = transform.position;
        }

        // No rotamos hacia el jugador.
        // Esto hace que realmente se sienta congelado.
        MantenerRotacionRecta();
    }

    private bool JugadorEstaMirandoAV19()
    {
        // Reiniciamos debug.
        debugEstaEnPantalla = false;
        debugHayObstaculo = false;
        debugAngulo = 0f;
        debugDistancia = 0f;

        // Si no hay cámara, no puede detectar mirada.
        if (camaraJugador == null)
        {
            return false;
        }

        // Punto alto del cuerpo de V19.
        Vector3 puntoMiradaV19 = transform.position + Vector3.up * alturaPuntoMirada;

        // Convertimos a coordenadas de pantalla.
        Vector3 puntoEnPantalla = camaraJugador.WorldToViewportPoint(puntoMiradaV19);

        // Si está detrás de la cámara, no lo estás mirando.
        if (puntoEnPantalla.z <= 0f)
        {
            return false;
        }

        // Revisamos si está dentro de la pantalla.
        debugEstaEnPantalla =
            puntoEnPantalla.x >= 0f &&
            puntoEnPantalla.x <= 1f &&
            puntoEnPantalla.y >= 0f &&
            puntoEnPantalla.y <= 1f;

        if (!debugEstaEnPantalla)
        {
            return false;
        }

        // Distancia entre cámara y V19.
        debugDistancia = Vector3.Distance(camaraJugador.transform.position, puntoMiradaV19);

        if (debugDistancia > distanciaMaximaParaDetenerlo)
        {
            return false;
        }

        // Dirección hacia V19.
        Vector3 direccionHaciaV19 = puntoMiradaV19 - camaraJugador.transform.position;

        // Ángulo entre el centro de la cámara y V19.
        debugAngulo = Vector3.Angle(camaraJugador.transform.forward, direccionHaciaV19);

        if (debugAngulo > anguloMiradaJugador)
        {
            return false;
        }

        // Revisamos si hay una pared entre la cámara y V19.
        if (Physics.Raycast(
            camaraJugador.transform.position,
            direccionHaciaV19.normalized,
            out RaycastHit hit,
            debugDistancia,
            capaObstaculos))
        {
            // Si el raycast golpeó al propio V19 o a un hijo de V19, NO cuenta como obstáculo.
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                debugHayObstaculo = false;
                return true;
            }

            // Si golpeó otra cosa, sí hay obstáculo.
            debugHayObstaculo = true;
            return false;
        }

        // Si pasó pantalla, distancia, ángulo y no hay pared, sí lo estás mirando.
        return true;
    }
private bool JugadorEnRangoDeAtaque()
{
    // Si no hay jugador, no puede atacar.
    if (jugador == null)
    {
        return false;
    }

    // Calculamos distancia horizontal para evitar errores por altura.
    Vector3 posicionV19 = transform.position;
    Vector3 posicionJugador = jugador.position;

    // Ignoramos la diferencia de altura.
    posicionV19.y = 0f;
    posicionJugador.y = 0f;

    // Calculamos distancia real en el piso.
    float distanciaHorizontal = Vector3.Distance(posicionV19, posicionJugador);

    // Guardamos la distancia para verla en el Inspector.
    distanciaActualAlJugador = distanciaHorizontal;

    // Si está suficientemente cerca, puede atacar.
    return distanciaHorizontal <= distanciaAtaque;
}

private void DetenerMovimientoParaAtacar()
{
    // Detenemos el NavMeshAgent para que no se siga empujando contra el jugador.
    if (agente != null && agente.enabled && agente.isOnNavMesh)
    {
        agente.isStopped = true;
        agente.ResetPath();
        agente.velocity = Vector3.zero;

        // Sincronizamos la posición del agente con el transform.
        agente.nextPosition = transform.position;
    }

    // Lo hacemos mirar al jugador antes de atacar.
    RotarSoloEnYHacia(jugador.position);
}


    private void AtacarJugador()
{
    // Evitamos ataques repetidos en el mismo instante.
    if (contadorAtaque > 0f)
    {
        return;
    }

    // Reiniciamos cooldown de ataque.
    contadorAtaque = tiempoEntreAtaques;

    // Detenemos a V19 antes de aplicar daño.
    DetenerMovimientoParaAtacar();

    // Mandamos daño mortal al jugador.
    // BroadcastMessage ayuda si PlayerHealth está en un hijo del Player.
    if (jugador != null)
    {
        jugador.gameObject.SendMessage("RecibirDaño", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.SendMessage("RecibirDanio", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.SendMessage("RecibirDano", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.SendMessage("TakeDamage", danioMortal, SendMessageOptions.DontRequireReceiver);

        jugador.gameObject.BroadcastMessage("RecibirDaño", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.BroadcastMessage("RecibirDanio", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.BroadcastMessage("RecibirDano", danioMortal, SendMessageOptions.DontRequireReceiver);
        jugador.gameObject.BroadcastMessage("TakeDamage", danioMortal, SendMessageOptions.DontRequireReceiver);
    }

    // Por si tu vida del jugador está en un objeto separado del Canvas.
    GameObject managerVida = GameObject.Find("UI_PlayerHealth_Manager");

    if (managerVida != null)
    {
        managerVida.SendMessage("RecibirDaño", danioMortal, SendMessageOptions.DontRequireReceiver);
        managerVida.SendMessage("RecibirDanio", danioMortal, SendMessageOptions.DontRequireReceiver);
        managerVida.SendMessage("RecibirDano", danioMortal, SendMessageOptions.DontRequireReceiver);
        managerVida.SendMessage("TakeDamage", danioMortal, SendMessageOptions.DontRequireReceiver);
    }

    Debug.Log("V19 atacó al jugador de cerca.");
}

    public void RecibirGolpeObjeto(float fuerzaGolpe)
    {
        // Si está muerto, no recibe golpes.
        if (estaMuerto)
        {
            return;
        }

        // Si el golpe fue muy débil, no cuenta.
        if (fuerzaGolpe < fuerzaMinimaObjeto)
        {
            Debug.Log("El objeto golpeó a V19, pero no con suficiente fuerza.");
            return;
        }

        // Mandamos el golpe a EnemyHealth para que baje la barra.
        if (vida != null)
        {
            vida.RecibirGolpeObjeto(fuerzaGolpe);

            Debug.Log("V19 recibió golpe de objeto. Vida: " + vida.vidaActual + " / " + vida.vidaMaxima);

            if (vida.estaMuerto || vida.vidaActual <= 0f)
            {
                DetenerIA();
            }

            return;
        }

        Debug.LogWarning("V19 recibió golpe, pero no tiene EnemyHealth asignado.");
    }

    public void RecibirDaño(float cantidad)
    {
        // V19 ignora daño normal.
        Debug.Log("V19 ignoró daño normal. Solo objetos lanzados le hacen daño.");
    }

    public void RecibirDanio(float cantidad)
    {
        // V19 ignora daño normal.
        Debug.Log("V19 ignoró daño normal. Solo objetos lanzados le hacen daño.");
    }

    public void RecibirDano(float cantidad)
    {
        // V19 ignora daño normal.
        Debug.Log("V19 ignoró daño normal. Solo objetos lanzados le hacen daño.");
    }

    public void TakeDamage(float cantidad)
    {
        // V19 ignora daño normal.
        Debug.Log("V19 ignoró daño normal. Solo objetos lanzados le hacen daño.");
    }

    public void Stun(float duracion)
    {
        // V19 ignora stun.
        Debug.Log("V19 ignoró el stun.");
    }

    public void Aturdir(float duracion)
    {
        // V19 ignora aturdimiento.
        Debug.Log("V19 ignoró el aturdimiento.");
    }

    private void IrAlSiguientePuntoPatrulla()
    {
        // Si no hay puntos de patrulla, no hacemos nada.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            return;
        }

        // Si el agente no está listo, salimos.
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        // Mandamos al punto actual.
        agente.SetDestination(puntosPatrulla[indicePatrulla].position);

        // Avanzamos al siguiente.
        indicePatrulla++;

        // Si se pasa, regresa al primero.
        if (indicePatrulla >= puntosPatrulla.Length)
        {
            indicePatrulla = 0;
        }
    }

    private void CambiarEstado(EstadoV19 nuevoEstado)
    {
        // Si está muerto, no cambia.
        if (estaMuerto)
        {
            return;
        }

        // Evitamos repetir el mismo estado.
        if (estadoActual == nuevoEstado)
        {
            return;
        }

        // Cambiamos estado.
        estadoActual = nuevoEstado;

        Debug.Log("V19 cambió a estado: " + nuevoEstado);
    }

    private void RotarSoloEnYHacia(Vector3 objetivo)
    {
        // Dirección hacia objetivo.
        Vector3 direccion = objetivo - transform.position;

        // Eliminamos altura.
        direccion.y = 0f;

        // Si no hay dirección válida, salimos.
        if (direccion.sqrMagnitude < 0.01f)
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

    private void RotarHaciaMovimiento()
    {
        // Si no hay agente, no rotamos.
        if (agente == null)
        {
            return;
        }

        // Dirección del movimiento.
        Vector3 direccion = agente.velocity;

        // Eliminamos altura.
        direccion.y = 0f;

        // Si casi no se mueve, salimos.
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
        // Conservamos solo la Y.
        float rotacionY = transform.eulerAngles.y;

        // Forzamos X y Z en cero.
        transform.rotation = Quaternion.Euler(0f, rotacionY, 0f);
    }

    public void DetenerIA()
    {
        // Evitamos detener dos veces.
        if (estaMuerto)
        {
            return;
        }

        // Marcamos muerte.
        estaMuerto = true;
        estadoActual = EstadoV19.Muerto;

        // Detenemos agente.
        if (agente != null && agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
            agente.enabled = false;
        }

        // Dejamos recto.
        MantenerRotacionRecta();

        // Apagamos script.
        enabled = false;

        Debug.Log("IA de V19 detenida.");
    }

    public void Morir()
    {
        DetenerIA();
    }

private void OnCollisionStay(Collision collision)
{
    // Si V19 está muerto, no ataca.
    if (estaMuerto)
    {
        return;
    }

    // Si está mirando a V19, no debe atacar.
    if (elJugadorLoEstaMirando)
    {
        return;
    }

    // Si el objeto tocado pertenece al jugador, ataca.
    if (EsJugador(collision.transform))
    {
        DetenerMovimientoParaAtacar();
        AtacarJugador();
    }
}

private void OnTriggerStay(Collider other)
{
    // Si V19 está muerto, no ataca.
    if (estaMuerto)
    {
        return;
    }

    // Si está mirando a V19, no debe atacar.
    if (elJugadorLoEstaMirando)
    {
        return;
    }

    // Si el trigger tocado pertenece al jugador, ataca.
    if (EsJugador(other.transform))
    {
        DetenerMovimientoParaAtacar();
        AtacarJugador();
    }
}

private bool EsJugador(Transform objetoTocado)
{
    // Si no hay jugador asignado, no podemos comparar.
    if (jugador == null || objetoTocado == null)
    {
        return false;
    }

    // Si tocó exactamente el transform del jugador.
    if (objetoTocado == jugador)
    {
        return true;
    }

    // Si tocó un hijo del jugador.
    if (objetoTocado.IsChildOf(jugador))
    {
        return true;
    }

    // Si por alguna razón el jugador está dentro del objeto tocado.
    if (jugador.IsChildOf(objetoTocado))
    {
        return true;
    }

    // Si usas tag Player, también lo detectamos.
    if (objetoTocado.CompareTag("Player"))
    {
        return true;
    }

    return false;
}





    private void OnDrawGizmosSelected()
    {
        // Rango de activación.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);

        // Rango donde mirarlo lo detiene.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaMaximaParaDetenerlo);

        // Rango de ataque.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}