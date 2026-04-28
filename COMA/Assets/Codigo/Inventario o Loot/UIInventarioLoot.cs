using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventarioLoot : MonoBehaviour
{
    public static bool InventarioAbierto
    {
        get; private set;
    }

    [Header("Referencias")]
    public InventarioLoot inventario;
    public GameObject panelInventario;
    public TMP_Text textoListaLoot;
    public TMP_Text textoResumen;

    [Header("Entrada")]
    public KeyCode teclaInventario = KeyCode.Tab;

    void Start()
    {
        if(panelInventario != null)
        panelInventario.SetActive(false);

  InventarioAbierto = false;
  ActualizarUI();
  if(inventario != null)
  inventario.AlCambiarInventario -= ActualizarUI;

    }

 void Update()
    {
        if (Input.GetKeyDown(teclaInventario))
        {
            AlternarInventario();
        }
    }   
    public void AlternarInventario()
    {
        InventarioAbierto = !InventarioAbierto;
        if(panelInventario != null)
        panelInventario.SetActive(InventarioAbierto);

        if (InventarioAbierto)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ActualizarUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void ActualizarUI()
    {
        if (inventario == null) return;
    if(textoListaLoot != null)
        {
            StringBuilder sb = new StringBuilder();

            var lista = inventario.ObtenerLoot();
            if(lista.Count == 0)
            {
                sb.AppendLine("Inventario vacio.");
            }
            else
            {
                for(int i =0; i<lista.Count; i++)
                {
                    LootGuardado loot = lista[i];
                    sb.AppendLine((i + 1) + ". " + loot.nombre + 
                    " | $ " + loot.valor +
                     " | Peso: " + loot.peso + "kg");     
                }
            }
            textoListaLoot.text = sb.ToString();
        }
        if(textoResumen != null)
        {
            textoResumen.text = 
            "Objetos:" + inventario.lootRecolectado.Count + "/" + inventario.capacidadMaximaObjetos +
            "\nPeso: " + inventario.pesoActual.ToString("0.0") + "/" + inventario.pesoMaximo.ToString("0.0") + " kg" +
                "\nValor total: $" + inventario.valorTotal;
        }
    }
}
