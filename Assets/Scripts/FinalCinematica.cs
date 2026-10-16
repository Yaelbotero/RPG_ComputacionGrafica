using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cinemática de la escena "Final".
/// 1. Al iniciar, el Personaje queda inmóvil (no camina, no salta, no ataca).
/// 2. El jugador solo puede hablar con Nun (tecla E, manejado por TextManagerNun).
/// 3. Al terminar el diálogo, Nun camina con animación hasta su destino.
/// 4. Cuando Nun llega, el Personaje camina con animación hasta su destino.
/// 5. Al llegar el Personaje, cae el rayo (durante 0.47s) y automáticamente se carga la escena "Pelea Final".
/// </summary>
public class FinalCinematica : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject jugador;
    [SerializeField] private NunController nun;
    [SerializeField] private TextManagerNun dialogoNun;
    [SerializeField] private GameObject rayo;

    [Header("Destinos (solo se usa X; la altura la mantiene el piso)")]
    [SerializeField] private float nunDestinoX = -3.04f;
    [SerializeField] private float jugadorDestinoX = -7.49f;

    [Header("Ajustes")]
    [SerializeField] private float velocidadJugador = 4f;
    [SerializeField] private float pausaEntreCaminatas = 0.3f;
    // Si es true, el jugador recupera el control al terminar la cinemática.
    [SerializeField] private bool devolverControlAlFinal = false;

    [Header("Ajustes Rayo y Transición")]
    [SerializeField] private float duracionRayo = 0.47f;
    [SerializeField] private float esperaAntesDelRayo = 0.1f;
    [SerializeField] private string escenaSiguiente = "Pelea Final";
    [SerializeField] private bool cambiarEscena = true;

    private Movimiento movimiento;
    private Player_Attack ataque;
    private Rigidbody2D rbJugador;
    private Animator animJugador;
    private SpriteRenderer spriteNun;
    private NunSigueJugador nunSigue;
    private bool dialogoTerminado;

    private void Start()
    {
        if (jugador == null || nun == null || dialogoNun == null)
        {
            Debug.LogError("FinalCinematica: faltan referencias (jugador / nun / dialogoNun).");
            enabled = false;
            return;
        }

        movimiento = jugador.GetComponent<Movimiento>();
        ataque = jugador.GetComponent<Player_Attack>();
        rbJugador = jugador.GetComponent<Rigidbody2D>();
        animJugador = jugador.GetComponent<Animator>();
        spriteNun = nun.GetComponent<SpriteRenderer>();
        nunSigue = nun.GetComponent<NunSigueJugador>();

        // Seguir al jugador movería a Nun por su cuenta y rompería la cinemática.
        if (nunSigue != null) nunSigue.enabled = false;

        // El rayo empieza oculto
        if (rayo != null) rayo.SetActive(false);

        BloquearJugador(true);
        dialogoNun.DialogoTerminado += AlTerminarDialogo;
        StartCoroutine(Secuencia());
    }

    private void OnDestroy()
    {
        if (dialogoNun != null) dialogoNun.DialogoTerminado -= AlTerminarDialogo;
    }

    private void AlTerminarDialogo()
    {
        dialogoTerminado = true;
    }

    private IEnumerator Secuencia()
    {
        yield return new WaitUntil(() => dialogoTerminado);

        // Nun camina primero.
        Vector3 posNun = nun.transform.position;
        if (spriteNun != null) spriteNun.flipX = nunDestinoX < posNun.x;
        nun.IniciarCaminata(new Vector2(nunDestinoX, posNun.y));
        yield return new WaitUntil(() => nun.CaminataTerminada);

        yield return new WaitForSeconds(pausaEntreCaminatas);

        // Cuando Nun llega, camina el Personaje.
        yield return CaminarJugador(jugadorDestinoX);

        // Cuando el Personaje llega a su posición, se activa el Rayo
        if (rayo != null)
        {
            if (esperaAntesDelRayo > 0f)
            {
                yield return new WaitForSeconds(esperaAntesDelRayo);
            }

            rayo.SetActive(true);

            var animRayo = rayo.GetComponent<Animator>();
            if (animRayo != null)
            {
                animRayo.Play(0, 0, 0f);
            }

            yield return new WaitForSeconds(duracionRayo);

            rayo.SetActive(false);
        }

        if (cambiarEscena && !string.IsNullOrEmpty(escenaSiguiente))
        {
            Time.timeScale = 1f;
            Debug.Log($"FinalCinematica: Animación del rayo finalizada ({duracionRayo}s). Cargando automáticamente '{escenaSiguiente}'...");
            SceneManager.LoadScene(escenaSiguiente, LoadSceneMode.Single);
            yield break;
        }

        if (devolverControlAlFinal)
        {
            jugador.transform.rotation = Quaternion.identity;
            BloquearJugador(false);
        }
    }

    private IEnumerator CaminarJugador(float destinoX)
    {
        float dir = Mathf.Sign(destinoX - rbJugador.position.x);
        jugador.transform.rotation = dir < 0f ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

        while (Mathf.Abs(destinoX - rbJugador.position.x) > 0.05f)
        {
            PonerAnimacion(1f);
            rbJugador.linearVelocity = new Vector2(dir * velocidadJugador, rbJugador.linearVelocity.y);
            yield return new WaitForFixedUpdate();
            if (Mathf.Sign(destinoX - rbJugador.position.x) != dir) break;
        }

        rbJugador.linearVelocity = new Vector2(0f, rbJugador.linearVelocity.y);
        rbJugador.position = new Vector2(destinoX, rbJugador.position.y);
        PonerAnimacion(0f);
    }

    // Desactivar Movimiento y Player_Attack bloquea caminar, saltar y atacar,
    // y evita que TextManagerNun devuelva el control al terminar la charla.
    private void BloquearJugador(bool bloquear)
    {
        if (movimiento != null) movimiento.enabled = !bloquear;
        if (ataque != null) ataque.enabled = !bloquear;

        if (bloquear)
        {
            rbJugador.linearVelocity = new Vector2(0f, rbJugador.linearVelocity.y);
            PonerAnimacion(0f);
        }
    }

    private void PonerAnimacion(float caminar)
    {
        if (animJugador == null) return;
        animJugador.SetBool("ensuelo", true);
        animJugador.SetFloat("Caminar", caminar);
    }
}
