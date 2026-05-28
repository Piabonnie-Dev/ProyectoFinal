using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Referencia al enemigo")]

    // Objeto que tiene el script EnemyHealth.
    public EnemyHealth enemyHealth;

    [Header("UI de vida")]

    // Slider que representa la vida del enemigo.
    public Slider barraVida;

    // Texto opcional. Puede mostrar 100 / 100.
    public TMP_Text textoVida;

    [Header("UI de muerte")]

    // Objeto que contiene la calavera.
    // Puede ser un TextMeshPro con el símbolo ☠.
    public GameObject iconoCalavera;

    [Header("Billboarding")]

    // Cámara del jugador.
    public Camera camaraJugador;

    // Si está activado, la barra siempre mira hacia la cámara.
    public bool mirarSiempreACamara = true;

    [Header("Movimiento visual de la barra")]

    // Velocidad con la que la barra baja suavemente.
    public float velocidadSuavizado = 10f;

    // Valor visual interno de la barra.
    private float valorVisual = 1f;

    void Start()
    {
        // Si no se asignó EnemyHealth, lo buscamos en el padre.
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        // Si no se asignó cámara, usamos la cámara principal.
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Configuramos el Slider para trabajar de 0 a 1.
        if (barraVida != null)
        {
            barraVida.minValue = 0f;
            barraVida.maxValue = 1f;
            barraVida.value = 1f;
        }

        // Al iniciar, la calavera debe estar apagada.
        if (iconoCalavera != null)
        {
            iconoCalavera.SetActive(false);
        }

        // Actualizamos la barra inmediatamente al iniciar.
        ActualizarUI(true);
    }

    void Update()
    {
        // Actualizamos la vida visual.
        ActualizarUI(false);
    }

    void LateUpdate()
    {
        // LateUpdate ayuda a que el billboarding se actualice después del movimiento de cámara.
        if (mirarSiempreACamara)
        {
            MirarACamara();
        }
    }

    void ActualizarUI(bool instantaneo)
    {
        // Si no hay EnemyHealth, no podemos actualizar.
        if (enemyHealth == null)
        {
            return;
        }

        // Si el enemigo está muerto, mostramos calavera.
        if  (enemyHealth.estaMuerto)
        {
            MostrarCalavera();
            return;
        }

        // Si el enemigo está vivo, mostramos barra.
        MostrarBarraVida();

        // Calculamos porcentaje de vida.
        float porcentajeVida = enemyHealth.vidaActual / enemyHealth.vidaMaxima;

        // Evitamos valores menores a 0 o mayores a 1.
        porcentajeVida = Mathf.Clamp01(porcentajeVida);

        // Si es actualización instantánea, la barra cambia de golpe.
        if (instantaneo)
        {
            valorVisual = porcentajeVida;
        }
        else
        {
            // Si no, la barra baja suavemente.
            valorVisual = Mathf.Lerp(
                valorVisual,
                porcentajeVida,
                velocidadSuavizado * Time.deltaTime
                                                    );
        }

        // Aplicamos el valor visual al Slider.
        if (barraVida != null)
        {
            barraVida.value = valorVisual;
        }

        // Actualizamos el texto de vida si existe.
        if (textoVida != null)
        {
            textoVida.text =
                Mathf.CeilToInt(enemyHealth.vidaActual) +
                " / " +
                 Mathf.CeilToInt(enemyHealth.vidaMaxima);
        }
    }

    void MostrarBarraVida()
    {
        // Activamos la barra de vida.
        if (barraVida != null)
        {
            barraVida.gameObject.SetActive(true);
        }

        // Activamos el texto de vida si existe.
        if (textoVida != null)
        {
            textoVida.gameObject.SetActive(true);
        }

        // Ocultamos la calavera.
        if (iconoCalavera != null)
        {
            iconoCalavera.SetActive(false);
        }
    }

    void MostrarCalavera()
    {
        // Ocultamos la barra de vida.
        if (barraVida != null)
        {
            barraVida.gameObject.SetActive(false);
        }

        // Ocultamos el texto de vida.
        if (textoVida != null)
        {
            textoVida.gameObject.SetActive(false);
        }

        // Mostramos la calavera.
        if (iconoCalavera != null)
        {
            iconoCalavera.SetActive(true);
        }
    }

    void MirarACamara()
    {
    // Si no hay cámara asignada, intentamos encontrarla otra vez.
     if (camaraJugador == null)
        {
        camaraJugador = Camera.main;
        }

        // Si sigue sin haber cámara, salimos.
        if  (camaraJugador == null)
        {
             return;
        }

        // Esta técnica hace que el canvas copie la orientación de la cámara.
        // Así siempre se ve plano en 2D frente al jugador.
        transform.rotation = Quaternion.LookRotation(
            transform.position - camaraJugador.transform.position
        );
    }
}