using TMPro;
using UnityEngine;

public class LootScanUI : MonoBehaviour
{
    [Header("Referencias al loot")]
    public LootItem lootItem;

    [Header("Textos de UI")]
    public TMP_Text textoNombre;
    public TMP_Text textoValor;
    public TMP_Text textoPeso;

    [Header("Billboarding")]
    public Camera camaraJugador;
    public bool mirarSiempreACamara = true;
    [Header("Ocultamiento por paredes")]
    public Transform puntoRevision;
    public LayerMask capasQueBloqueanUI;

    [Header("Comportamiento")]
    public float tiempoVisibleDespuesSolicitud = 0.15f;
    public float distanciaMaximaVisible = 12f;

    [Header("Estado")]
    public CanvasGroup canvasGroup;
    private float ultimoTiempoSolicitado = -999f;

    private void Awake()
    {
        if(lootItem == null)
        {
            lootItem = GetComponentInParent<LootItem>();

        }
        if(puntoRevision == null && lootItem != null)
        {
            puntoRevision = lootItem.transform;
        }

        if(canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        //Si no existe CanvasGroup, lo agregamos en automatico
        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        OcultarInstantaneo();
        ActualizarDatos();
    }

    private void Update()
    {
        if(camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }
        ActualizarVisibilidad();
    }
    private void LateUpdate()
    {
        if (mirarSiempreACamara)
        {
            MirarACamara();
        }


    }

    public void SolicitarMostrar(Camera camara)
    {
        if(camara != null)
        {
            camaraJugador = camara;
        }
        ultimoTiempoSolicitado = Time.time;
        ActualizarDatos();
    }

    private void ActualizarDatos()
    {
        if(lootItem == null)
        {
            return;
        }
        if(textoNombre != null)
        {
            textoNombre.text = lootItem.nombreLoot;
        }
        if(textoValor != null)
        {
            textoValor.text = "Valor: $" + lootItem.valor;
        }

        if(textoPeso != null)
        {
            textoPeso.text = "Peso: " + lootItem.peso.ToString("0.0") + " kg";

        }

    }
    private void ActualizarVisibilidad()
    {
        

        bool solicitado = Time.time <= ultimoTiempoSolicitado + tiempoVisibleDespuesSolicitud;
        if (!solicitado)
        {
            Ocultar();
            return;
        }

        if(camaraJugador == null || puntoRevision == null)
        {
            Ocultar();
            return;
        }

        float distancia = Vector3.Distance(camaraJugador.transform.position, puntoRevision.position);


        if(distancia> distanciaMaximaVisible)
        {
            Ocultar();
            return;
        }
        Vector3 direccionALoot = puntoRevision.position - camaraJugador.transform.position;

        if(Vector3.Dot(camaraJugador.transform.forward, direccionALoot.normalized)<= 0f)
        {
            Ocultar();
            return;
        }

        if(HayObstaculoEntreCamaraYLoot())
        {
            Ocultar();
            return;
        }

        Mostrar();

    
    }

    private bool HayObstaculoEntreCamaraYLoot()
    {
        // Si no hay cámara o punto, por seguridad decimos que hay obstáculo.
        if (camaraJugador == null || puntoRevision == null)
        {
            return true;
        }

        // Origen del raycast.
        Vector3 origen = camaraJugador.transform.position;

        // Destino del raycast.
        Vector3 destino = puntoRevision.position;

        // Dirección hacia el loot.
        Vector3 direccion = destino - origen;

        // Distancia al loot.
        float distancia = direccion.magnitude;

        // Si la distancia es casi cero, no hay obstáculo.
        if (distancia <= 0.01f)
        {
            return false;
        }

        // Revisamos si una pared/estructura bloquea la UI.
        if (Physics.Raycast(
            origen,
            direccion.normalized,
            out RaycastHit hit,
            distancia,
            capasQueBloqueanUI,
            QueryTriggerInteraction.Ignore))
        {
            // Si el raycast pegó con algo que NO pertenece al mismo loot, está bloqueado.
            LootItem lootDelImpacto = hit.collider.GetComponentInParent<LootItem>();

            if (lootDelImpacto != lootItem)
            {
                return true;
            }
        }

        return false;
    }

    private void Mostrar()
    {
        // Mostramos visualmente.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Ocultar()
    {
        // Ocultamos visualmente.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OcultarInstantaneo()
    {
        // Ocultamos al iniciar.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void MirarACamara()
    {
        // Si no hay cámara, no hacemos nada.
        if (camaraJugador == null)
        {
            return;
        }

        // Técnica de billboarding.
        // Hace que el canvas siempre mire plano hacia el jugador.
        transform.rotation = Quaternion.LookRotation(
            transform.position - camaraJugador.transform.position
        );
    }
}
