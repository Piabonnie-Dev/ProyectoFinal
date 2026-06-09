using UnityEngine;

public class CamaraPrimeraPersona : MonoBehaviour
{
    public Transform cuerpoJugador;
    public Camera camara;
    public Mover mover;

    [Header("Mouse")]
    public float sensibilidadX = 180f;
    public float sensibilidadY = 180f;
    public float limiteArriba = 89f;
    public float limiteAbajo = -89f;

    [Header("Visual")]
    public float fovNormal = 72f;
    public float fovCorriendo = 79f;
    public float suavizadoFov = 8f;

    private float rotacionX;

    void Start()
    {
        // Si no asignaste cámara, buscamos una en hijos.
        if (camara == null)
            camara = GetComponentInChildren<Camera>();

        // Cargamos la sensibilidad guardada desde opciones.
        float sensibilidadGuardada = PlayerPrefs.GetFloat("SensibilidadMouse", 180f);
        sensibilidadX = sensibilidadGuardada;
        sensibilidadY = sensibilidadGuardada;

        // Bloqueamos cursor al iniciar el mapa.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Si la partida terminó, no movemos cámara.
        if (ControlPartida.PartidaTerminada)
            return;

        // Si el juego está pausado, no movemos cámara.
        if (Time.timeScale == 0f)
            return;

        // Si el inventario está abierto, no movemos cámara.
        if (UIInventarioLoot.InventarioAbierto)
            return;

        ActualizarMouseLook();
        ActualizarFov();
    }

    void ActualizarMouseLook()
    {
        // Leemos movimiento del mouse usando sensibilidad.
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;

        // Rotación vertical de cámara.
        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, limiteAbajo, limiteArriba);

        // Aplicamos rotación vertical al pivote/cámara.
        transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);

        // Aplicamos rotación horizontal al cuerpo del jugador.
        if (cuerpoJugador != null)
            cuerpoJugador.Rotate(Vector3.up * mouseX);
    }

    void ActualizarFov()
    {
        // Si no hay cámara, salimos.
        if (camara == null)
            return;

        // FOV normal.
        float objetivo = fovNormal;

        // Si está corriendo, usamos FOV de carrera.
        if (mover != null && mover.EstaCorriendo)
            objetivo = fovCorriendo;

        // Suavizamos cambio de FOV.
        camara.fieldOfView = Mathf.Lerp(camara.fieldOfView, objetivo, suavizadoFov * Time.deltaTime);
    }
}
