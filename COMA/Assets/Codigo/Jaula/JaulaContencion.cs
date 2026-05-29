using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JaulaContencion : MonoBehaviour
{
    [Header("Punto interno de la jaula")]

    // Lugar donde se coloca el enemigo dentro de la jaula.
    public Transform puntoContenido;

    [Header("Vida de la jaula")]

    // Vida máxima de la jaula.
    public float vidaMaximaJaula = 100f;

    // Vida actual de la jaula.
    public float vidaActualJaula = 100f;

    [Header("Daño del enemigo vivo")]

    // Daño que hace el enemigo a la jaula si está vivo.
    public float danioPorGolpeEnemigo = 8f;

    // Tiempo entre golpes del enemigo encerrado.
    public float intervaloGolpeEnemigo = 1.2f;

    [Header("Colores de desgaste")]

    // Renderers de las barras de la jaula.
    public Renderer[] partesVisualesJaula;

    // Color sano.
    public Color colorSano = Color.cyan;

    // Color dañado.
    public Color colorDaniado = Color.red;

    [Header("Transporte")]

    // Permite mover la jaula.
    public bool puedeMoverse = true;

    // Si la masa es menor o igual a este valor, se puede cargar.
    public float pesoParaConsiderarlaLigera = 18f;

    [Header("Estado")]

    // Enemigo capturado.
    public EnemyHealth enemigoCapturado;

    // Indica si hay criatura dentro.
    public bool tieneCriatura = false;

    // Indica si la jaula ya se rompió.
    public bool estaRota = false;

    private Rigidbody rb;

    private NavMeshAgent agentCapturado;

    private Transform padreOriginalEnemigo;

    private readonly List<MonoBehaviour> scriptsIAApagados = new List<MonoBehaviour>();

    private readonly List<Collider> collidersApagados = new List<Collider>();

    private float proximoGolpe = 0f;

    private void Awake()
    {
        // Tomamos el Rigidbody de la jaula.
        rb = GetComponent<Rigidbody>();

        // Inicializamos la vida.
        vidaActualJaula = vidaMaximaJaula;

        // Si no asignaste renderers, los buscamos automáticamente.
        if (partesVisualesJaula == null || partesVisualesJaula.Length == 0)
        {
            partesVisualesJaula = GetComponentsInChildren<Renderer>();
        }

        // Aplicamos color inicial.
        ActualizarColorJaula();
    }

    private void Update()
    {
        // Si no hay criatura, no hay golpes internos.
        if (!tieneCriatura)
            return;

        // Si la jaula ya se rompió, no hacemos nada.
        if (estaRota)
            return;

        // Si el enemigo está vivo, golpea la jaula.
        if (enemigoCapturado != null && !enemigoCapturado.estaMuerto)
        {
            GolpearJaulaDesdeDentro();
        }
    }

    public void CapturarEnemigo(EnemyHealth enemigo)
    {
        // Validamos enemigo.
        if (enemigo == null)
            return;

        // Guardamos referencia.
        enemigoCapturado = enemigo;
        tieneCriatura = true;

        // Guardamos su padre original por si después queremos restaurarlo.
        padreOriginalEnemigo = enemigo.transform.parent;

        // Colocamos la jaula correctamente sobre el suelo.
        AcomodarJaulaEnSuelo(enemigo.transform.position);

        // Si no hay punto interno, usamos el centro del objeto.
        if (puntoContenido == null)
            puntoContenido = transform;

        // Movemos al enemigo dentro de la jaula.
        enemigo.transform.position = puntoContenido.position;

        // Lo enderezamos para que no quede acostado o torcido.
        enemigo.transform.rotation = Quaternion.Euler(0f, enemigo.transform.eulerAngles.y, 0f);

        // Lo hacemos hijo de la jaula para que viaje con ella.
        enemigo.transform.SetParent(transform, true);

        // Apagamos movimiento e IA.
        ApagarMovimientoEIA(enemigo);

        // Apagamos colliders para que no empuje la jaula desde adentro.
        ApagarCollidersEnemigo(enemigo);

        // Aseguramos física correcta de la jaula.
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        Debug.Log("Criatura contenida: " + enemigo.name);
    }

    private void AcomodarJaulaEnSuelo(Vector3 posicionBase)
    {
        // Lanzamos un rayo hacia abajo para encontrar el suelo real.
        Vector3 origen = posicionBase + Vector3.up * 3f;

        RaycastHit hit;

        if (Physics.Raycast(origen, Vector3.down, out hit, 8f, ~0, QueryTriggerInteraction.Ignore))
        {
            // Ponemos la jaula en el suelo.
            transform.position = hit.point;
        }
        else
        {
            // Si no encuentra suelo, usamos la posición del enemigo.
            transform.position = posicionBase;
        }

        // Enderezamos la jaula.
        transform.rotation = Quaternion.identity;
    }

    private void ApagarMovimientoEIA(EnemyHealth enemigo)
    {
        // Buscamos NavMeshAgent del enemigo.
        agentCapturado = enemigo.GetComponent<NavMeshAgent>();

        // Apagamos el agente.
        if (agentCapturado != null)
        {
            if (agentCapturado.enabled && agentCapturado.isOnNavMesh)
            {
                agentCapturado.ResetPath();
                agentCapturado.isStopped = true;
            }

            agentCapturado.enabled = false;
        }

        // Tomamos scripts del enemigo.
        MonoBehaviour[] scripts = enemigo.GetComponents<MonoBehaviour>();

        for (int i = 0; i < scripts.Length; i++)
        {
            MonoBehaviour script = scripts[i];

            if (script == null)
                continue;

            // No apagamos EnemyHealth.
            if (script is EnemyHealth)
                continue;

            string nombre = script.GetType().Name;

            // Apagamos scripts de IA.
            if (nombre.Contains("AI") || nombre.Contains("EnemyVision") || nombre.Contains("EnemyBlind"))
            {
                if (script.enabled)
                {
                    script.enabled = false;
                    scriptsIAApagados.Add(script);
                }
            }
        }
    }

    private void ApagarCollidersEnemigo(EnemyHealth enemigo)
    {
        Collider[] colliders = enemigo.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];

            if (col == null)
                continue;

            if (col.enabled)
            {
                col.enabled = false;
                collidersApagados.Add(col);
            }
        }
    }

    private void GolpearJaulaDesdeDentro()
    {
        // Todavía no toca el siguiente golpe.
        if (Time.time < proximoGolpe)
            return;

        // Programamos siguiente golpe.
        proximoGolpe = Time.time + intervaloGolpeEnemigo;

        // Aplicamos daño.
        RecibirDanioJaula(danioPorGolpeEnemigo);

        Debug.Log("La criatura golpeó la jaula. Vida: " + vidaActualJaula);
    }

    public void RecibirDanioJaula(float cantidad)
    {
        // Si ya está rota, no recibe daño.
        if (estaRota)
            return;

        // Restamos vida.
        vidaActualJaula -= cantidad;

        // Limitamos la vida.
        vidaActualJaula = Mathf.Clamp(vidaActualJaula, 0f, vidaMaximaJaula);

        // Actualizamos color.
        ActualizarColorJaula();

        // Si llegó a cero, se rompe.
        if (vidaActualJaula <= 0f)
        {
            RomperJaula();
        }
    }

    private void ActualizarColorJaula()
    {
        // Calculamos porcentaje.
        float porcentaje = vidaActualJaula / vidaMaximaJaula;

        // Mientras menos vida, más rojo.
        Color colorActual = Color.Lerp(colorDaniado, colorSano, porcentaje);

        for (int i = 0; i < partesVisualesJaula.Length; i++)
        {
            if (partesVisualesJaula[i] == null)
                continue;

            // Usamos material instanciado para que no cambie todos los prefabs.
            partesVisualesJaula[i].material.color = colorActual;
        }
    }

    private void RomperJaula()
    {
        // Marcamos como rota.
        estaRota = true;

        Debug.Log("La jaula se rompió.");

        // Liberamos al enemigo antes de destruir la jaula.
        LiberarEnemigo();

        // Destruimos la jaula.
        Destroy(gameObject);
    }

    private void LiberarEnemigo()
    {
        // Si no hay enemigo, salimos.
        if (enemigoCapturado == null)
            return;

        Transform enemigoTransform = enemigoCapturado.transform;

        // Lo sacamos de la jaula antes de destruirla.
        enemigoTransform.SetParent(padreOriginalEnemigo, true);

        // Enderezamos al enemigo.
        enemigoTransform.rotation = Quaternion.Euler(0f, enemigoTransform.eulerAngles.y, 0f);

        // Buscamos una posición válida en NavMesh cerca de donde está.
        Vector3 posicionLiberacion = enemigoTransform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(posicionLiberacion, out hit, 4f, NavMesh.AllAreas))
        {
            posicionLiberacion = hit.position;
        }

        enemigoTransform.position = posicionLiberacion;

        // Reactivamos colliders.
        for (int i = 0; i < collidersApagados.Count; i++)
        {
            if (collidersApagados[i] != null)
            {
                collidersApagados[i].enabled = true;
            }
        }

        // Si el enemigo está muerto, NO reactivamos IA.
        // Se queda como cadáver/capturable.
        if (enemigoCapturado.estaMuerto)
        {
            Debug.Log("La jaula se rompió, pero el enemigo estaba muerto.");
            return;
        }

        // Reactivamos NavMeshAgent.
        if (agentCapturado != null)
        {
            agentCapturado.enabled = true;

            if (agentCapturado.isOnNavMesh)
            {
                agentCapturado.Warp(posicionLiberacion);
                agentCapturado.isStopped = false;
                agentCapturado.ResetPath();
            }
        }

        // Reactivamos scripts de IA.
        for (int i = 0; i < scriptsIAApagados.Count; i++)
        {
            if (scriptsIAApagados[i] != null)
            {
                scriptsIAApagados[i].enabled = true;
            }
        }

        Debug.Log("Enemigo liberado y reactivado.");
    }

    public bool EsLigera()
    {
        // Si no tiene Rigidbody, la consideramos ligera.
        if (rb == null)
            return true;

        return rb.mass <= pesoParaConsiderarlaLigera;
    }
}