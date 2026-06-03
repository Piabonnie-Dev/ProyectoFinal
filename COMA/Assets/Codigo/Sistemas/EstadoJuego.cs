
using UnityEngine;

public class EstadoJuego : MonoBehaviour
{
    public static bool JuegoPausado = false;
    public static void PausarJuego()
    {
        
        JuegoPausado = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public static void ReanudarJuego()
    {
        
        JuegoPausado = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void PrepararMenuPrincipal()
    {
        JuegoPausado = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
