using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;

    [Header("Reglas especiales de daño")]
    public bool soloDanoPorObjetos = false;
    public float danoPorObjeto = 1f;
    public float fuerzaMinimaObjeto = 9f;

    [Header("Estado")]
    public bool estaMuerto = false;
    public bool estaAturdido = false;

    private NavMeshAgent agent;
    private float velocidadOriginal;
    private float tiempoFinStun = 0f;

    private void Awake()
    {
        // La vida actual inicia llena.
        vidaActual = vidaMaxima;

        // Buscamos el NavMeshAgent del enemigo.
        agent = GetComponent<NavMeshAgent>();

        // Guardamos su velocidad original por si lo aturdimos.
        if (agent != null)
        {
            velocidadOriginal = agent.speed;
        }
    }

    private void Update()
    {
        // Si ya murió, no procesamos nada.
        if (estaMuerto)
        {
            return;
        }

        // Si estaba aturdido y ya terminó el tiempo, recupera movimiento.
        if (estaAturdido && Time.time >= tiempoFinStun)
        {
            estaAturdido = false;

            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                agent.speed = velocidadOriginal;
            }
        }
    }

    public void RecibirDaño(float cantidad)
    {
        // Versión con ñ.
        AplicarDanoNormal(cantidad);
    }

    public void RecibirDanio(float cantidad)
    {
        // Versión sin ñ.
        AplicarDanoNormal(cantidad);
    }

    public void RecibirDano(float cantidad)
    {
        // Otra versión sin caracteres especiales.
        AplicarDanoNormal(cantidad);
    }

    public void TakeDamage(float cantidad)
    {
        // Versión en inglés.
        AplicarDanoNormal(cantidad);
    }

    private void AplicarDanoNormal(float cantidad)
    {
        // Si ya murió, no recibe daño.
        if (estaMuerto)
        {
            return;
        }

        // Si este enemigo solo recibe daño por objetos, ignora balas, trampas y stun.
        if (soloDanoPorObjetos)
        {
            Debug.Log(gameObject.name + " ignoró daño normal. Solo objetos lanzados le hacen daño.");
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

    public void RecibirGolpeObjeto(float fuerzaGolpe)
    {
        // Si ya murió, no recibe daño.
        if (estaMuerto)
        {
            return;
        }

        // Si el objeto no venía con fuerza suficiente, no cuenta.
        if (fuerzaGolpe < fuerzaMinimaObjeto)
        {
            Debug.Log(gameObject.name + " recibió un golpe débil. No cuenta como daño.");
            return;
        }

        // Para V19, cada objeto lanzado le baja una cantidad fija.
        vidaActual -= danoPorObjeto;

        // Limitamos la vida.
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);

        Debug.Log(gameObject.name + " recibió golpe de objeto. Vida: " + vidaActual + " / " + vidaMaxima);

        // Si se queda sin vida, muere.
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    public void Aturdir(float duracion)
    {
        // Si ya murió, no se aturde.
        if (estaMuerto)
        {
            return;
        }

        // Si solo recibe daño por objetos, también ignora aturdimiento.
        if (soloDanoPorObjetos)
        {
            Debug.Log(gameObject.name + " ignoró el aturdimiento.");
            return;
        }

        // Activamos estado de aturdimiento.
        estaAturdido = true;

        // Guardamos cuándo termina.
        tiempoFinStun = Time.time + duracion;

        // Detenemos el NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            velocidadOriginal = agent.speed;
            agent.isStopped = true;
            agent.speed = 0f;
            agent.ResetPath();
        }

        // Avisamos a la IA que también está stuneada.
        BroadcastMessage("Stun", duracion, SendMessageOptions.DontRequireReceiver);
    }

    public void Morir()
    {
        // Evitamos ejecutar muerte dos veces.
        if (estaMuerto)
        {
            return;
        }

        // Marcamos muerto.
        estaMuerto = true;
        vidaActual = 0f;

        // Avisamos a la IA que debe detenerse.
        BroadcastMessage("DetenerIA", SendMessageOptions.DontRequireReceiver);

        // Detenemos el NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        Debug.Log(gameObject.name + " ha muerto.");
    }
}