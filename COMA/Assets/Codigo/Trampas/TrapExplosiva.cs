using UnityEngine;

public class TrapExplosiva : MonoBehaviour
{
    [Header("Explosión")]
    public float danio = 50f;
    public float radioExplosion = 4f;
    public float tiempoArmado = 0.8f;

    private bool armada = false;
    private bool activada = false;

    void Start()
    {
        Invoke(nameof(Armar), tiempoArmado);
    }

    void Armar()
    {
        armada = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!armada || activada)
            return;

        EnemyHealth enemigo = collision.collider.GetComponentInParent<EnemyHealth>();
        PlayerHealth jugador = collision.collider.GetComponentInParent<PlayerHealth>();

        if (enemigo != null || jugador != null)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (activada)
            return;

        activada = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, radioExplosion);

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemigo = hits[i].GetComponentInParent<EnemyHealth>();
            if (enemigo != null)
                enemigo.RecibirDanio(danio);

            PlayerHealth jugador = hits[i].GetComponentInParent<PlayerHealth>();
            if (jugador != null)
                jugador.RecibirDanio(danio * 0.5f);
        }

        Debug.Log("Trampa explosiva detonó.");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}
