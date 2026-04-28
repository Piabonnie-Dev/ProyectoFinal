using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Mover : MonoBehaviour
{
    [Header("Referencias")]
    public Transform pivoteCamara;

    [Header("Velocidades")]
    public float velocidadCaminar = 4.8f;
    public float velocidadCorrer = 7.2f;
    public float velocidadAgachado = 2.4f;

    [Header("Sensación del movimiento")]
    public float aceleracionSuelo = 22f;
    public float desaceleracionSuelo = 18f;
    public float aceleracionAire = 6f;
    public float controlAereo = 0.35f;

    [Header("Salto y gravedad")]
    public float alturaSalto = 1.15f;
    public float gravedad = -24f;
    public float fuerzaPegadoSuelo = -2f;

    [Header("Agacharse")]
    public bool agacharseMantener = true;
    public float alturaNormal = 1.8f;
    public float alturaAgachado = 1.15f;
    public float velocidadTransicionAgachado = 12f;
    public float alturaCamaraNormal = 1.6f;
    public float alturaCamaraAgachado = 1.0f;

    [Header("Stamina")]
    public bool usarStamina = true;
    public float staminaMax = 5f;
    public float gastoStamina = 1.15f;
    public float recuperacionStamina = 0.9f;
    public float retrasoRecuperacion = 0.6f;

    private CharacterController controller;
    private Vector3 velocidadHorizontalActual;
    private float velocidadVertical;
    private float staminaActual;
    private float temporizadorRecuperacion;
    private bool estaAgachado;

    public bool EstaCorriendo { get; private set; }
    public bool EstaMoviendose { get; private set; }
    public float VelocidadHorizontal01 { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = alturaNormal;
        controller.center = new Vector3(0f, alturaNormal * 0.5f, 0f);
        staminaActual = staminaMax;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
if(UIInventarioLoot.InventarioAbierto)
return;


        if (pivoteCamara == null)
            return;

        ActualizarAgachado();
        ActualizarMovimiento();
        ActualizarCapsula();
    }

    void ActualizarAgachado()
    {
        if (agacharseMantener)
            estaAgachado = Input.GetKey(KeyCode.LeftControl);
        else if (Input.GetKeyDown(KeyCode.LeftControl))
            estaAgachado = !estaAgachado;
    }

    void ActualizarMovimiento()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = pivoteCamara.forward;
        Vector3 right = pivoteCamara.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direccionDeseada = (forward * z + right * x).normalized;
        EstaMoviendose = direccionDeseada.sqrMagnitude > 0.01f;

        bool enSuelo = controller.isGrounded;
        float velocidadObjetivo = estaAgachado ? velocidadAgachado : velocidadCaminar;

        bool intentaCorrer = Input.GetKey(KeyCode.LeftShift) && z > 0f && !estaAgachado && EstaMoviendose;
        bool puedeCorrer = intentaCorrer;

        if (usarStamina)
        {
            if (temporizadorRecuperacion > 0f)
                temporizadorRecuperacion -= Time.deltaTime;

            if (intentaCorrer && staminaActual > 0f && temporizadorRecuperacion <= 0f)
            {
                velocidadObjetivo = velocidadCorrer;
                EstaCorriendo = true;
                staminaActual -= gastoStamina * Time.deltaTime;

                if (staminaActual <= 0f)
                {
                    staminaActual = 0f;
                    EstaCorriendo = false;
                    temporizadorRecuperacion = retrasoRecuperacion;
                }
            }
            else
            {
                EstaCorriendo = false;
                staminaActual += recuperacionStamina * Time.deltaTime;
            }

            staminaActual = Mathf.Clamp(staminaActual, 0f, staminaMax);
            puedeCorrer = EstaCorriendo;
        }
        else
        {
            EstaCorriendo = intentaCorrer;
            if (EstaCorriendo)
                velocidadObjetivo = velocidadCorrer;
        }

        if (puedeCorrer)
            velocidadObjetivo = velocidadCorrer;

        Vector3 velocidadHorizontalDeseada = direccionDeseada * velocidadObjetivo;

        if (enSuelo)
        {
            float aceleracion = EstaMoviendose ? aceleracionSuelo : desaceleracionSuelo;
            velocidadHorizontalActual = Vector3.MoveTowards(
                velocidadHorizontalActual,
                velocidadHorizontalDeseada,
                aceleracion * Time.deltaTime
            );

            if (velocidadVertical < 0f)
                velocidadVertical = fuerzaPegadoSuelo;

            if (Input.GetButtonDown("Jump") && !estaAgachado)
                velocidadVertical = Mathf.Sqrt(alturaSalto * -2f * gravedad);
        }
        else
        {
            velocidadHorizontalActual = Vector3.MoveTowards(
                velocidadHorizontalActual,
                velocidadHorizontalDeseada,
                aceleracionAire * Time.deltaTime
            );

            Vector3 direccionAire = velocidadHorizontalDeseada.normalized;
            if (direccionAire.sqrMagnitude > 0.01f && velocidadHorizontalActual.sqrMagnitude > 0.01f)
            {
                float magnitudActual = velocidadHorizontalActual.magnitude;
                Vector3 mezcla = Vector3.Lerp(
                    velocidadHorizontalActual.normalized,
                    direccionAire,
                    controlAereo * Time.deltaTime
                ).normalized;

                velocidadHorizontalActual = mezcla * magnitudActual;
            }
        }

        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimientoFinal = velocidadHorizontalActual + Vector3.up * velocidadVertical;
        controller.Move(movimientoFinal * Time.deltaTime);

        Vector3 velocidadPlana = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        VelocidadHorizontal01 = Mathf.InverseLerp(0f, velocidadCorrer, velocidadPlana.magnitude);
    }

    void ActualizarCapsula()
    {
        float alturaObjetivo = estaAgachado ? alturaAgachado : alturaNormal;
        controller.height = Mathf.Lerp(controller.height, alturaObjetivo, velocidadTransicionAgachado * Time.deltaTime);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);

        if (pivoteCamara != null)
        {
            Vector3 local = pivoteCamara.localPosition;
            float yObjetivo = estaAgachado ? alturaCamaraAgachado : alturaCamaraNormal;
            local.y = Mathf.Lerp(local.y, yObjetivo, velocidadTransicionAgachado * Time.deltaTime);
            pivoteCamara.localPosition = local;
        }
    }

    public float ObtenerStaminaNormalizada()
    {
        if (!usarStamina || staminaMax <= 0f)
            return 1f;

        return staminaActual / staminaMax;
    }
}