using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LootGuardado
{
    public string nombre;
    public int valor;
    public float peso;
    public Sprite icono;

    public LootGuardado(
        string nombre,
        int valor,
        float peso,
        Sprite icono)
    {
        this.nombre = nombre;
        this.valor = valor;
        this.peso = peso;
        this.icono = icono;
    }
}

public class InventarioLoot : MonoBehaviour
{
    [Header("Capacidad")]
    public int capacidadMaximaObjetos = 10;
    public float pesoMaximo = 20f;

    [Header("Estado actual")]
    public List<LootGuardado> lootRecolectado =
        new List<LootGuardado>();

    public float pesoActual = 0f;
    public int valorTotal = 0;

    // Se ejecuta al agregar o vaciar el inventario.
    public event Action AlCambiarInventario;

    public int CantidadObjetos
    {
        get
        {
            return lootRecolectado.Count;
        }
    }

    public bool EstaVacio
    {
        get
        {
            return lootRecolectado.Count == 0;
        }
    }

    public bool PuedeRecoger(LootItem item)
    {
        // No se puede recoger una referencia vacía.
        if (item == null)
        {
            return false;
        }

        // Revisamos límite de objetos.
        if (lootRecolectado.Count >= capacidadMaximaObjetos)
        {
            return false;
        }

        // Revisamos límite de peso.
        if (pesoActual + item.peso > pesoMaximo)
        {
            return false;
        }

        return true;
    }

    public bool AgregarLoot(LootItem item)
    {
        // Si no hay espacio, no lo agregamos.
        if (!PuedeRecoger(item))
        {
            return false;
        }

        // Guardamos todos los datos necesarios,
        // incluyendo la imagen del inventario.
        LootGuardado nuevoLoot = new LootGuardado(
            item.nombreLoot,
            item.valor,
            item.peso,
            item.iconoInventario
        );

        lootRecolectado.Add(nuevoLoot);

        // Actualizamos totales.
        pesoActual += item.peso;
        valorTotal += item.valor;

        Debug.Log(
            "Recogiste: " + item.nombreLoot +
            "\nValor: $" + item.valor +
            "\nPeso: " + item.peso.ToString("0.0") + " kg" +
            "\nObjetos: " + CantidadObjetos +
            " / " + capacidadMaximaObjetos +
            "\nPeso total: " + pesoActual.ToString("0.0") +
            " / " + pesoMaximo.ToString("0.0") + " kg" +
            "\nValor total: $" + valorTotal
        );

        NotificarCambio();

        return true;
    }

    public void VaciarInventario()
    {
        // Eliminamos todo el loot entregado.
        lootRecolectado.Clear();

        // Reiniciamos los acumuladores.
        pesoActual = 0f;
        valorTotal = 0;

        // IMPORTANTE:
        // Ahora sí notificamos a la interfaz.
        NotificarCambio();
    }

    public List<LootGuardado> ObtenerLoot()
    {
        return lootRecolectado;
    }

    private void NotificarCambio()
    {
        AlCambiarInventario?.Invoke();
    }
}