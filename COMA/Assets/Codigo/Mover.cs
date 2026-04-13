using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Mover : MonoBehaviour
{
    public float velocidad = 5f;
    public float velocidadCorrer = 8f;
    public float gravedad = -20f;
    public float fuerzaSalto = 1.5f;

    public Transform orientacionCamara;

    private CharacterController controller;
    private Vector3 velocidadVertical;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = orientacionCamara.forward;
        Vector3 right = orientacionCamara.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movimiento = (forward * z + right * x).normalized;

        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidad;

        controller.Move(movimiento * velocidadActual * Time.deltaTime);

        if (controller.isGrounded && velocidadVertical.y < 0f)
        {
            velocidadVertical.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocidadVertical.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
        }

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }
}