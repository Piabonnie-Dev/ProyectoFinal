using TMPro;
using UnityEngine;

public class ZonaExtraccion : MonoBehaviour
{
    [Header("Referencias")]
    public InventarioLoot inventario;
    public ProgresoJugador progreso;
    public TMP_Text textoPrompt;
    public TMP_Text textoResultado;
    public Transform puntoReinicioOpcional;

    [Header("Entrada")]
    public KeyCode teclaExtraer = KeyCode.X;

    private bool jugadorDentro = false;
    private GameObject jugadorActual;


    void Start()
    {
        
        if(textoPrompt != null)
        textoPrompt.gameObject.SetActive(false);

        if(textoResultado != null)
        textoResultado.gameObject.SetActive(false);
    }
    void Update()
    {
if (ControlPartida.PartidaTerminada)
    return;

        if(!jugadorDentro)
        return;

        if(Input.GetKeyDown(teclaExtraer))
        {
            IntentarExtraer();
        }
    }

    void OnTriggerEnter(Collider ohter)
    {
        if(!ohter.CompareTag("Player"))
        return;

     jugadorDentro = true;
     jugadorActual = ohter.gameObject;

     if(textoPrompt != null)
        {
            textoPrompt.text = "Presiona X para EXTRAER";
            textoPrompt.gameObject.SetActive(true);
        }   
    }
    void OnTriggerExit(Collider ohter)
    {
        if(!ohter.CompareTag("Player"))
        return;

        jugadorDentro = false;
        jugadorActual = null;

        if(textoPrompt != null)
        textoPrompt.gameObject.SetActive(false);
    }

   void IntentarExtraer()
    {
        if(inventario == null || progreso == null)
        {
            Debug.LogWarning("Falta asignar inventarioo progreso en ZonaExtraccion.");
            return;
        }
        if(inventario.lootRecolectado.Count <= 0 || inventario.valorTotal <= 0)
        {
            MostrarResultado("No tienes nada que extraer.");
            Debug.Log("No hay loot en el inventario.");
            return;
        }
        int valorExtraido = inventario.valorTotal;
        int cantidadObjetos = inventario.lootRecolectado.Count;

        progreso.AgregarDinero(valorExtraido);
        inventario.VaciarInventario();

        MostrarResultado("Extraccion exitosa!\n+" + valorExtraido + "$ | Objetos:" + cantidadObjetos);
        Debug.Log("Extraccion exitosa! Ganaste $" + valorExtraido);

        if(puntoReinicioOpcional != null && jugadorActual != null)
        {
            CharacterController controller = jugadorActual.GetComponent<CharacterController>();
if (controller != null)
controller.enabled = false;

jugadorActual.transform.position = puntoReinicioOpcional.position;

if(controller != null)
controller.enabled = true;

        }   
    } 

    void MostrarResultado(string mensaje)
    {
        if(textoResultado != null)
        {
            textoResultado.text = mensaje;
            textoResultado.gameObject.SetActive(true);
            CancelInvoke(nameof(OcultarResultado));
            Invoke(nameof(OcultarResultado), 2.5f);

        }
    }
    void OcultarResultado()
    {
        if(textoResultado != null)
        textoResultado.gameObject.SetActive(false);
    }
}
