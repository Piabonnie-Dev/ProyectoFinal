using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponFPS : MonoBehaviour
{
    public enum TipoDisparo
    {
        Normal,
        Stun,
        Explosiva
    }

    [Header("Referencias")]
    public Camera camaraJugador;
    public TMP_Text textoMunicion;
    public TMP_Text textoModoDisparo;
    public Image crosshair;

    [Header("Salida visual del disparo")]

    // Punto desde donde saldrá visualmente la bala.
    public Transform puntoSalidaBala;

    // Prefab visual para el disparo normal.
    public GameObject prefabProyectilNormal;

    // Prefab visual para el disparo stun.
    public GameObject prefabProyectilStun;

    // Prefab visual para el disparo explosivo.
    public GameObject prefabProyectilExplosivo;

    [Header("Partículas de boca de cañón")]

    // Partícula que se reproduce al disparar en modo normal.
    public ParticleSystem particulasDisparoNormal;

    // Partícula que se reproduce al disparar en modo stun.
    public ParticleSystem particulasDisparoStun;

    // Partícula que se reproduce al disparar en modo explosivo.
    public ParticleSystem particulasDisparoExplosivo;

    [Header("Efectos de impacto")]

    // Efecto que aparece cuando pega un disparo normal.
    public GameObject efectoImpactoNormal;

    // Efecto que aparece cuando pega un disparo stun.
    public GameObject efectoImpactoStun;

    // Efecto que aparece cuando pega un disparo explosivo.
    public GameObject efectoImpactoExplosivo;

    [Header("Línea visual de respaldo")]

    // Si no hay prefab de proyectil, se dibuja una línea rápida.
    public bool usarLineaFallback = true;

    // Tiempo que dura la línea visual si no hay prefab.
    public float duracionLineaFallback = 0.04f;

    // Grosor inicial de la línea.
    public float grosorLineaInicio = 0.035f;

    // Grosor final de la línea.
    public float grosorLineaFinal = 0.01f;

    [Header("Colores de respaldo")]
    public Color colorNormal = Color.white;
    public Color colorStun = Color.cyan;
    public Color colorExplosiva = new Color(1f, 0.45f, 0.1f);

    [Header("Tipo actual")]
    public TipoDisparo tipoDisparo = TipoDisparo.Normal;

    [Header("Disparo")]
    public float alcance = 60f;
    public float cadencia = 0.2f;
    public float dispersionCadera = 0.02f;
    public float dispersionApuntando = 0.005f;

    [Header("Daño / efectos")]
    public float danioNormal = 25f;
    public float duracionStun = 4f;
    public float danioExplosivo = 40f;
    public float radioExplosion = 4f;

    [Header("Munición")]
    public int balasPorCargador = 12;
    public int municionEnCargador = 12;
    public int municionReserva = 60;
    public float tiempoRecarga = 1.5f;

    [Header("Apuntado")]
    public float fovNormal = 72f;
    public float fovApuntando = 50f;
    public float velocidadApuntado = 10f;
    public Vector3 posicionReposo = new Vector3(0f, 0f, 0f);
    public Vector3 posicionApuntando = new Vector3(-0.08f, -0.03f, 0.12f);

    [Header("Crosshair")]
    public float tamCrosshairNormal = 22f;
    public float tamCrosshairApuntando = 12f;

    // Indica si el arma está recargando.
    private bool recargando = false;

    // Indica si el jugador está apuntando.
    private bool apuntando = false;

    // Guarda el tiempo del último disparo.
    private float ultimoDisparo = -999f;

    // Posición objetivo del arma al apuntar o descansar.
    private Vector3 posicionObjetivo;

    void Start()
    {
        // Si no asignaste cámara manualmente, intenta buscarla en los padres.
        if (camaraJugador == null)
            camaraJugador = GetComponentInParent<Camera>();

        // Si no asignaste punto de salida, usamos el propio objeto del arma.
        if (puntoSalidaBala == null)
            puntoSalidaBala = transform;

        // Guardamos la posición inicial del arma.
        posicionObjetivo = posicionReposo;

        // Actualizamos los textos iniciales.
        ActualizarUI();
    }

    void Update()
    {
        // Si la partida terminó, no permitimos disparar.
        if (ControlPartida.PartidaTerminada)
            return;

        // Si el inventario está abierto, no permitimos disparar.
        if (UIInventarioLoot.InventarioAbierto)
            return;

        // Si el panel de mejoras está abierto, no permitimos disparar.
        if (SuitUpgradeUI.PanelAbierto)
            return;

        // Click derecho para apuntar.
        apuntando = Input.GetMouseButton(1);

        // Actualizamos el FOV, posición del arma y crosshair.
        ActualizarVisualApuntado();

        // R para recargar.
        if (Input.GetKeyDown(KeyCode.R))
            IntentarRecargar();

        // Click izquierdo para disparar.
        if (Input.GetMouseButton(0))
            IntentarDisparar();

        // Tecla 1 para disparo normal.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            tipoDisparo = TipoDisparo.Normal;
            ActualizarUI();
        }

        // Tecla 2 para disparo stun.
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            tipoDisparo = TipoDisparo.Stun;
            ActualizarUI();
        }

        // Tecla 3 para disparo explosivo.
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            tipoDisparo = TipoDisparo.Explosiva;
            ActualizarUI();
        }
    }

    void ActualizarVisualApuntado()
    {
        // Si no hay cámara, no hacemos cambios visuales.
        if (camaraJugador == null)
            return;

        // Elegimos el FOV dependiendo de si está apuntando o no.
        float fovObjetivo = apuntando ? fovApuntando : fovNormal;

        // Movemos suavemente el FOV.
        camaraJugador.fieldOfView = Mathf.Lerp(
            camaraJugador.fieldOfView,
            fovObjetivo,
            velocidadApuntado * Time.deltaTime
        );

        // Elegimos la posición local del arma.
        posicionObjetivo = apuntando ? posicionApuntando : posicionReposo;

        // Movemos suavemente el arma.
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            velocidadApuntado * Time.deltaTime
        );

        // Si hay crosshair, cambiamos su tamaño.
        if (crosshair != null)
        {
            float tamObjetivo = apuntando ? tamCrosshairApuntando : tamCrosshairNormal;

            crosshair.rectTransform.sizeDelta = Vector2.Lerp(
                crosshair.rectTransform.sizeDelta,
                new Vector2(tamObjetivo, tamObjetivo),
                velocidadApuntado * Time.deltaTime
            );
        }
    }

    void IntentarDisparar()
    {
        // Si está recargando, no dispara.
        if (recargando)
            return;

        // Respetamos la cadencia del arma.
        if (Time.time < ultimoDisparo + cadencia)
            return;

        // Si no hay cámara, no podemos calcular el disparo.
        if (camaraJugador == null)
            return;

        // Si no hay munición, intentamos recargar.
        if (municionEnCargador <= 0)
        {
            IntentarRecargar();
            return;
        }

        // Guardamos el tiempo del disparo.
        ultimoDisparo = Time.time;

        // Restamos una bala.
        municionEnCargador--;

        // Actualizamos la UI.
        ActualizarUI();

        // Calculamos la dispersión dependiendo de si apunta o no.
        float dispersion = apuntando ? dispersionApuntando : dispersionCadera;

        // Dirección base del disparo.
        Vector3 direccion = camaraJugador.transform.forward;

        // Agregamos dispersión horizontal.
        direccion += camaraJugador.transform.right * Random.Range(-dispersion, dispersion);

        // Agregamos dispersión vertical.
        direccion += camaraJugador.transform.up * Random.Range(-dispersion, dispersion);

        // Normalizamos la dirección final.
        direccion.Normalize();

        // Creamos el raycast desde la cámara.
        Ray ray = new Ray(camaraJugador.transform.position, direccion);

        // Por defecto, el disparo termina al máximo alcance.
        Vector3 puntoFinal = ray.origin + ray.direction * alcance;

        // Guardamos si hubo impacto real.
        bool huboImpacto = false;

        // Guardamos información del impacto.
        RaycastHit hit;

        // Lanzamos el raycast.
        if (Physics.Raycast(ray, out hit, alcance))
        {
            // Si impactó algo, el punto final será el punto de impacto.
            puntoFinal = hit.point;

            // Marcamos que sí hubo impacto.
            huboImpacto = true;
        }

        // Creamos el feedback visual del disparo.
        CrearFeedbackDisparo(puntoFinal, direccion, huboImpacto);

        // Si no hubo impacto, no aplicamos daño.
        if (!huboImpacto)
            return;

        // Si el modo es explosivo, aplicamos explosión en el punto de impacto.
        if (tipoDisparo == TipoDisparo.Explosiva)
        {
            AplicarExplosion(hit.point);
            return;
        }

        // Buscamos vida de enemigo en el objeto impactado o sus padres.
        EnemyHealth enemigo = hit.collider.GetComponentInParent<EnemyHealth>();

        // Si no impactó enemigo, terminamos.
        if (enemigo == null)
            return;

        // Si el disparo es normal, hacemos daño.
        if (tipoDisparo == TipoDisparo.Normal)
        {
            enemigo.RecibirDanio(danioNormal);
            Debug.Log("Disparo normal impactó a " + enemigo.name);
        }

        // Si el disparo es stun, aturdimos.
        else if (tipoDisparo == TipoDisparo.Stun)
        {
            enemigo.Aturdir(duracionStun);
            Debug.Log("Disparo stun impactó a " + enemigo.name);
        }
    }

    void CrearFeedbackDisparo(Vector3 puntoFinal, Vector3 direccion, bool huboImpacto)
    {
        // Reproducimos la partícula de boca de cañón correspondiente.
        ReproducirParticulaDisparo();

        // Creamos el proyectil visual o línea de respaldo.
        CrearProyectilVisual(puntoFinal);

        // Si hubo impacto, creamos efecto en el punto golpeado.
        if (huboImpacto)
        {
            CrearEfectoImpacto(puntoFinal, direccion);
        }
    }

    void ReproducirParticulaDisparo()
    {
        // Elegimos qué partícula usar dependiendo del modo actual.
        ParticleSystem particulaActual = null;

        if (tipoDisparo == TipoDisparo.Normal)
            particulaActual = particulasDisparoNormal;
        else if (tipoDisparo == TipoDisparo.Stun)
            particulaActual = particulasDisparoStun;
        else if (tipoDisparo == TipoDisparo.Explosiva)
            particulaActual = particulasDisparoExplosivo;

        // Si hay partícula asignada, la reproducimos.
        if (particulaActual != null)
        {
            particulaActual.Stop();
            particulaActual.Play();
        }
    }

    void CrearProyectilVisual(Vector3 puntoFinal)
    {
        // Elegimos el prefab visual según el tipo de disparo.
        GameObject prefab = ObtenerPrefabProyectil();

        // El proyectil saldrá desde el punto de salida del arma.
        Vector3 origen = puntoSalidaBala != null ? puntoSalidaBala.position : transform.position;

        // Calculamos dirección visual del proyectil.
        Vector3 direccionVisual = puntoFinal - origen;

        // Si la dirección es casi cero, evitamos errores de rotación.
        if (direccionVisual.sqrMagnitude <= 0.001f)
            direccionVisual = transform.forward;

        // Si sí hay prefab, lo instanciamos.
        if (prefab != null)
        {
            GameObject proyectil = Instantiate(
                prefab,
                origen,
                Quaternion.LookRotation(direccionVisual.normalized)
            );

            // Buscamos el script que mueve el proyectil visual.
            ProyectilVisual visual = proyectil.GetComponent<ProyectilVisual>();

            // Si tiene el script, le damos destino.
            if (visual != null)
            {
                visual.Iniciar(puntoFinal);
            }
            else
            {
                // Si no tiene script, lo destruimos rápido para evitar basura.
                Destroy(proyectil, 0.5f);
            }

            return;
        }

        // Si no hay prefab, usamos una línea visual rápida.
        if (usarLineaFallback)
        {
            CrearLineaFallback(origen, puntoFinal);
        }
    }

    GameObject ObtenerPrefabProyectil()
    {
        // Regresamos el prefab correspondiente al modo actual.
        if (tipoDisparo == TipoDisparo.Normal)
            return prefabProyectilNormal;

        if (tipoDisparo == TipoDisparo.Stun)
            return prefabProyectilStun;

        if (tipoDisparo == TipoDisparo.Explosiva)
            return prefabProyectilExplosivo;

        return null;
    }

    void CrearLineaFallback(Vector3 origen, Vector3 destino)
    {
        // Creamos un objeto temporal para la línea.
        GameObject lineaObj = new GameObject("LineaDisparoTemporal");

        // Agregamos un LineRenderer.
        LineRenderer linea = lineaObj.AddComponent<LineRenderer>();

        // Creamos un material simple para que la línea se vea.
        linea.material = new Material(Shader.Find("Sprites/Default"));

        // Asignamos el color según el tipo de disparo.
        Color color = ObtenerColorDisparo();

        // Aplicamos el color a la línea.
        linea.startColor = color;
        linea.endColor = color;

        // Definimos el grosor de la línea.
        linea.startWidth = grosorLineaInicio;
        linea.endWidth = grosorLineaFinal;

        // La línea tendrá dos puntos: origen y destino.
        linea.positionCount = 2;
        linea.SetPosition(0, origen);
        linea.SetPosition(1, destino);

        // Destruimos la línea rápidamente.
        Destroy(lineaObj, duracionLineaFallback);
    }

    Color ObtenerColorDisparo()
    {
        // Color para disparo normal.
        if (tipoDisparo == TipoDisparo.Normal)
            return colorNormal;

        // Color para disparo stun.
        if (tipoDisparo == TipoDisparo.Stun)
            return colorStun;

        // Color para disparo explosivo.
        if (tipoDisparo == TipoDisparo.Explosiva)
            return colorExplosiva;

        return Color.white;
    }

    void CrearEfectoImpacto(Vector3 punto, Vector3 direccion)
    {
        // Elegimos el efecto según el tipo de disparo.
        GameObject efecto = null;

        if (tipoDisparo == TipoDisparo.Normal)
            efecto = efectoImpactoNormal;
        else if (tipoDisparo == TipoDisparo.Stun)
            efecto = efectoImpactoStun;
        else if (tipoDisparo == TipoDisparo.Explosiva)
            efecto = efectoImpactoExplosivo;

        // Si no hay efecto asignado, no hacemos nada.
        if (efecto == null)
            return;

        // Rotamos el efecto para que mire contrario a la dirección del disparo.
        Quaternion rotacion = Quaternion.LookRotation(-direccion);

        // Creamos el efecto en el punto de impacto.
        GameObject nuevoEfecto = Instantiate(efecto, punto, rotacion);

        // Lo destruimos después de un tiempo para no llenar la escena.
        Destroy(nuevoEfecto, 2f);
    }

    void AplicarExplosion(Vector3 centro)
    {
        // Buscamos colliders dentro del radio de explosión.
        Collider[] hits = Physics.OverlapSphere(centro, radioExplosion);

        // Recorremos todos los colliders encontrados.
        for (int i = 0; i < hits.Length; i++)
        {
            // Buscamos si pertenece a un enemigo.
            EnemyHealth enemigo = hits[i].GetComponentInParent<EnemyHealth>();

            // Si es enemigo, le aplicamos daño explosivo.
            if (enemigo != null)
            {
                enemigo.RecibirDanio(danioExplosivo);
                Debug.Log("Explosión dañó a " + enemigo.name);
            }
        }
    }

    void IntentarRecargar()
    {
        // Si ya está recargando, no hacemos nada.
        if (recargando)
            return;

        // Si el cargador ya está lleno, no recargamos.
        if (municionEnCargador >= balasPorCargador)
            return;

        // Si no hay reserva, no recargamos.
        if (municionReserva <= 0)
            return;

        // Iniciamos la corrutina de recarga.
        StartCoroutine(Recargar());
    }

    IEnumerator Recargar()
    {
        // Marcamos que estamos recargando.
        recargando = true;

        // Esperamos el tiempo de recarga.
        yield return new WaitForSeconds(tiempoRecarga);

        // Calculamos cuántas balas faltan para llenar el cargador.
        int faltantes = balasPorCargador - municionEnCargador;

        // Calculamos cuántas balas podemos cargar desde la reserva.
        int balasACargar = Mathf.Min(faltantes, municionReserva);

        // Agregamos balas al cargador.
        municionEnCargador += balasACargar;

        // Quitamos balas de la reserva.
        municionReserva -= balasACargar;

        // Terminamos la recarga.
        recargando = false;

        // Actualizamos la UI.
        ActualizarUI();
    }

    void ActualizarUI()
    {
        // Actualizamos el texto de munición.
        if (textoMunicion != null)
            textoMunicion.text = municionEnCargador + " / " + municionReserva;

        // Actualizamos el texto del modo de disparo.
        if (textoModoDisparo != null)
            textoModoDisparo.text = "Modo: " + tipoDisparo;
    }

    void OnDrawGizmosSelected()
    {
        // Dibujamos el radio de explosión alrededor del arma solo como referencia de editor.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}