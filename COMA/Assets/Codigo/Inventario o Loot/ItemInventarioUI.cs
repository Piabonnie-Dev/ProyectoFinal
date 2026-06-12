using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventarioUI : MonoBehaviour
{
    [Header("Referencias visuales")]
    public Image imagenIcono;
    public TMP_Text textoNombre;
    public TMP_Text textoPeso;
    public TMP_Text textoValor;

    [Header("Imagen alternativa")]
    public Sprite iconoPorDefecto;

    public void Configurar(LootGuardado datos)
    {
        // Mostramos la imagen específica del loot.
        // Si no tiene una imagen, usamos la imagen por defecto.
        if (imagenIcono != null)
        {
            if (datos.icono != null)
            {
                imagenIcono.sprite = datos.icono;
                imagenIcono.enabled = true;
            }
            else if (iconoPorDefecto != null)
            {
                imagenIcono.sprite = iconoPorDefecto;
                imagenIcono.enabled = true;
            }
            else
            {
                imagenIcono.enabled = false;
            }
        }

        // Mostramos el nombre del objeto.
        if (textoNombre != null)
        {
            textoNombre.text = datos.nombre;
        }

        // Mostramos el peso con un decimal.
        if (textoPeso != null)
        {
            textoPeso.text =
                "Peso: " + datos.peso.ToString("0.0") + " kg";
        }

        // Mostramos el valor económico.
        if (textoValor != null)
        {
            textoValor.text = "Valor: $" + datos.valor;
        }
    }
}
