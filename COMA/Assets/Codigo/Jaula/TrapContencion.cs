using UnityEngine;

public class TrapContencion : MonoBehaviour
{
    [Header("Prefab de la jaula")]

    // Prefab que se va a crear alrededor del enemigo capturado.
    public GameObject prefabJaulaContencion;

    [Header("Detección")]

    // Radio donde la trampa busca enemigos para capturar.
    public float radioCaptura = 2.2f;

    // Tiempo que tarda en armarse después de tocar el suelo.
    public float tiempoParaArmar = 0.5f;

    // Capas donde pueden estar los enemigos.
    // Puedes dejar Everything al inicio para probar.
    public LayerMask capasEnemigo = ~0;

    [Header("Estado")]

    // Indica si la trampa ya tocó el suelo.
    private bool tocoSuelo = false;

    // Indica si la trampa ya está armada.
    private bool armada = false;

    // Evita que capture dos veces.
    private bool usada = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya tocó suelo antes, no repetimos.
        if (tocoSuelo)
            return;

        // Marcamos que ya tocó algo sólido.
        tocoSuelo = true;

        // Armamos la trampa después de un pequeño tiempo.
        Invoke(nameof(Armar), tiempoParaArmar);
    }

    private void Armar()
    {
        // La trampa ya puede capturar.
        armada = true;

        Debug.Log("Trampa de contención armada.");
    }

    private void Update()
    {
        // Si no está armada, no busca enemigos.
        if (!armada)
            return;

        // Si ya se usó, no hace nada.
        if (usada)
            return;

        // Revisamos si hay enemigos dentro del radio.
        BuscarEnemigoParaCapturar();
    }

    private void BuscarEnemigoParaCapturar()
    {
        // Buscamos colliders dentro del radio de captura.
        Collider[] encontrados = Physics.OverlapSphere(
            transform.position,
            radioCaptura,
            capasEnemigo,
            QueryTriggerInteraction.Ignore
        );

        // Revisamos cada collider encontrado.
        for (int i = 0; i < encontrados.Length; i++)
        {
            // Buscamos EnemyHealth en el objeto o en sus padres.
            EnemyHealth enemigo = encontrados[i].GetComponentInParent<EnemyHealth>();

            // Si no tiene EnemyHealth, no es criatura capturable.
            if (enemigo == null)
                continue;

            // Capturamos al primer enemigo válido.
            Capturar(enemigo);
            return;
        }
    }

    private void Capturar(EnemyHealth enemigo)
    {
        // Evitamos doble captura.
        usada = true;

        // Validamos que exista el prefab de jaula.
        if (prefabJaulaContencion == null)
        {
            Debug.LogWarning("Falta asignar prefabJaulaContencion en TrapContencion.");
            return;
        }

        // Creamos la jaula en la posición del enemigo.
        GameObject nuevaJaula = Instantiate(
            prefabJaulaContencion,
            enemigo.transform.position,
            Quaternion.identity
        );

        // Buscamos el script de la jaula.
        JaulaContencion jaula = nuevaJaula.GetComponent<JaulaContencion>();

        // Si la jaula tiene su script, le pasamos el enemigo.
        if (jaula != null)
        {
            jaula.CapturarEnemigo(enemigo);
        }
        else
        {
            Debug.LogWarning("El prefab de jaula no tiene JaulaContencion.");
        }

        // Destruimos la trampa lanzable.
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujamos el radio de captura.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioCaptura);
    }
}
