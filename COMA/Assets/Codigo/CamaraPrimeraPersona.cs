using UnityEngine;

public class CamaraPrimeraPersona : MonoBehaviour
{
    public Transform cuerpoJugador;
    public float sensibilidad = 150f;

    private float rotacionX = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, 0f, 120f);

        // La cámara solo mira arriba y abajo
        transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);

        // El cuerpo gira a izquierda y derecha
        cuerpoJugador.Rotate(Vector3.up * mouseX);
    }
}
