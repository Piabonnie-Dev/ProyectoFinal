using UnityEngine.AI;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
[Header( "Vida")]
public float vidaMaxima = 100f;
public float vidaActual = 100f;

[Header("Estado")]
public bool estaMuerto = false;
public bool estaAturdido = false;

private NavMeshAgent agent;
private float velocidadOriginal;
private float tiempoFinStun = 0f;

    void Awake()
    {
        vidaActual = vidaMaxima;
        agent = GetComponent<NavMeshAgent>();
        
        if(agent != null)
        velocidadOriginal = agent.speed;
    } 

    void Update()
    {
        if (estaMuerto)
        return;

        if(estaAturdido && Time.time >= tiempoFinStun){
        estaAturdido = false;

        if(agent != null)
        agent.speed = velocidadOriginal;
}

}

public void RecibirDanio(float cantidad)
    {
        if(estaMuerto)
        return;

        vidaActual -= cantidad;

        if( vidaActual <= 0f)
        {
            Morir();
        }
    }

    public void Aturdir(float duracion)
    {
        if(estaMuerto)
        return;

        estaAturdido = true;
        tiempoFinStun = Time.time + duracion;

        if(agent != null){
        velocidadOriginal = agent.speed;
        agent.speed = 0f;
        agent.ResetPath();
    }
    }

    void Morir()
    {
        estaMuerto = true;
        vidaActual = 0f;

        if(agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();

        }

        Debug.Log(gameObject.name + " ha muerto.");
        // Aqui falta poner animacion o ragdoll. 
    }
}