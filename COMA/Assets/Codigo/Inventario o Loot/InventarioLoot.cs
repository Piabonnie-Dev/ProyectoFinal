using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LootGuardado
{
    public string nombre;
    public int valor;
    public float peso;

public LootGuardado(string nombre, int valor, float peso)
    {
        this.nombre = nombre;
        this.valor = valor;
        this.peso = peso;
    }

}


public class InventarioLoot : MonoBehaviour
{
   [Header("Capacidad")]
   public int capacidadMaximaObjetos = 10;
   public float pesoMaximo = 20f;

   [Header("Estado actual")]
   public List<LootGuardado> lootRecolectado = new List<LootGuardado>();
   public float pesoActual = 0f;
   public int valorTotal = 0;
   public event Action AlCambiarInventario;

   public bool PuedeRecoger(LootItem item)
    {
        if(item == null) return false;
        if(lootRecolectado.Count >= capacidadMaximaObjetos) return false;
        if (pesoActual + item.peso > pesoMaximo) return false;

        return true;
    }
    public bool AgregarLoot(LootItem item)
    {
        if(!PuedeRecoger(item))
        return false;

        LootGuardado nuevo = new LootGuardado(item.nombreLoot, item.valor, item.peso);
        lootRecolectado.Add(nuevo);

        pesoActual += item.peso;
        valorTotal += item.valor;
   Debug.Log( "Recogiste: " + item.nombreLoot +
            " | Valor: " + item.valor +
            " | Peso: " + item.peso +
            "\nObjetos: " + lootRecolectado.Count + "/" + capacidadMaximaObjetos +
            " | Peso total: " + pesoActual + "/" + pesoMaximo +
            " | Valor total: " + valorTotal);

            NotificarCambio();
        return true;
    }

    public void VaciarInventario()
    {
        lootRecolectado.Clear();
        pesoActual = 0f;
        valorTotal = 0;
    }

    public List<LootGuardado> ObtenerLoot()
    {
        return lootRecolectado;
    }
    void NotificarCambio()
    {
        AlCambiarInventario?.Invoke();
    }
    
}
