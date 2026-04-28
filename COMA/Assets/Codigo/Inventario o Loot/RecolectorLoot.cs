
using UnityEngine;

public class RecolectorLoot : MonoBehaviour
{
   [Header("Referencias")]
   public Camera camaraJugador;
   public InventarioLoot inventario;

   [Header("Entrada")]
   public KeyCode teclaRecoger = KeyCode.F;
   
   [Header("Deteccion")]
   public float distanciaRecoger = 3f;
   public LayerMask capasLoot = ~0;


   void Update()
    {
        if (Input.GetKeyDown(teclaRecoger))
        {
            IntentarRecoger();
        }
        if(camaraJugador != null)
        {
            Debug.DrawRay(camaraJugador.transform.position,
            camaraJugador.transform.forward * distanciaRecoger,
            Color.yellow);
        }
    }

void IntentarRecoger()
    {
        if(camaraJugador == null || inventario == null)
        {
            Debug.LogWarning("Falta asignar camaraJugador o inventario en RecolectorLoot.");
            return;
        }
        Ray rayo = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        if(Physics.Raycast(rayo, out RaycastHit hit, distanciaRecoger, capasLoot))
        {
            LootItem loot = hit.collider.GetComponent<LootItem>();

            if(loot == null)
            loot = hit.collider.GetComponentInParent<LootItem>();

            if(loot == null)
            loot = hit.collider.GetComponentInChildren<LootItem>();

            if(loot != null)
            {
                if (inventario.AgregarLoot(loot))
                {
                    if(loot.destruirAlRecoger)
                    Destroy(loot.gameObject);
                    else loot.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("No puedes recoger ese loot: inventario lleno o demasiado peso.");
                }
            }
            else
            {
                Debug.Log("Lo que miras no tiene componente LootItem.");
            }

        }
        else 
        {
            Debug.Log("No hay loot enfrente para recoger.");
        }
    }

}
