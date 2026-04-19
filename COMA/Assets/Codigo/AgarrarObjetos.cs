

using UnityEngine;

public class AgarrarObjetos : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraJugador;
    public Transform puntoAgarre;

    [Header("Entrada")]
    public KeyCode teclaAgarrar = KeyCode.E;
    public KeyCode teclaLanzar = KeyCode.Q;

    [Header("Deteccion")]
    public float distanciaMaxima = 4f;
    public LayerMask capasAgarrables = ~0;

    [Header("Movimiento del objeto")]
    public float fuerzaSeguimiento = 18f;
    public float velocidadMaxima = 20f;
    public float distanciaSoltarSiSeAleja = 5f;

    [Header("Lanzamiento")]
    public float fuerzaLanzamiento = 100f;
    public float torqueLanzamiento = 2f;

    private ObjetoAgarrable objetoActual;
    private Rigidbody rbActual;

    private bool teniaGravedad;
    private float dragOriginal;
    private float angularDragOriginal;
    private CollisionDetectionMode collisionOriginal;
    private RigidbodyInterpolation interpolationOriginal;

    void Update()
    {
        if (camaraJugador != null)
        {
            Debug.DrawRay(camaraJugador.transform.position, camaraJugador.transform.forward * distanciaMaxima, Color.red);
        }

        if (Input.GetKeyDown(teclaAgarrar))
        {
            if (objetoActual == null)
                IntentarAgarrar();
            else
                SoltarObjeto();
        }

        if (objetoActual != null && Input.GetKeyDown(teclaLanzar))
        {
            LanzarObjeto();
        }

        if (objetoActual != null)
        {
            float distancia = Vector3.Distance(transform.position, objetoActual.transform.position);
            if (distancia > distanciaSoltarSiSeAleja)
                SoltarObjeto();
        }
    }

    void FixedUpdate()
    {
        if (objetoActual != null)
        {
            MoverObjetoAgarrado();
        }
    }

    void IntentarAgarrar()
    {
        if (camaraJugador == null || puntoAgarre == null)
        {
            Debug.LogWarning("Falta asignar camaraJugador o puntoAgarre.");
            return;
        }

        Ray rayo = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

        if (Physics.Raycast(rayo, out RaycastHit hit, distanciaMaxima, capasAgarrables))
        {
            Debug.Log("Raycast golpeó a: " + hit.collider.name);

            ObjetoAgarrable agarrable = hit.collider.GetComponent<ObjetoAgarrable>();

            if (agarrable == null)
                agarrable = hit.collider.GetComponentInParent<ObjetoAgarrable>();

            if (agarrable == null)
                agarrable = hit.collider.GetComponentInChildren<ObjetoAgarrable>();

            if (agarrable != null && agarrable.SePuedeAgarrar())
            {
                TomarObjeto(agarrable);
                Debug.Log("Objeto agarrado: " + agarrable.name);
            }
            else
            {
                Debug.Log("El objeto golpeado no tiene ObjetoAgarrable o no se puede agarrar.");
            }
        }
        else
        {
            Debug.Log("El raycast no golpeó ningún objeto agarrable.");
        }
    }

    void TomarObjeto(ObjetoAgarrable agarrable)
    {
        objetoActual = agarrable;
        rbActual = agarrable.rb;

        teniaGravedad = rbActual.useGravity;
        dragOriginal = rbActual.drag;
        angularDragOriginal = rbActual.angularDrag;
        collisionOriginal = rbActual.collisionDetectionMode;
        interpolationOriginal = rbActual.interpolation;

        rbActual.useGravity = false;
        rbActual.drag = 10f;
        rbActual.angularDrag = 10f;
        rbActual.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rbActual.interpolation = RigidbodyInterpolation.Interpolate;

        rbActual.velocity = Vector3.zero;
        rbActual.angularVelocity = Vector3.zero;
    }

    void MoverObjetoAgarrado()
    {
        if (rbActual == null || puntoAgarre == null || camaraJugador == null)
            return;

        Vector3 direccion = puntoAgarre.position - rbActual.position;
        Vector3 velocidadDeseada = direccion * fuerzaSeguimiento;

        if (velocidadDeseada.magnitude > velocidadMaxima)
            velocidadDeseada = velocidadDeseada.normalized * velocidadMaxima;

        rbActual.velocity = velocidadDeseada;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(camaraJugador.transform.forward, Vector3.up);
        Quaternion diferencia = rotacionObjetivo * Quaternion.Inverse(rbActual.rotation);

        diferencia.ToAngleAxis(out float angulo, out Vector3 eje);

        if (angulo > 180f)
            angulo -= 360f;

        if (Mathf.Abs(angulo) > 1f && eje != Vector3.zero)
        {
            Vector3 velocidadAngular = eje * angulo * Mathf.Deg2Rad * 8f;
            rbActual.angularVelocity = velocidadAngular;
        }
    }

    void SoltarObjeto()
    {
        if (rbActual != null)
        {
            rbActual.useGravity = teniaGravedad;
            rbActual.drag = dragOriginal;
            rbActual.angularDrag = angularDragOriginal;
            rbActual.collisionDetectionMode = collisionOriginal;
            rbActual.interpolation = interpolationOriginal;
        }

        objetoActual = null;
        rbActual = null;
    }

    void LanzarObjeto()
    {
        if (rbActual == null || camaraJugador == null)
            return;

        Rigidbody rbLanzado = rbActual;

        SoltarObjeto();

        rbLanzado.AddForce(camaraJugador.transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
        rbLanzado.AddTorque(Random.onUnitSphere * torqueLanzamiento, ForceMode.Impulse);
    }
}