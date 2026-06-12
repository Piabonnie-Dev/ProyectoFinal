using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Header("Datos del loot")]
    public string nombreLoot = "Chatarra";

    [Min(0)]
    public int valor = 10;

    [Min(0f)]
    public float peso = 1f;

    [Header("Imagen para el inventario")]
    public Sprite iconoInventario;

    [Header("Recolección")]
    public bool destruirAlRecoger = true;
}
