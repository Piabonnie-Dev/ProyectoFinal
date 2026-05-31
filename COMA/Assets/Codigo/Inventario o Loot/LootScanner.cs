
using System.Collections.Generic;
using UnityEngine;

public class LootScanner : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraJugador;

    [Header("Entrada")]
    public KeyCode teclaEscaneo = KeyCode.Q;


    [Header("Escaneo al mirar")]
    public float distanciaMirada = 5f;


    [Header("Escaneo amplio")]
public float radioEscaneo = 10f;
public float anguloEscaneo = 70f;

[Header("Capas")]
public LayerMask capasLoot = ~0;
public LayerMask capasQueBloqueanVision;

[Header("Debug")]
public bool mostrarRayosDebug = true;
private readonly List<LootScanUI> lootMostradosEsteFrame = new List<LootScanUI>();
private void Start()
    {
        if(camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

    }

    private void Update()
    {
        if (ControlPartida.PartidaTerminada)
        {
            return;

        }

         // Si el inventario está abierto, no escaneamos.
        if (UIInventarioLoot.InventarioAbierto)
        {
            return;
        }

        // Limpiamos lista temporal.
        lootMostradosEsteFrame.Clear();

        // Escaneo normal: mirar directamente un loot.
        EscanearLootMirado();

        // Escaneo amplio: mantener Q.
        if (Input.GetKey(teclaEscaneo))
        {
            EscanearLootCercano();
        }
    }

    private void EscanearLootMirado()
    {
        // Si no hay cámara, no podemos escanear.
        if (camaraJugador == null)
        {
            return;
        }

        // Creamos rayo desde el centro de la cámara.
        Ray rayo = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

        // Dibujamos raycast para debug.
        if (mostrarRayosDebug)
        {
            Debug.DrawRay(rayo.origin, rayo.direction * distanciaMirada, Color.green);
        }

        // Primero revisamos si una pared bloquea antes del loot.
        if (Physics.Raycast(rayo, out RaycastHit hitBloqueo, distanciaMirada, capasQueBloqueanVision, QueryTriggerInteraction.Ignore))
        {
            // Si pegamos primero con pared, no mostramos loot detrás.
            return;
        }

        // Buscamos loot al frente.
        if (Physics.Raycast(rayo, out RaycastHit hitLoot, distanciaMirada, capasLoot, QueryTriggerInteraction.Collide))
        {
            LootItem loot = hitLoot.collider.GetComponentInParent<LootItem>();

            if (loot == null)
            {
                return;
            }

            MostrarUILoot(loot);
        }
    }

    private void EscanearLootCercano()
    {
        // Si no hay cámara, no hacemos nada.
        if (camaraJugador == null)
        {
            return;
        }

        // Buscamos colliders de loot alrededor del jugador.
        Collider[] encontrados = Physics.OverlapSphere(
            camaraJugador.transform.position,
            radioEscaneo,
            capasLoot,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < encontrados.Length; i++)
        {
            LootItem loot = encontrados[i].GetComponentInParent<LootItem>();

            if (loot == null)
            {
                continue;
            }

            // Revisamos si está dentro del cono de visión.
            if (!EstaDentroDelCono(loot.transform.position))
            {
                continue;
            }

            // Revisamos si una estructura lo tapa.
            if (HayObstaculoEntreCamaraYLoot(loot))
            {
                continue;
            }

            MostrarUILoot(loot);
        }
    }

    private bool EstaDentroDelCono(Vector3 posicionLoot)
    {
        // Dirección hacia el loot.
        Vector3 direccion = posicionLoot - camaraJugador.transform.position;

        // Si está detrás, no se muestra.
        if (Vector3.Dot(camaraJugador.transform.forward, direccion.normalized) <= 0f)
        {
            return false;
        }

        // Calculamos ángulo.
        float angulo = Vector3.Angle(camaraJugador.transform.forward, direccion.normalized);

        // Debe estar dentro del ángulo.
        return angulo <= anguloEscaneo * 0.5f;
    }

    private bool HayObstaculoEntreCamaraYLoot(LootItem loot)
    {
        // Si no hay loot, por seguridad decimos que sí hay obstáculo.
        if (loot == null)
        {
            return true;
        }

        // Origen desde cámara.
        Vector3 origen = camaraJugador.transform.position;

        // Destino hacia el loot.
        Vector3 destino = loot.transform.position;

        // Dirección.
        Vector3 direccion = destino - origen;

        // Distancia.
        float distancia = direccion.magnitude;

        // Raycast contra paredes/estructuras.
        if (Physics.Raycast(
            origen,
            direccion.normalized,
            out RaycastHit hit,
            distancia,
            capasQueBloqueanVision,
            QueryTriggerInteraction.Ignore))
        {
            // Si el obstáculo no es parte del mismo loot, bloquea.
            LootItem lootDelImpacto = hit.collider.GetComponentInParent<LootItem>();

            if (lootDelImpacto != loot)
            {
                if (mostrarRayosDebug)
                {
                    Debug.DrawLine(origen, hit.point, Color.red);
                }

                return true;
            }
        }

        if (mostrarRayosDebug)
        {
            Debug.DrawLine(origen, destino, Color.cyan);
        }

        return false;
    }

    private void MostrarUILoot(LootItem loot)
    {
        // Buscamos la UI de escaneo en el loot.
        LootScanUI ui = loot.GetComponentInChildren<LootScanUI>(true);

        // Si no tiene UI, avisamos.
        if (ui == null)
        {
            Debug.LogWarning("El loot " + loot.name + " no tiene LootScanUI en hijos.");
            return;
        }

        // Evitamos pedir mostrar dos veces el mismo loot en el mismo frame.
        if (lootMostradosEsteFrame.Contains(ui))
        {
            return;
        }

        lootMostradosEsteFrame.Add(ui);

        // Le pedimos a la UI que se muestre.
        ui.SolicitarMostrar(camaraJugador);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja radio de escaneo.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioEscaneo);
    }
    }


