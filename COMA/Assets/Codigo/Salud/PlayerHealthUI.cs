using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencia a la vida del jugador")]
     // Aquí arrastras el objeto Player que tiene PlayerHealth
    public PlayerHealth playerHealth;

    [Header("Elementos UI")]
    // Slider que representa la barra de vida.
    public Slider barraVida;
     // Texto opcional para mostrar 100 / 100.
    public TMP_Text textoVida;

    [Header("Animacion visual")]
    public float velocidadSuavizado = 8f;
    private float valorVisual = 1f;

    void Start()
    {
        
        //si no se asigno PlayerHealth manualmente, intentamos buscarlo.
        if(playerHealth == null)
        playerHealth = FindObjectOfType<PlayerHealth>();

        if(barraVida != null)
        {
            barraVida.minValue = 0f;
            barraVida.maxValue = 1f;
            barraVida.value = 1f;
        }

        ActualizarUI(true);
    }

    void Update()
    {
        ActualizarUI(false);
    }

    void ActualizarUI(bool instantaneo)
    {
        if(playerHealth == null)
        return;


        float porcentajeVida = playerHealth.vidaActual / playerHealth.vidaMaxima;

        porcentajeVida = Mathf.Clamp01(porcentajeVida);
        if (instantaneo)
        {
            valorVisual = porcentajeVida;
        }
        else
        {
            valorVisual = Mathf.Lerp(valorVisual, porcentajeVida, velocidadSuavizado * Time.deltaTime);
        }
        
if(barraVida != null)
barraVida.value = valorVisual;

if (textoVida != null)
        {
            textoVida.text = Mathf.CeilToInt(playerHealth.vidaActual)+ " / " + Mathf.CeilToInt(playerHealth.vidaMaxima);
        }

    }

}
