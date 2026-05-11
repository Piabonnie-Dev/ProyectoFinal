using System.Collections;
using TMPro;
using UnityEngine;

public class TrapThrower : MonoBehaviour
{
   public enum TipoTrampa
    {
        Explosiva,
        Stun
    }

    [Header("Referencias")]
    public Camera camaraJugador;
    public Transform puntoLanzamiento;
    public Transform trapVisual;
    public TMP_Text textoTrampaActual;

    [Header("Prefabs")]
    public GameObject prefabTrampaExplosiva;
    public GameObject prefabTrampaStun;

    [Header("Cantidad")]
    public int cantidadExplosiva = 3;
    public int cantidadStun = 3;

    [Header ("Lanzamiento")]
    public float fuerzaLanzamiento =10f;
    public float fuerzaHaciaArriba = 2.2f;

    [Header("Animacion")]
    public float duracionAnimacion = 0.18f;
    public Vector3 posicionReposo = new Vector3(0f, 0f, 0f);
    public Vector3 posicionPreparacion= new Vector3(-0.08f, -0.04f, -0.08f);
    [Header("Entrada")]
public KeyCode teclaExplosiva = KeyCode.Alpha4;
public KeyCode teclaStun = KeyCode.Alpha5;
public KeyCode teclaLanzar = KeyCode.G;

private TipoTrampa trampaActual = TipoTrampa.Explosiva;
private bool lanzando = false;

void Start()
    {
        if(camaraJugador == null)
        camaraJugador =GetComponentInParent<Camera>();
        ActualizarUI();
    }

void Update()
    {
        if(ControlPartida.PartidaTerminada)
        return;

        if(UIInventarioLoot.InventarioAbierto)
        return;

        if(SuitUpgradeUI.PanelAbierto)
        return;
        if(Input.GetKeyDown(teclaExplosiva))
        {
            trampaActual = TipoTrampa.Explosiva;
            ActualizarUI();
        }
       if (Input.GetKeyDown(teclaStun))
        {
            trampaActual = TipoTrampa.Stun;
            ActualizarUI();
        }

        if (Input.GetKeyDown(teclaLanzar))
        {
            IntentarLanzar();
        }
    }
    void IntentarLanzar()
    {
        if(lanzando)
        return;

        if(trampaActual == TipoTrampa.Explosiva && cantidadExplosiva <= 0)
        return;

        if(trampaActual == TipoTrampa.Stun && cantidadStun <= 0 )
        return;
        StartCoroutine(AnimarYLanzar());
    }
    IEnumerator AnimarYLanzar()
    {
        lanzando = true;
        Vector3 inicio = posicionReposo;
        Vector3 fin = posicionPreparacion;

        float t = 0f;
        while (t < duracionAnimacion)
        {
            t += Time.deltaTime;
            float p = t / duracionAnimacion;

            if (trapVisual != null)
                trapVisual.localPosition = Vector3.Lerp(inicio, fin, p);

            yield return null;
    }
    LanzarTrampaReal();
    t = 0f;
    while(t < duracionAnimacion)
        {
            t += Time.deltaTime;
            float p = t / duracionAnimacion;

            if(trapVisual != null)
            trapVisual.localPosition = Vector3.Lerp(fin, inicio, p);

            yield return null;
        }
     if(trapVisual != null)
     trapVisual.localPosition = posicionReposo;

     lanzando = false;
     ActualizarUI();

}

void LanzarTrampaReal()
    {
        
        if(camaraJugador == null || puntoLanzamiento == null)
        return;

        GameObject prefab = null;

        if(trampaActual == TipoTrampa.Explosiva)
        {
            prefab = prefabTrampaExplosiva;
            cantidadExplosiva--;
        }
        else
        {
            prefab = prefabTrampaStun;
            cantidadStun--;
        }

        if (prefab == null)
            return;

        GameObject nuevaTrampa = Instantiate(prefab, puntoLanzamiento.position, Quaternion.identity);

        Rigidbody rb = nuevaTrampa.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direccion = camaraJugador.transform.forward * fuerzaLanzamiento;
            direccion += camaraJugador.transform.up * fuerzaHaciaArriba;
            rb.AddForce(direccion, ForceMode.Impulse);
        }
    }
void ActualizarUI()
    {
        if (textoTrampaActual == null)
            return;

        string nombre = trampaActual == TipoTrampa.Explosiva ? "Explosiva" : "Stun";
        int cantidad = trampaActual == TipoTrampa.Explosiva ? cantidadExplosiva : cantidadStun;

        textoTrampaActual.text = "Trampa: " + nombre + " (" + cantidad + ")";
    }

}
