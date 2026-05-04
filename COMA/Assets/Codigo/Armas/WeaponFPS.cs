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
    public Image crosshair;

    [Header("Tipo de arma")]
    public TipoDisparo tipoDisparo = TipoDisparo.Normal;

    [Header("Daño")]
    public float danio = 25f;
    public float duracionStun = 4f;
    public float radioExplosion = 4f;

    [Header("Disparo")]
    public float alcance = 60f;
    public float cadencia = 0.18f;
    public float dispersionCadera = 0.02f;
    public float dispersionApuntando = 0.005f;

    [Header("Municion")]
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
    public float crosshairTamNormal = 22f;
    public float crosshairTamApuntando = 12f;

    private float ultimoDisparo = -999f;
    private bool recargando = false;
    private bool apuntando = false;
    private Vector3 posicionObjetivo;

    void Start()
    {
        if (camaraJugador == null)
            camaraJugador = GetComponentInParent<Camera>();

        posicionObjetivo = posicionReposo;
        ActualizarUI();
    }

    void Update()
    {
        if (ControlPartida.PartidaTerminada)
            return;

        if (UIInventarioLoot.InventarioAbierto)
            return;

        if (SuitUpgradeUI.PanelAbierto)
            return;

        apuntando = Input.GetMouseButton(1);

        ActualizarApuntadoVisual();

        if (Input.GetKeyDown(KeyCode.R))
            IntentarRecargar();

        if (Input.GetMouseButton(0))
            IntentarDisparar();
    }

    void ActualizarApuntadoVisual()
    {
        if (camaraJugador == null)
            return;

        float fovObjetivo = apuntando ? fovApuntando : fovNormal;
        camaraJugador.fieldOfView = Mathf.Lerp(camaraJugador.fieldOfView, fovObjetivo, velocidadApuntado * Time.deltaTime);

        posicionObjetivo = apuntando ? posicionApuntando : posicionReposo;
        transform.localPosition = Vector3.Lerp(transform.localPosition, posicionObjetivo, velocidadApuntado * Time.deltaTime);

        if (crosshair != null)
        {
            float tam = apuntando ? crosshairTamApuntando : crosshairTamNormal;
            crosshair.rectTransform.sizeDelta = Vector2.Lerp(
                crosshair.rectTransform.sizeDelta,
                new Vector2(tam, tam),
                velocidadApuntado * Time.deltaTime
            );
        }
    }

    void IntentarDisparar()
    {
        if (recargando)
            return;

        if (Time.time < ultimoDisparo + cadencia)
            return;

        if (municionEnCargador <= 0)
        {
            IntentarRecargar();
            return;
        }

        ultimoDisparo = Time.time;
        municionEnCargador--;
        ActualizarUI();

        float dispersion = apuntando ? dispersionApuntando : dispersionCadera;

        Vector3 direccion = camaraJugador.transform.forward;
        direccion += camaraJugador.transform.right * Random.Range(-dispersion, dispersion);
        direccion += camaraJugador.transform.up * Random.Range(-dispersion, dispersion);
        direccion.Normalize();

        Ray ray = new Ray(camaraJugador.transform.position, direccion);

        if (Physics.Raycast(ray, out RaycastHit hit, alcance))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);

            if (tipoDisparo == TipoDisparo.Explosiva)
            {
                Explode(hit.point);
                return;
            }

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                if (tipoDisparo == TipoDisparo.Normal)
                {
                    enemy.RecibirDanio(danio);
                }
                else if (tipoDisparo == TipoDisparo.Stun)
                {
                    enemy.Aturdir(duracionStun);
                }
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * alcance, Color.red, 1f);
        }
    }

    void Explode(Vector3 centro)
    {
        Collider[] golpes = Physics.OverlapSphere(centro, radioExplosion);

        for (int i = 0; i < golpes.Length; i++)
        {
            EnemyHealth enemy = golpes[i].GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.RecibirDanio(danio);
        }
    }

    void IntentarRecargar()
    {
        if (recargando)
            return;

        if (municionEnCargador >= balasPorCargador)
            return;

        if (municionReserva <= 0)
            return;

        StartCoroutine(Recargar());
    }

    System.Collections.IEnumerator Recargar()
    {
        recargando = true;
        yield return new WaitForSeconds(tiempoRecarga);

        int faltantes = balasPorCargador - municionEnCargador;
        int balasACargar = Mathf.Min(faltantes, municionReserva);

        municionEnCargador += balasACargar;
        municionReserva -= balasACargar;

        recargando = false;
        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (textoMunicion != null)
            textoMunicion.text = municionEnCargador + " / " + municionReserva;
    }

    public void CambiarTipoDisparo(TipoDisparo nuevoTipo)
    {
        tipoDisparo = nuevoTipo;
    }

    void OnDrawGizmosSelected()
    {
        if (tipoDisparo == TipoDisparo.Explosiva)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioExplosion);
        }
    }
}

