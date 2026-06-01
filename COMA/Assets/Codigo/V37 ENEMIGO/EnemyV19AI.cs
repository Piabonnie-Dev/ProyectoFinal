using UnityEngine;
using UnityEngine.AI;

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

    [Header("Detección por mirada")]
    public float distanciaMaximaParaDetenerlo = 35f;
    public float anguloMiradaJugador = 28f;

    [Header("Detección / persecución")]
    public float distanciaActivacion = 40f;
    public float tiempoMemoriaJugador = 8f;

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

    [Header("Muerte por objetos")]
    public int golpesDeObjetoParaMorir = 1;
    public float fuerzaMinimaObjeto = 9f;

    [Header("Estado actual")]
    public EstadoV19 estadoActual = EstadoV19.Patrulla;

    private int indicePatrulla = 0;
    private float contadorAtaque = 0f;
    private float contadorMemoria = 0f;
    private int golpesRecibidosPorObjeto = 0;

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
        // Buscamos el NavMeshAgent si no fue asignado desde el Inspector.
        if (agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
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

        // Buscamos la cámara principal del jugador si no fue asignada.
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Configuramos el agente para que no rote solo.
        // Así evitamos inclinaciones raras en X o Z.
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

        // V19 inicia patrullando.
        IrAlSiguientePuntoPatrulla();
    }

    private void Update()
    {
        // Si la partida terminó, no seguimos procesando IA.
        if (ControlPartida.PartidaTerminada)
        {
            return;
        }

        // Si está muerto, no hace nada.
        if (estaMuerto)
        {
            return;
        }

        // Si faltan referencias importantes, no hacemos nada.
        if (jugador == null || agente == null || !agente.enabled)
        {
            return;
        }

        // Forzamos que V19 no se incline.
        MantenerRotacionRecta();

        // Bajamos el contador de ataque.
        if (contadorAtaque > 0f)
        {
            contadorAtaque -= Time.deltaTime;
        }

        // Revisamos si el jugador está mirando a V19.
        bool jugadorLoEstaMirando = JugadorEstaMirandoAV19();

        // Revisamos si V19 está suficientemente cerca para activarse.
        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        bool jugadorDentroDeRango = distanciaJugador <= distanciaActivacion;

        // Si el jugador lo mira, V19 se congela.
        if (jugadorLoEstaMirando)
        {
            CambiarEstado(EstadoV19.CongeladoPorMirada);
        }
        else
        {
            // Si no lo mira y está en rango, V19 persigue.
            if (jugadorDentroDeRango)
            {
                contadorMemoria = tiempoMemoriaJugador;
                CambiarEstado(EstadoV19.Persecucion);
            }
            else
            {
                contadorMemoria -= Time.deltaTime;
            }
        }

        // Ejecutamos el comportamiento del estado actual.
        switch (estadoActual)
        {
            case EstadoV19.Patrulla:
                EstadoPatrulla();
                break;

            case EstadoV19.CongeladoPorMirada:
                EstadoCongeladoPorMirada();
                break;

            case EstadoV19.Persecucion:
                EstadoPersecucion();
                break;
        }
    }

    private void EstadoPatrulla()
    {
        // Si no hay agente activo, salimos.
        if (agente == null || !agente.enabled)
        {
            return;
        }

        // Patrulla lenta.
        agente.speed = velocidadPatrulla;
        agente.isStopped = false;

        // Rota hacia donde camina.
        RotarHaciaMovimiento();

        // Si llegó a su punto de patrulla, va al siguiente.
        if (!agente.pathPending && agente.remainingDistance <= 0.6f)
        {
            IrAlSiguientePuntoPatrulla();
        }
    }

    private void EstadoCongeladoPorMirada()
    {
        // Cuando el jugador lo mira, V19 se queda completamente quieto.
        if (agente != null && agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }

        // Aunque esté congelado, mira al jugador.
        // Esto hace que se sienta amenazante.
        RotarSoloEnYHacia(jugador.position);
    }

    private void EstadoPersecucion()
    {
        // Si no hay agente activo, salimos.
        if (agente == null || !agente.enabled)
        {
            return;
        }

        // V19 corre rápido cuando no lo estás mirando.
        agente.speed = velocidadPersecucion;
        agente.acceleration = aceleracion;
        agente.isStopped = false;

        // Persigue directamente al jugador.
        if (agente.isOnNavMesh)
        {
            agente.SetDestination(jugador.position);
        }

        // Rota hacia el movimiento para que no camine de lado o de espaldas.
        if (agente.velocity.sqrMagnitude > 0.1f)
        {
            RotarHaciaMovimiento();
        }
        else
        {
            RotarSoloEnYHacia(jugador.position);
        }

        // Si alcanza al jugador, lo mata de un golpe.
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque)
        {
            AtacarJugador();
        }

        // Si lo pierde mucho tiempo, regresa a patrulla.
        contadorMemoria -= Time.deltaTime;

        if (contadorMemoria <= 0f)
        {
            CambiarEstado(EstadoV19.Patrulla);
        }
    }

    private bool JugadorEstaMirandoAV19()
    {
        // Si no hay cámara, no puede detectar la mirada.
        if (camaraJugador == null)
        {
            return false;
        }

        // Calculamos distancia entre la cámara del jugador y V19.
        float distancia = Vector3.Distance(camaraJugador.transform.position, transform.position);

        // Si está muy lejos, mirarlo no lo detiene.
        if (distancia > distanciaMaximaParaDetenerlo)
        {
            return false;
        }

        // Dirección desde la cámara hacia V19.
        Vector3 direccionHaciaV19 = transform.position - camaraJugador.transform.position;

        // Calculamos si V19 está dentro del centro de la mirada.
        float angulo = Vector3.Angle(camaraJugador.transform.forward, direccionHaciaV19);

        // Si el ángulo es mayor al permitido, el jugador no lo está mirando de verdad.
        if (angulo > anguloMiradaJugador)
        {
            return false;
        }

        // Revisamos si hay una pared entre el jugador y V19.
        if (Physics.Raycast(
            camaraJugador.transform.position,
            direccionHaciaV19.normalized,
            out RaycastHit hit,
            distancia,
            capaObstaculos))
        {
            return false;
        }

        // Si pasó distancia, ángulo y línea de visión, entonces sí lo está mirando.
        return true;
    }

    private void AtacarJugador()
    {
        // Evitamos repetir ataques en el mismo instante.
        if (contadorAtaque > 0f)
        {
            return;
        }

        // Reiniciamos cooldown.
        contadorAtaque = tiempoEntreAtaques;

        // Daño mortal al jugador.
        if (jugador != null)
        {
            jugador.SendMessage("RecibirDaño", danioMortal, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("RecibirDanio", danioMortal, SendMessageOptions.DontRequireReceiver);
            jugador.SendMessage("TakeDamage", danioMortal, SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("V19 mató al jugador de un golpe.");
    }

    public void RecibirGolpeObjeto(float fuerzaGolpe)
    {
        // Si ya murió, no hacemos nada.
        if (estaMuerto)
        {
            return;
        }

        // Si el objeto no venía con suficiente fuerza, V19 lo ignora.
        if (fuerzaGolpe < fuerzaMinimaObjeto)
        {
            Debug.Log("El objeto golpeó a V19, pero no con suficiente fuerza.");
            return;
        }

        // Sumamos un golpe válido.
        golpesRecibidosPorObjeto++;

        Debug.Log("V19 recibió golpe de objeto. Golpes: " + golpesRecibidosPorObjeto);

        // Si llegó a los golpes necesarios, muere.
        if (golpesRecibidosPorObjeto >= golpesDeObjetoParaMorir)
        {
            MorirPorObjeto();
        }
    }

    private void MorirPorObjeto()
    {
        // Marcamos muerto.
        estaMuerto = true;
        estadoActual = EstadoV19.Muerto;

        // Detenemos completamente el agente.
        if (agente != null && agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.enabled = false;
        }

        // Dejamos la rotación recta.
        MantenerRotacionRecta();

        // Apagamos este script.
        enabled = false;

        Debug.Log("V19 murió por un objeto lanzado.");
    }

    public void RecibirDaño(float cantidad)
    {
        // V19 ignora balas y daño normal.
        Debug.Log("V19 ignoró el daño normal.");
    }

    public void RecibirDanio(float cantidad)
    {
        // V19 ignora balas y daño normal.
        Debug.Log("V19 ignoró el daño normal.");
    }

    public void TakeDamage(float cantidad)
    {
        // V19 ignora balas y daño normal.
        Debug.Log("V19 ignoró el daño normal.");
    }

    public void Stun(float duracion)
    {
        // V19 no puede ser stuneado.
        Debug.Log("V19 ignoró el stun.");
    }

    public void Aturdir(float duracion)
    {
        // V19 no puede ser aturdido.
        Debug.Log("V19 ignoró el aturdimiento.");
    }

    private void IrAlSiguientePuntoPatrulla()
    {
        // Si no hay puntos, no patrulla.
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            return;
        }

        // Si el agente no está listo, salimos.
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        // Mandamos a V19 al punto actual.
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
        // Si está muerto, no cambia de estado.
        if (estaMuerto)
        {
            return;
        }

        // Evitamos repetir el mismo estado cada frame.
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
        // Calculamos dirección hacia el objetivo.
        Vector3 direccion = objetivo - transform.position;

        // Eliminamos altura para que no se incline.
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
        // Tomamos la dirección del movimiento del agente.
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
        // Guardamos la rotación en Y.
        float rotacionY = transform.eulerAngles.y;

        // Forzamos X y Z en cero.
        transform.rotation = Quaternion.Euler(0f, rotacionY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de activación.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);

        // Distancia para detenerlo con la mirada.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaMaximaParaDetenerlo);

        // Distancia de ataque mortal.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}