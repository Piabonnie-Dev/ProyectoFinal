
using UnityEngine;

public class HitboxEnemigo : MonoBehaviour
{
[Header("Referencias al enemigo principal")]
public GameObject objetoConVida;

[Header("Nombre de la funcion de daño en EnemyHealth")]
public string nombreFuncionDaño = "RecibirDaño";

[Header("Ajuste de daño")]
public float multiplicadorDaño = 1f;

private void Awake()
    {
        
        if(objetoConVida == null)
        {
            EnemyHealth vidaEncontrada = GetComponentInParent<EnemyHealth>();

            if(vidaEncontrada != null)
            {
                objetoConVida = vidaEncontrada.gameObject;
            }
        }
    }

    public void RecibirDaño(float daño)
    {
        EnviarDañoAlEnemigo(daño);

    }
    public void RecibirDanio(float daño)
    {
        // Esta versión sirve por si algún script usa "Danio" sin ñ.
        EnviarDañoAlEnemigo(daño);
    }

    public void TakeDamage(float daño)
    {
        // Esta versión sirve por si algún script usa el nombre en inglés.
        EnviarDañoAlEnemigo(daño);
    }

    private void EnviarDañoAlEnemigo(float daño)
    {
        // Si no hay objeto con vida, evitamos errores.
        if (objetoConVida == null)
        {
            Debug.LogWarning("La hitbox no tiene asignado un objeto con vida.");
            return;
        }

        // Calculamos el daño final.
        float dañoFinal = daño * multiplicadorDaño;

        // Mandamos el daño al objeto principal V37.
        objetoConVida.SendMessage(nombreFuncionDaño, dañoFinal, SendMessageOptions.DontRequireReceiver);
    }
}
