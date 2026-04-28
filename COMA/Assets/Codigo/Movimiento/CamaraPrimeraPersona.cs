using UnityEngine;

public class CamaraPrimeraPersona : MonoBehaviour
{
    public Transform cuerpoJugador;
  public Camera camara;
  public Mover mover;

  [Header ("Mouse")]
  public float sensibilidadX = 180f;
  public float sensibilidadY = 180f;
  public float limiteArriba = 89f;
    public float limiteAbajo = -89f;

    [Header ("Visual")]
    public float fovNormal = 72f;
    public float fovCorriendo = 79f;
    public float suavizadoFov = 8f;
    private float rotacionX;

    void Start()
    {
        if (camara == null)
        camara = GetComponentInChildren<Camera>();

       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false; 

    }

void Update()
    {
         if (UIInventarioLoot.InventarioAbierto)
        return;
        
ActualizarMouseLook();
ActualizarFov();


    }

   void ActualizarMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X")* sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;


        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -80f, 80f);
transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
if(cuerpoJugador != null)
cuerpoJugador.Rotate(Vector3.up * mouseX);

    } 
    void ActualizarFov()
    {
        if (camara == null)
        return;

        float objetivo = fovNormal; 
        if(mover != null && mover.EstaCorriendo)
objetivo = fovCorriendo;
camara.fieldOfView = Mathf.Lerp(camara.fieldOfView, objetivo , suavizadoFov * Time.deltaTime);
    }


}
