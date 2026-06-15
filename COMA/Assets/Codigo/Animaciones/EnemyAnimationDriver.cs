
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationDriver : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;
    public NavMeshAgent agente;
    public EnemyHealth vida;

    [Header("Tipo de enemigo")]
    public EnemyV19AI enemigoV19;
    public EnemyV37AI enemigoV37;

    [Header("Nombres de estados del Animator")]
    public string estadoIdle = "Idle";
    public string estadoCaminar = "Walk";
    public string estadoCorrer = "Run";
    public string estadoAtaque = "Attack";
    public string estadoGolpe = "Hit";
    public string estadoMuerte = "Die";

    [Header("Movimiento")]
    public float velocidadMinimaMovimiento = 0.12f;
    public float velocidadConsideradaCarrera = 3.5f;

    [Header("Transiciones")]
    public float duracionTransicion = 0.12f;

    [Header("Duración de acciones")]
    public float duracionAtaque = 0.8f;
    public float duracionGolpe = 0.45f;

    private string estadoReproduciendose = "";
    private float bloqueoAnimacionHasta = 0f;
    private bool muerteReproducida = false;

    private void Awake()
    {
        /*
         * El Animator normalmente se encuentra en el modelo,
         * que puede ser un hijo del objeto principal del enemigo.
         */
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        /*
         * El NavMeshAgent y las IA normalmente están
         * en el objeto raíz del enemigo.
         */
        if (agente == null)
        {
            agente = GetComponentInParent<NavMeshAgent>();
        }

        if (agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
        }

        if (vida == null)
        {
            vida = GetComponentInParent<EnemyHealth>();
        }

        if (vida == null)
        {
            vida = GetComponent<EnemyHealth>();
        }

        if (enemigoV19 == null)
        {
            enemigoV19 = GetComponentInParent<EnemyV19AI>();
        }

        if (enemigoV19 == null)
        {
            enemigoV19 = GetComponent<EnemyV19AI>();
        }

        if (enemigoV37 == null)
        {
            enemigoV37 = GetComponentInParent<EnemyV37AI>();
        }

        if (enemigoV37 == null)
        {
            enemigoV37 = GetComponent<EnemyV37AI>();
        }

        if (animator != null)
        {
            /*
             * El NavMeshAgent ya mueve al enemigo.
             * Root Motion debe permanecer desactivado
             * para evitar que el modelo avance dos veces.
             */
            animator.applyRootMotion = false;
        }
    }

    private void Start()
    {
        CambiarEstado(estadoIdle);
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        /*
         * Revisamos muerte antes que cualquier otra animación.
         */
        if (EstaMuerto())
        {
            ReproducirMuerte();
            return;
        }

        /*
         * Mientras se reproduce ataque o golpe,
         * no dejamos que caminar lo interrumpa.
         */
        if (Time.time < bloqueoAnimacionHasta)
        {
            return;
        }

        /*
         * V19 debe quedarse quieto cuando el jugador lo mira.
         */
        if (enemigoV19 != null &&
            enemigoV19.estadoActual ==
            EnemyV19AI.EstadoV19.CongeladoPorMirada)
        {
            CambiarEstado(estadoIdle);
            return;
        }

        ActualizarLocomocion();
    }

    private void ActualizarLocomocion()
    {
        if (agente == null ||
            !agente.enabled ||
            !agente.isOnNavMesh ||
            agente.isStopped)
        {
            CambiarEstado(estadoIdle);
            return;
        }

        float velocidadActual = agente.velocity.magnitude;

        if (velocidadActual <= velocidadMinimaMovimiento)
        {
            CambiarEstado(estadoIdle);
            return;
        }

        bool debeCorrer = velocidadActual >= velocidadConsideradaCarrera;

        /*
         * También utilizamos los estados de las IA
         * para reconocer una persecución.
         */
        if (enemigoV19 != null &&
            enemigoV19.estadoActual ==
            EnemyV19AI.EstadoV19.Persecucion)
        {
            debeCorrer = true;
        }

        if (enemigoV37 != null &&
            enemigoV37.estadoActual ==
            EnemyV37AI.EstadoV37.Persecucion)
        {
            debeCorrer = true;
        }

        if (debeCorrer)
        {
            /*
             * Si el Animator no tiene estado Run,
             * utilizará Walk como respaldo.
             */
            if (ExisteEstado(estadoCorrer))
            {
                CambiarEstado(estadoCorrer);
            }
            else
            {
                CambiarEstado(estadoCaminar);
            }

            return;
        }

        CambiarEstado(estadoCaminar);
    }

    public void ReproducirAtaque()
    {
        if (string.IsNullOrWhiteSpace(estadoAtaque))
        {
            return;
        }

        if (!ExisteEstado(estadoAtaque))
        {
            return;
        }

        bloqueoAnimacionHasta = Time.time + duracionAtaque;

        ForzarEstado(estadoAtaque);
    }

    public void ReproducirGolpe()
    {
        if (string.IsNullOrWhiteSpace(estadoGolpe))
        {
            return;
        }

        if (!ExisteEstado(estadoGolpe))
        {
            return;
        }

        bloqueoAnimacionHasta = Time.time + duracionGolpe;

        ForzarEstado(estadoGolpe);
    }

    public void ReproducirMuerte()
    {
        if (muerteReproducida)
        {
            return;
        }

        muerteReproducida = true;

        if (string.IsNullOrWhiteSpace(estadoMuerte))
        {
            return;
        }

        if (!ExisteEstado(estadoMuerte))
        {
            return;
        }

        /*
         * La muerte se reproduce una sola vez
         * y no regresa a caminar.
         */
        ForzarEstado(estadoMuerte);
    }

    private bool EstaMuerto()
    {
        if (vida != null && vida.estaMuerto)
        {
            return true;
        }

        if (enemigoV19 != null &&
            enemigoV19.estadoActual ==
            EnemyV19AI.EstadoV19.Muerto)
        {
            return true;
        }

        if (enemigoV37 != null &&
            enemigoV37.estadoActual ==
            EnemyV37AI.EstadoV37.Muerto)
        {
            return true;
        }

        return false;
    }

    private void CambiarEstado(string nombreEstado)
    {
        if (string.IsNullOrWhiteSpace(nombreEstado))
        {
            return;
        }

        if (estadoReproduciendose == nombreEstado)
        {
            return;
        }

        if (!ExisteEstado(nombreEstado))
        {
            return;
        }

        estadoReproduciendose = nombreEstado;

        animator.CrossFadeInFixedTime(
            ObtenerNombreCompleto(nombreEstado),
            duracionTransicion
        );
    }

    private void ForzarEstado(string nombreEstado)
    {
        if (string.IsNullOrWhiteSpace(nombreEstado))
        {
            return;
        }

        estadoReproduciendose = nombreEstado;

        animator.CrossFadeInFixedTime(
            ObtenerNombreCompleto(nombreEstado),
            duracionTransicion
        );
    }

    private bool ExisteEstado(string nombreEstado)
    {
        if (animator == null ||
            string.IsNullOrWhiteSpace(nombreEstado))
        {
            return false;
        }

        int hashCorto = Animator.StringToHash(nombreEstado);
        int hashCompleto = Animator.StringToHash(
            "Base Layer." + nombreEstado
        );

        return animator.HasState(0, hashCorto) ||
               animator.HasState(0, hashCompleto);
    }

    private string ObtenerNombreCompleto(string nombreEstado)
    {
        int hashCompleto = Animator.StringToHash(
            "Base Layer." + nombreEstado
        );

        if (animator.HasState(0, hashCompleto))
        {
            return "Base Layer." + nombreEstado;
        }

        return nombreEstado;
    }
}
