
using UnityEngine;

public class TrapStun : MonoBehaviour
{
   [Header("Stun")]
   public float duracionStun = 4f;
   public float radioDeteccion = 1.8f;
   public float radioStun = 3f;
   public float tiempoArmado = 0.8f;

//Indica si la trampa ya esta lista para activarse.
   private bool armada = false;
   //Evitamos que la trampa se active mas de una vez
   private bool activada = false;

   void Start()
    {
        //Esperamos un pequeño tiempo antes de armar la trampa
        Invoke(nameof(Armar), tiempoArmado);

    }

    void Update()
    {
        //si no esta armada, no revisamos a los enemigos
        if(!armada)
        return;
//si ya fue activado, no revisamos nada.
        if(activada)
        return;
//Revisamos si hay enemigos cerca.
        RevisarEnemigosCercanos();

    }
    void Armar()
    {
        //La trampa ya puede detectar enemigos
        armada = true;

        Debug.Log("Trampa stun armada");
    }


    void RevisarEnemigosCercanos()
    {
        Collider [] hits = Physics.OverlapSphere(transform.position, radioDeteccion);

        for (int i = 0; i< hits.Length; i++)
        {
            
            EnemyHealth enemigo = hits [i].GetComponentInParent<EnemyHealth>();

            if(enemigo != null)
            {
                ActivarStun();
                return;
            }
        }
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
//Dibujamos el radio de deteccion
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        //Dibujamos el radio del stun
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioStun);
    }
}

