using UnityEngine;

public class PlayerJaulaInteractor : MonoBehaviour
{
    [Header("Referencias")]

    // Cámara desde donde se hace el raycast.
    public Camera camaraJugador;

    // Punto donde se sostiene la jaula ligera.
    public Transform puntoSujecion;

    [Header("Interacción")]

    // Tecla para agarrar, soltar o empujar.
    public KeyCode teclaInteraccion = KeyCode.E;

    // Distancia máxima para interactuar.
    public float distanciaInteraccion = 3f;

    [Header("Agarre de jaula ligera")]

    // Fuerza con la que la jaula intenta ir al punto de sujeción.
    public float fuerzaAgarre = 80f;

    // Amortiguación para que no tiemble tanto.
    public float amortiguacion = 12f;

    [Header("Empuje de jaula pesada")]

    // Fuerza aplicada a jaulas pesadas.
    public float fuerzaEmpuje = 260f;

    // Jaula que se está cargando.
    private JaulaContencion jaulaActual;

    // Rigidbody de la jaula actual.
    private Rigidbody rbActual;

    // Indica si el jugador está cargando una jaula.
    private bool cargandoJaula = false;

    private void Start()
    {
        // Si no hay cámara asignada, usamos Camera.main.
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        // Si no hay punto de sujeción, creamos uno enfrente de la cámara.
        if (puntoSujecion == null && camaraJugador != null)
        {
            GameObject punto = new GameObject("PuntoSujecionJaula");
            punto.transform.SetParent(camaraJugador.transform);
            punto.transform.localPosition = new Vector3(0f, -0.25f, 2.2f);
            punto.transform.localRotation = Quaternion.identity;

            puntoSujecion = punto.transform;
        }
    }

    private void Update()
    {
        // Si el juego está pausado, no permitimos acciones.
if (EstadoJuego.JuegoPausado)
    return;
        // Si estamos cargando una jaula, actualizamos su movimiento.
        if (cargandoJaula)
        {
            ActualizarAgarre();

            // Si presiona E otra vez, soltamos.
            if (Input.GetKeyDown(teclaInteraccion))
            {
                SoltarJaula();
            }

            return;
        }

        // Si presiona E, intenta agarrar una jaula ligera.
        if (Input.GetKeyDown(teclaInteraccion))
        {
            IntentarAgarrarJaula();
        }

        // Si mantiene E sobre jaula pesada, la empuja.
        if (Input.GetKey(teclaInteraccion))
        {
            IntentarEmpujarJaulaPesada();
        }
    }

    private void IntentarAgarrarJaula()
    {
        // Buscamos una jaula al frente.
        JaulaContencion jaula = BuscarJaulaAlFrente(out RaycastHit hit);

        // Si no hay jaula, salimos.
        if (jaula == null)
            return;

        // Si la jaula no puede moverse, salimos.
        if (!jaula.puedeMoverse)
            return;

        // Si no es ligera, no la agarramos; se empuja.
        if (!jaula.EsLigera())
        {
            Debug.Log("La jaula es pesada. Empújala manteniendo E.");
            return;
        }

        // Buscamos Rigidbody.
        Rigidbody rb = jaula.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("La jaula no tiene Rigidbody.");
            return;
        }

        // Guardamos referencias.
        jaulaActual = jaula;
        rbActual = rb;
        cargandoJaula = true;

        // Ajustamos física mientras está cargada.
        rbActual.useGravity = false;
        rbActual.drag = 8f;
        rbActual.angularDrag = 8f;

        Debug.Log("Jaula agarrada.");
    }

    private void ActualizarAgarre()
    {
        // Si falta algo, soltamos.
        if (jaulaActual == null || rbActual == null || puntoSujecion == null)
        {
            SoltarJaula();
            return;
        }

        // Calculamos dirección hacia el punto de sujeción.
        Vector3 direccion = puntoSujecion.position - rbActual.position;

        // Velocidad objetivo para mover la jaula.
        Vector3 velocidadObjetivo = direccion * fuerzaAgarre;

        // Aplicamos velocidad suavizada.
        rbActual.velocity = Vector3.Lerp(
            rbActual.velocity,
            velocidadObjetivo,
            Time.deltaTime * amortiguacion
        );

        // Reducimos rotación rara.
        rbActual.angularVelocity = Vector3.Lerp(
            rbActual.angularVelocity,
            Vector3.zero,
            Time.deltaTime * amortiguacion
        );
    }

    private void SoltarJaula()
    {
        // Restauramos física.
        if (rbActual != null)
        {
            rbActual.useGravity = true;
            rbActual.drag = 1f;
            rbActual.angularDrag = 1f;
        }

        jaulaActual = null;
        rbActual = null;
        cargandoJaula = false;

        Debug.Log("Jaula soltada.");
    }

    private void IntentarEmpujarJaulaPesada()
    {
        // Buscamos jaula al frente.
        JaulaContencion jaula = BuscarJaulaAlFrente(out RaycastHit hit);

        // Si no hay jaula, salimos.
        if (jaula == null)
            return;

        // Si la jaula es ligera, no la empujamos con esta mecánica.
        if (jaula.EsLigera())
            return;

        // Buscamos Rigidbody.
        Rigidbody rb = jaula.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        // Dirección de empuje desde la cámara.
        Vector3 direccion = camaraJugador.transform.forward;

        // Quitamos inclinación vertical para empujar horizontalmente.
        direccion.y = 0f;
        direccion.Normalize();

        // Aplicamos fuerza en el punto donde mira el jugador.
        rb.AddForceAtPosition(
            direccion * fuerzaEmpuje,
            hit.point,
            ForceMode.Force
        );
    }

    private JaulaContencion BuscarJaulaAlFrente(out RaycastHit hit)
    {
        hit = new RaycastHit();

        // Si no hay cámara, no podemos buscar.
        if (camaraJugador == null)
            return null;

        // Creamos raycast desde el centro de la cámara.
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

        // Lanzamos raycast.
        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            // Buscamos JaulaContencion en el objeto o sus padres.
            JaulaContencion jaula = hit.collider.GetComponentInParent<JaulaContencion>();

            return jaula;
        }

        return null;
    }
}
