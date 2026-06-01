using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;

    [Header("Estado")]
    public bool estaMuerto = false;
    public bool estaAturdido = false;

    private NavMeshAgent agent;
    private float velocidadOriginal;
    private float tiempoFinStun = 0f;

    private void Awake()
    {
        // La vida actual inicia igual a la vida máxima.
        vidaActual = vidaMaxima;

        // Buscamos el NavMeshAgent del enemigo.
        agent = GetComponent<NavMeshAgent>();

        // Guardamos la velocidad original para recuperarla después del stun.
        if (agent != null)
        {
            velocidadOriginal = agent.speed;
        }
    }

    private void Update()
    {
        // Si ya murió, no procesamos nada más.
        if (estaMuerto)
        {
            return;
        }

        // Si estaba aturdido y ya terminó el tiempo, se recupera.
        if (estaAturdido && Time.time >= tiempoFinStun)
        {
            estaAturdido = false;

            // Restauramos la velocidad original.
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                agent.speed = velocidadOriginal;
            }
        }
    }

    public void RecibirDaño(float cantidad)
    {
        // Versión con ñ, por si la hitbox o el arma usan este nombre.
        AplicarDaño(cantidad);
    }

    public void RecibirDanio(float cantidad)
    {
        // Versión sin ñ, que es la que ya estabas usando.
        AplicarDaño(cantidad);
    }

    public void RecibirDano(float cantidad)
    {
        // Otra versión sin caracteres especiales.
        AplicarDaño(cantidad);
    }

    public void TakeDamage(float cantidad)
    {
        // Versión en inglés, por compatibilidad con otros scripts.
        AplicarDaño(cantidad);
    }

    private void AplicarDaño(float cantidad)
    {
        // Si ya murió, no recibe más daño.
        if (estaMuerto)
        {
            return;
        }

        // Restamos vida.
        vidaActual -= cantidad;

        // Limitamos la vida entre 0 y vida máxima.
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);

        // Si la vida llega a cero, muere.
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    public void Aturdir(float duracion)
    {
        // Si ya murió, no puede ser aturdido.
        if (estaMuerto)
        {
            return;
        }

        // Marcamos estado de aturdimiento.
        estaAturdido = true;

        // Calculamos cuándo termina el stun.
        tiempoFinStun = Time.time + duracion;

        // Detenemos el NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            velocidadOriginal = agent.speed;
            agent.isStopped = true;
            agent.speed = 0f;
            agent.ResetPath();
        }

        // Avisamos a la IA que también debe detenerse.
        BroadcastMessage("Stun", duracion, SendMessageOptions.DontRequireReceiver);
    }

    public void Morir()
    {
        // Evitamos ejecutar muerte dos veces.
        if (estaMuerto)
        {
            return;
        }

        // Marcamos muerte.
        estaMuerto = true;
        vidaActual = 0f;

        // Avisamos a las IA del enemigo que deben apagarse.
        BroadcastMessage("DetenerIA", SendMessageOptions.DontRequireReceiver);

        // Detenemos completamente el NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        Debug.Log(gameObject.name + " ha muerto.");
    }
}