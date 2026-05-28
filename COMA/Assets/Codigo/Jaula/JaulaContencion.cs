using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class JaulaContencion : MonoBehaviour
{
    [Header("Punto interno de la jaula")]
public Transform puntoContenido;
    
    [Header("Vida de la jaula")]
    public float vidaMaximaJaula = 100f;
    public float vidaActualJaula = 100f;

    [Header("Danio del enemigo vivo")]
    public float danioPorGolpeEnemigo = 8f;
    public float intervaloGolpeEnemigo = 1.2f;

    [Header("Feedback visual")]
    public Renderer[] partesVisualesJaula;
    public Color colorSano = Color.cyan;
    public Color colorDaniado = Color.red;

    [Header("Transporte")]
    public bool puedeMoverse = true;
    public float pesoParaConsiderarlaLigera = 18f;
    
    [Header("Estado de captura")]
    public EnemyHealth enemigoCapturado;

    public bool tieneCriatura = false;
    public bool estaRota = false; 

//Componentes del enemigo que se apagaron al capturar. 
    private List<MonoBehaviour> scriptsIAApagados = new List<MonoBehaviour>();

private List<Collider> collidersApagados = new List<Collider>();

private NavMeshAgent agentCapturado;
private Rigidbody rb;
private float proximoGolpe = 0f;


private void Awake()
    {
        vidaActualJaula = vidaMaximaJaula;
        rb = GetComponent<Rigidbody>();

        if(partesVisualesJaula != null || partesVisualesJaula.Length == 0)
        {
            partesVisualesJaula = GetComponentsInChildren<Renderer>();

        }
        ActualizarColorJaula();
    }
    private void Update()
    {
        // Si no hay criatura, no hace daño interno.
        if (!tieneCriatura)
            return;

        // Si la jaula está rota, no hace nada.
        if (estaRota)
            return;

        // Si el enemigo está vivo, golpea la jaula cada cierto tiempo.
        if (enemigoCapturado != null && !enemigoCapturado.estaMuerto)
        {
            ProcesarGolpesDelEnemigo();
        }
    }

    public void CapturarEnemigo(EnemyHealth enemigo)
    {
       if(enemigo == null)
       {
        return;
       }


       enemigoCapturado = enemigo;

       tieneCriatura = true;
       if(puntoContenido == null)
        {
            puntoContenido = transform;
        }

        enemigo.transform.position = puntoContenido.position;
        //Hacemos que el enemigo sea hijo de la jaula, asi se mueve con ella.
        enemigo.transform.SetParent(transform);

        UnityEngine.AI.NavMeshAgent agent = enemigo.GetComponent<UnityEngine.AI.NavMeshAgent>();
if(agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        MonoBehaviour[] scripts = enemigo.GetComponents<MonoBehaviour>();
foreach (MonoBehaviour script in scripts)
    {
        if (script == null)
        {
            continue;
        }

        if (script is EnemyHealth)
        {
            continue;
        }

        string nombreScript = script.GetType().Name;

        if (nombreScript.Contains("AI") || nombreScript.Contains("EnemyVision") || nombreScript.Contains("EnemyBlind"))
        {
            script.enabled = false;
        }
    }

    Debug.Log("Enemigo capturado dentro de la jaula: " + enemigo.name);
    }

    private void ApagarMovimientoEIA(EnemyHealth enemigo)
    {
        agentCapturado = enemigo.GetComponent<NavMeshAgent>();
        if(agentCapturado != null)
        {
            agentCapturado.ResetPath();
            agentCapturado.enabled = false;
        }

        MonoBehaviour[] scripts = enemigo.GetComponents<MonoBehaviour>();

        // Apagamos scripts de IA, pero no apagamos EnemyHealth.
        for (int i = 0; i < scripts.Length; i++)
        {
            MonoBehaviour script = scripts[i];

            if (script == null)
                continue;

            // No apagamos EnemyHealth.
            if (script is EnemyHealth)
                continue;

            // Apagamos scripts cuyo nombre parezca IA.
            string nombreScript = script.GetType().Name;

            if (nombreScript.Contains("AI") || nombreScript.Contains("EnemyVision") || nombreScript.Contains("EnemyBlind"))
            {
                script.enabled = false;
                scriptsIAApagados.Add(script);
            }
        }
    }

    private void ApagarCollidersEnemigo(EnemyHealth enemigo)
    {
        // Tomamos todos los colliders del enemigo.
        Collider[] colliders = enemigo.GetComponentsInChildren<Collider>();

        // Los apagamos para evitar colisiones raras dentro de la jaula.
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;

            colliders[i].enabled = false;
            collidersApagados.Add(colliders[i]);
        }
    }

    private void ProcesarGolpesDelEnemigo()
    {
        // Revisamos si ya toca otro golpe.
        if (Time.time < proximoGolpe)
            return;

        // Programamos el siguiente golpe.
        proximoGolpe = Time.time + intervaloGolpeEnemigo;

        // Aplicamos daño a la jaula.
        RecibirDanioJaula(danioPorGolpeEnemigo);

        Debug.Log("La criatura golpeó la jaula. Vida jaula: " + vidaActualJaula);
    }

    public void RecibirDanioJaula(float cantidad)
    {
        // Si ya está rota, no recibe más daño.
        if (estaRota)
            return;

        // Bajamos vida.
        vidaActualJaula -= cantidad;

        // Limitamos la vida entre 0 y la máxima.
        vidaActualJaula = Mathf.Clamp(vidaActualJaula, 0f, vidaMaximaJaula);

        // Actualizamos color.
        ActualizarColorJaula();

        // Si llegó a 0, se rompe.
        if (vidaActualJaula <= 0f)
        {
            RomperJaula();
        }
    }

    private void ActualizarColorJaula()
    {
        // Calculamos porcentaje de vida.
        float porcentaje = vidaActualJaula / vidaMaximaJaula;

        // Entre menos vida, más rojo.
        Color colorActual = Color.Lerp(colorDaniado, colorSano, porcentaje);

        // Aplicamos el color a cada parte visual.
        for (int i = 0; i < partesVisualesJaula.Length; i++)
        {
            if (partesVisualesJaula[i] == null)
                continue;

            partesVisualesJaula[i].material.color = colorActual;
        }
    }

    private void RomperJaula()
    {
        // Marcamos la jaula como rota.
        estaRota = true;

        Debug.Log("La jaula se rompió.");

        // Liberamos al enemigo.
        LiberarEnemigo();

        // Destruimos la jaula.
        Destroy(gameObject);
    }

    private void LiberarEnemigo()
    {
        // Si no hay enemigo, no hacemos nada.
        if (enemigoCapturado == null)
            return;

        // Quitamos al enemigo como hijo de la jaula.
        enemigoCapturado.transform.SetParent(null);

        // Reactivamos colliders.
        for (int i = 0; i < collidersApagados.Count; i++)
        {
            if (collidersApagados[i] != null)
                collidersApagados[i].enabled = true;
        }

        // Si el enemigo está muerto, no reactivamos IA.
        if (enemigoCapturado.estaMuerto)
            return;

        // Reactivamos NavMeshAgent.
        if (agentCapturado != null)
        {
            agentCapturado.enabled = true;

            // Lo colocamos cerca de la posición actual sobre NavMesh.
            NavMeshHit hit;
            if (NavMesh.SamplePosition(enemigoCapturado.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                agentCapturado.Warp(hit.position);
            }
        }

        // Reactivamos scripts de IA.
        for (int i = 0; i < scriptsIAApagados.Count; i++)
        {
            if (scriptsIAApagados[i] != null)
                scriptsIAApagados[i].enabled = true;
        }
    }

    public bool EsLigera()
    {
        // Si no tiene Rigidbody, la consideramos ligera.
        if (rb == null)
            return true;

        // Si la masa está dentro del límite, se puede cargar.
        return rb.mass <= pesoParaConsiderarlaLigera;
    }
    }

