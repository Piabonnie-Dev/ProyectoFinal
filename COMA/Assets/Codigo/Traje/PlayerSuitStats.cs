using UnityEngine;

public class PlayerSuitStats : MonoBehaviour
{
    [Header("Niveles")]
    public int nivelVelocidad = 0;
    public int nivelVida = 0;
    public int nivelCapacidad = 0;

    [Header("Máximos")]
    public int maxNivelVelocidad = 5;
    public int maxNivelVida = 5;
    public int maxNivelCapacidad = 5;

    [Header("Bonos")]
    public float bonusVelocidadPorNivel = 0.15f;
    public float bonusVidaPorNivel = 20f;
    public float bonusPesoPorNivel = 5f;
    public int bonusObjetosPorNivel = 2;

    [Header("Referencias")]
    public PlayerHealth playerHealth;
    public InventarioLoot inventarioLoot;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (inventarioLoot == null)
            inventarioLoot = GetComponent<InventarioLoot>();
    }

    public float ObtenerMultiplicadorVelocidad()
    {
        return 1f + (nivelVelocidad * bonusVelocidadPorNivel);
    }

    public bool PuedeMejorarVelocidad()
    {
        return nivelVelocidad < maxNivelVelocidad;
    }

    public bool PuedeMejorarVida()
    {
        return nivelVida < maxNivelVida;
    }

    public bool PuedeMejorarCapacidad()
    {
        return nivelCapacidad < maxNivelCapacidad;
    }

    public void MejorarVelocidad()
    {
        if (!PuedeMejorarVelocidad())
            return;

        nivelVelocidad++;
        Debug.Log("Velocidad del traje mejorada. Nivel: " + nivelVelocidad);
    }

    public void MejorarVida()
    {
        if (!PuedeMejorarVida())
            return;

        nivelVida++;

        if (playerHealth != null)
        {
            playerHealth.vidaMaxima += bonusVidaPorNivel;
            playerHealth.vidaActual += bonusVidaPorNivel;
        }

        Debug.Log("Vida del traje mejorada. Nivel: " + nivelVida);
    }

    public void MejorarCapacidad()
    {
        if (!PuedeMejorarCapacidad())
            return;

        nivelCapacidad++;

        if (inventarioLoot != null)
        {
            inventarioLoot.capacidadMaximaObjetos += bonusObjetosPorNivel;
            inventarioLoot.pesoMaximo += bonusPesoPorNivel;
        }

        Debug.Log("Capacidad del traje mejorada. Nivel: " + nivelCapacidad);
    }
}