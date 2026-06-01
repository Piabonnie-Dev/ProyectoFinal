
using UnityEngine;

public class ObjetoArrojadoDaniaV19 : MonoBehaviour
{
    [Header("Daño contra V19")]
    public bool puedeDaniarV19 = false;
    public float tiempoActivoDespuesDeLanzar = 2f;
    public float velocidadMinimaParaDaniar = 9f;


    private Rigidbody rb;
    private float tiempoRestanteActivo = 0f;

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        if (!puedeDaniarV19)
        {
            return;
        }
        tiempoRestanteActivo -= Time.deltaTime;

        if(tiempoRestanteActivo <= 0f)
        {
            puedeDaniarV19 =false;
        }


    }

    public void ActivarComoProyectil()
    {
        puedeDaniarV19 = true;

        tiempoRestanteActivo = tiempoActivoDespuesDeLanzar;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!puedeDaniarV19)
        {
            return;
        }

        EnemyV19AI v19 = collision.collider.GetComponent<EnemyV19AI>();

        if(v19 == null)
        {
            return;
        }

        float fuerzaImpacto = collision.relativeVelocity.magnitude;

        if(fuerzaImpacto < velocidadMinimaParaDaniar)
        {
            Debug.Log("Objeto golpeo a V19, pero iba muy lento.");
            return;
        }


        v19.RecibirGolpeObjeto(fuerzaImpacto);

        puedeDaniarV19 = false;
    }
}
