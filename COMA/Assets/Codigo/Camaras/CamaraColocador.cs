using UnityEngine;

public class CamaraColocador : MonoBehaviour
{
    [Header("Referencias principales")]
    public Camera camaraJugador;
    public CamaraManager camaraManager;

    [Header("Prefab de la cámara")]
    public GameObject prefabCamaraColocable;

    [Header("Configuración de colocación")]
    public float distanciaMaximaColocacion = 5f;
    public LayerMask capasPermitidas;

    [Header("Controles")]
    public KeyCode teclaSeleccionarCamara = KeyCode.Alpha6;

    [Header("Inventario de cámaras")]
    public int camarasDisponibles = 3;

    [Header("Estado actual")]
    public bool camaraSeleccionada = false;

    private void Start()
    {
        // Si no asignaste la cámara del jugador desde el Inspector, usamos la cámara principal.
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Si no asignaste el CamaraManager, lo buscamos en el mismo Player.
        if (camaraManager == null)
        {
            camaraManager = GetComponent<CamaraManager>();
        }
    }

    private void Update()
    {
        // Presionar 6 ahora SOLO selecciona la herramienta de cámara.
        // Ya no funciona como interruptor, así que no puede decir "cámara guardada".
        if (Input.GetKeyDown(teclaSeleccionarCamara))
        {
            SeleccionarCamara();
        }

        // Si el jugador cambia a arma o trampa, deseleccionamos la cámara.
        // Esto evita que el click izquierdo siga colocando cámaras cuando quieres disparar.
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Alpha5))
        {
            DeseleccionarCamara();
        }

        // Si la cámara está seleccionada, cada click izquierdo intenta colocar una cámara.
        if (camaraSeleccionada && Input.GetMouseButtonDown(0))
        {
            IntentarColocarCamara();
        }
    }

    private void SeleccionarCamara()
    {
        // Dejamos la cámara seleccionada directamente.
        // No usamos toggle para evitar confusión.
        camaraSeleccionada = true;

        // Feedback claro en consola.
        Debug.Log("Herramienta de cámara seleccionada. Click izquierdo para colocar cámaras.");
    }

    private void DeseleccionarCamara()
    {
        // Quitamos la selección de cámara cuando el jugador cambia de herramienta.
        camaraSeleccionada = false;
    }

    private void IntentarColocarCamara()
    {
        // Si ya no hay cámaras disponibles, avisamos y no hacemos nada.
        if (camarasDisponibles <= 0)
        {
            Debug.Log("No tienes cámaras disponibles.");
            return;
        }

        // Creamos un rayo desde el centro de la cámara del jugador hacia adelante.
        Ray rayo = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

        RaycastHit hit;

        // Revisamos si el rayo toca una superficie válida para colocar la cámara.
        if (Physics.Raycast(rayo, out hit, distanciaMaximaColocacion, capasPermitidas))
        {
            ColocarCamaraEnSuperficie(hit);
        }
        else
        {
            Debug.Log("No hay superficie válida para colocar la cámara.");
        }
    }

    private void ColocarCamaraEnSuperficie(RaycastHit hit)
    {
        // Colocamos la cámara un poquito separada de la pared o piso.
        // Esto evita que se meta dentro de la superficie.
        Vector3 posicionCamara = hit.point + hit.normal * 0.05f;

        // Calculamos la rotación correcta según si apuntaste al piso o a una pared.
        Quaternion rotacionCamara = CalcularRotacionCamara(hit);

        // Creamos la cámara colocable en el mundo.
        GameObject nuevaCamaraObjeto = Instantiate(prefabCamaraColocable, posicionCamara, rotacionCamara);

        // Buscamos el script CamaraColocable en el prefab recién creado.
        CamaraColocable nuevaCamara = nuevaCamaraObjeto.GetComponent<CamaraColocable>();

        // Registramos la cámara en el manager para poder verla desde el monitor.
        if (nuevaCamara != null && camaraManager != null)
        {
            camaraManager.RegistrarCamara(nuevaCamara);
        }

        // Restamos una cámara disponible.
        camarasDisponibles--;

        // Feedback claro.
        Debug.Log("Cámara colocada. Cámaras restantes: " + camarasDisponibles);
    }

    private Quaternion CalcularRotacionCamara(RaycastHit hit)
    {
        // Detectamos si la superficie es piso usando la normal del impacto.
        bool esPiso = Vector3.Dot(hit.normal, Vector3.up) > 0.7f;

        if (esPiso)
        {
            // En piso, la cámara mira hacia donde mira el jugador, pero sin inclinarse hacia arriba o abajo.
            Vector3 direccionHorizontal = Vector3.ProjectOnPlane(camaraJugador.transform.forward, Vector3.up).normalized;

            // Si por alguna razón la dirección queda vacía, usamos el frente del Player.
            if (direccionHorizontal == Vector3.zero)
            {
                direccionHorizontal = transform.forward;
            }

            // La cámara queda parada sobre el piso mirando hacia adelante.
            return Quaternion.LookRotation(direccionHorizontal, Vector3.up);
        }
        else
        {
            // En pared, la cámara mira hacia afuera de la pared.
            return Quaternion.LookRotation(hit.normal, Vector3.up);
        }
    }
}