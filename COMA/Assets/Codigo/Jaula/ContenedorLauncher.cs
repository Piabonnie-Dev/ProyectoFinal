
using UnityEngine;

public class ContenedorLauncher : MonoBehaviour
{
   [Header("Referencias")]
   public Camera camaraJugador;
   public Transform puntoLanzamiento;
   public GameObject prefabTrampaContencion;

   [Header("Lanzamiento")]
   public KeyCode teclaLanzar = KeyCode.E;
   public float fuerzaLanzamiento = 12f;
   public float fuerzaHaciaArriba = 2.5f;

   [Header("Cantidad")]
   public int cantidadContenedores = 2;

   private void Start()
   {
      if(camaraJugador == null)
      camaraJugador = Camera.main;
   }

   private void Update()
   {
      // Si el juego está pausado, no permitimos acciones.
if (EstadoJuego.JuegoPausado)
    return;
      if(Input.GetKeyDown(teclaLanzar))
      {
LanzarContenedor();
      }

   }
private void LanzarContenedor()
   {
      
      if(cantidadContenedores <= 0)
      {
         Debug.Log("No quedan contenedores");
         return;

      }
//Validamos prefab
      if(prefabTrampaContencion == null)
      {
         Debug.LogWarning("Falta asignar prefabTrampaContencion");
         return;
      }

      if(puntoLanzamiento == null)
      {
         Debug.LogWarning("Falta asignar puntoLanzamiento");
         return;

      }

      if(camaraJugador == null)
      {
         Debug.LogWarning("Falta camara del jugador");
         return;

      }
//Restamos un contenedor
      cantidadContenedores--;


      GameObject nuevaTrampa = Instantiate(
         prefabTrampaContencion, 
         puntoLanzamiento.position,
         Quaternion.identity
      );

   Rigidbody rb = nuevaTrampa.GetComponent<Rigidbody>();


   if(rb != null)
      {
         
         Vector3 direccion = camaraJugador.transform.forward * fuerzaLanzamiento;
         direccion += camaraJugador.transform.up * fuerzaHaciaArriba;

         rb.AddForce(direccion, ForceMode.Impulse);
      }

      Debug.Log("Contenedor lanzado. Restantes:" + cantidadContenedores);


   }
}
