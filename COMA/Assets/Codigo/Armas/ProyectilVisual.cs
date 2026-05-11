
using UnityEngine;

public class ProyectilVisual : MonoBehaviour
{

    [Header("Movimiento visual")]
    //Velocidad con la que el proyectil visual viaja hacia su destino
    public float velocidad = 90f;
//Tiempo maximo de vida por si no llega al objetivo
    public float tiempoVida = 1f;
//Distancia minima para considerar que ya llego.
    public float distanciaParaDestruir = 0.08f;
//Punto al que debe viajar el proyectil visual
    private Vector3 destino;

    // Indica si el proyectil ya recibio un destino valido
    private bool iniciado = false;

    public void Iniciar(Vector3 nuevoDestino)
    {
        //Guardamos el punto final del disparo.
    
        destino = nuevoDestino;
//Marcamos el proyectil como iniciado
        iniciado = true;
//Destruimos el proyectil despues de cierto tiempo.
        Destroy(gameObject, tiempoVida);
    }


    
   
    void Update()
    {
        //Si todavia no tiene destino, no se mueve
        if(!iniciado)
        return;
//Movemos el proyectil hacia el destino.

        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);
//Si ya llego suficientemente cerca, se destruye.
        if(Vector3.Distance(transform.position, destino) <= distanciaParaDestruir)
        {
            Destroy(gameObject);
        }
    }
}
