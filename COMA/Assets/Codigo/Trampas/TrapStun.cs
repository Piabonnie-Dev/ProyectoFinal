
using UnityEngine;

public class TrapStun : MonoBehaviour
{
   [Header("Stun")]
   public float duracionStun = 4f;
   public float radioStun = 3f;
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
        if(!armada || activada)
        return;

        EnemyHealth enemigo = collision.collider.GetComponentInParent<EnemyHealth>();
        if(enemigo != null)
        {
            ActivarStun();

        }
    }
        void ActivarStun()
        {
            if(activada)
            return;

            activada= true;

            Collider[] hits = Physics.OverlapSphere(transform.position,radioStun);

            for (int i = 0; i < hits.Length; i++)
            {
                EnemyHealth enemigo = hits[i].GetComponentInParent<EnemyHealth>();
                if(enemigo!= null)
                enemigo.Aturdir(duracionStun);
            }
            Debug.Log("Trampa stun Activada");
            Destroy(gameObject);

        }

void OnDrawnGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioStun);
    }
}

