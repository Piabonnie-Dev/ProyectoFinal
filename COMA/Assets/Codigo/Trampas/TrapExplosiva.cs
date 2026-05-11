using UnityEngine;

public class TrapExplosiva : MonoBehaviour
{
    [Header("Explosión")]
    public float danio = 50f;
    public float radioDeteccion = 1.8f;
    public float radioExplosion = 4f;
    public float tiempoArmado = 0.8f;
// Esta variable indica si la mina ya puede activarse
    private bool armada = false;

    //Esta variable evita que explote dos veces
    private bool activada = false;

    void Start()
    {//en esta linea se espera un poco para que la mina logre de terminar activarse.
    // Y esto evita a que explote apenas toque el suelo o justo cuando se lanze.
        Invoke(nameof(Armar), tiempoArmado);
    }


void Update()
    {
        // Si todavia no esta armada, no revisamos nada.
        if(!armada)
        return;
// Si ya se activo, tampoco seguimos revisando. 
        if(activada)
        return;
//Revisamos si hay enemigos cerca usando una esfera invisible. 
        RevisarEnemigosCercanos();
    }
    void Armar()
    {
        //La mina ya quedo lista para detectar enemigos.
        armada = true;

        Debug.Log("Trampa explosiva armada");
    }

void RevisarEnemigosCercanos()
    {
        // Buscamos todos los colliders dentro del radio de deteccion.    
    Collider [] hits = Physics.OverlapSphere(transform.position, radioDeteccion);
//Recorremos todos los collideres encontrados
for (int i=0; i<hits.Length; i++)
        {
            //Intentamos encontrar el script EnemyHealth en el objeto o en su padre.
            EnemyHealth enemigo = hits[i].GetComponentInParent<EnemyHealth>();
// Si encontramos un enemigo, detonamos la mina.
            if(enemigo != null)
            {
                Explode();
                return;
            }
        }



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
        //Si ya exploto, no hacemos nada.
        if (activada)
            return;
//Marcamos que ya se activo para no repetir la explosion
        activada = true;
//Buscamos todo lo que este adentro del radio de la explosion nuevamente
        Collider[] hits = Physics.OverlapSphere(transform.position, radioExplosion);
//Recorremos todo lo alcanzado por la explosion
        for (int i = 0; i < hits.Length; i++)
        {
            //Si hay un enemigo dentro del radio, le aplicamos daño
            EnemyHealth enemigo = hits[i].GetComponentInParent<EnemyHealth>();
            if (enemigo != null)
                enemigo.RecibirDanio(danio);
//Aplicamos también daño al jugador
            PlayerHealth jugador = hits[i].GetComponentInParent<PlayerHealth>();
            if (jugador != null)
                jugador.RecibirDanio(danio * 0.5f);
        }

        Debug.Log("Trampa explosiva detonó.");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        //Dibujamos el radio pequeño donde detecta el enemigo.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
//Dibujamos el radio grande donde hace daño al explotar
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);

    }
}
