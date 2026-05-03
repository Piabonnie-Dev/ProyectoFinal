using UnityEngine;
using UnityEngine.AI;

public class EnemyBlindAI : MonoBehaviour
{
    public enum Estado
    {
        Patrulla,
        InvestigarRuido,
        Ataque
    }

    [Header("Referencias")]
    public Transform[] puntosPatrulla;
    public Transform jugador;
    public PlayerNoise playerNoise;

    [Header("Ruido")]
    public float tiempoMemoriaRuido = 3f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2.5f;
    public float velocidadPersecucion = 5f;
    public float tiempoEsperaPatrulla = 1f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.8f;
    public float danio = 12f;
    public float enfriamientoAtaque = 1f;

    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private PlayerHealth playerHealth;

    private Estado estadoActual = Estado.Patrulla;
    private int indicePatrulla = 0;
    private float temporizadorEspera = 0f;
    private float temporizadorRuido = 0f;
    private float ultimoAtaque = -999f;
    private Vector3 ultimaPosicionRuido;

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

            if (playerNoise == null)
                playerNoise = jugador.GetComponent<PlayerNoise>();
        }

        CambiarEstado(Estado.Patrulla);
    }

    void Update()
    {
        if (enemyHealth != null && (enemyHealth.estaMuerto || enemyHealth.estaAturdido))
            return;

        if (jugador == null || agent == null || playerNoise == null)
            return;

        DetectarRuido();

        switch (estadoActual)
        {
            case Estado.Patrulla:
                ActualizarPatrulla();
                break;

            case Estado.InvestigarRuido:
                ActualizarInvestigarRuido();
                break;

            case Estado.Ataque:
                ActualizarAtaque();
                break;
        }
    }

    void DetectarRuido()
    {
        if (!playerNoise.estaHaciendoRuido)
            return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= playerNoise.radioRuidoActual)
        {
            ultimaPosicionRuido = jugador.position;
            temporizadorRuido = tiempoMemoriaRuido;

            float distanciaAtaqueActual = Vector3.Distance(transform.position, jugador.position);

            if (distanciaAtaqueActual <= distanciaAtaque)
                CambiarEstado(Estado.Ataque);
            else
                CambiarEstado(Estado.InvestigarRuido);
        }
    }

    void CambiarEstado(Estado nuevoEstado)
    {
        if (estadoActual == nuevoEstado)
            return;

        estadoActual = nuevoEstado;

        switch (estadoActual)
        {
            case Estado.Patrulla:
                agent.speed = velocidadPatrulla;
                IrAlSiguientePunto();
                break;

            case Estado.InvestigarRuido:
                agent.speed = velocidadPersecucion;
                agent.SetDestination(ultimaPosicionRuido);
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

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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

    void ActualizarInvestigarRuido()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque && playerNoise.estaHaciendoRuido)
        {
            CambiarEstado(Estado.Ataque);
            return;
        }

        temporizadorRuido -= Time.deltaTime;

        if (playerNoise.estaHaciendoRuido)
        {
            float distanciaRuido = Vector3.Distance(transform.position, jugador.position);
            if (distanciaRuido <= playerNoise.radioRuidoActual)
            {
                ultimaPosicionRuido = jugador.position;
                agent.SetDestination(ultimaPosicionRuido);
                temporizadorRuido = tiempoMemoriaRuido;
            }
        }

        if (temporizadorRuido <= 0f && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            CambiarEstado(Estado.Patrulla);
        }
    }

    void ActualizarAtaque()
    {
        transform.LookAt(new Vector3(jugador.position.x, transform.position.y, jugador.position.z));

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaAtaque + 0.5f)
        {
            CambiarEstado(Estado.InvestigarRuido);
            return;
        }

        if (Time.time >= ultimoAtaque + enfriamientoAtaque)
        {
            ultimoAtaque = Time.time;

            if (playerHealth != null)
                playerHealth.RecibirDanio(danio);
        }
    }
}