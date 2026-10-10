using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int cantMonedas = 0;

    private TextManager textManager;

    public bool atacando = false;

    public Text txtmoneda, txtPersonaje;

    // Start is called before the first frame update
void Start()
    {
        textManager = FindAnyObjectByType<TextManager>();

        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (txtmoneda != null)
        {
            txtmoneda.text = cantMonedas.ToString();
        }

        if (txtPersonaje != null)
        {
            txtPersonaje.text = "";
        }

        StartCoroutine(CinematicaIntroduccion());
    }

    [Header("Cinemática de introducción")]
    [SerializeField] private GameObject monja;
    [SerializeField] private NunController nunController;
    [SerializeField] private GameObject jugador;
    [SerializeField] private MonoBehaviour movimientoJugador;
    [SerializeField] private float esperaInicial = 10f;
    [SerializeField, Min(0f)] private float esperaAntesDeBuscarNun = 0f;

    [SerializeField] private float velocidadJugadorCinematica = 2f;

    private static readonly Vector2 PosicionPuertaMonja = new Vector2(-4.37f, -3.169f);
    private static readonly Vector2 DestinoMonja = new Vector2(2.59f, -3.169f);
    private static readonly Vector2 PosicionPuertaJugador = new Vector2(-4.37f, -2.975f);
    private static readonly Vector2 DestinoJugador = new Vector2(0.79f, -2.975f);

    private IEnumerator CinematicaIntroduccion()
    {
        // Fase 0: ocultar a ambos personajes y bloquear el control habitual.
        if (monja != null)
        {
            monja.SetActive(false);
        }

        if (jugador != null)
        {
            jugador.SetActive(false);
        }

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = false;
        }

        // Fase 1: esperar y hacer aparecer a la monja en la puerta del bar.
        yield return new WaitForSeconds(esperaInicial);

        if (monja != null)
        {
            monja.transform.position = new Vector3(PosicionPuertaMonja.x, PosicionPuertaMonja.y, monja.transform.position.z);
            monja.SetActive(true);
        }

        // Fase 2: hacer caminar a la monja hasta su destino.
        if (nunController != null)
        {
            nunController.IniciarCaminata(DestinoMonja);
            yield return new WaitUntil(() => nunController.CaminataTerminada);
        }

        // Fase 3: esperar a que el diálogo avance antes de sacar al jugador.
        yield return new WaitUntil(() => textManager == null || textManager.getLineIndex() >= 8);

        // Mantener el tiempo configurable del Inspector después de alcanzar la línea requerida.
        yield return new WaitForSeconds(esperaAntesDeBuscarNun);

        if (jugador != null)
        {
            jugador.transform.position = new Vector3(PosicionPuertaJugador.x, PosicionPuertaJugador.y, jugador.transform.position.z);
            jugador.SetActive(true);
        }

        PrepararAnimacionJugador();

        // Fase 4: mover al jugador automaticamente hasta quedar junto a la monja.
        yield return StartCoroutine(MoverJugadorEnCinematica());

        // Fase 5: mantener la escena unos segundos y devolver el control al jugador.
        yield return new WaitForSeconds(3f);

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = true;
        }
    }

    private void PrepararAnimacionJugador()
    {
        if (jugador == null)
        {
            return;
        }

        Animator animator = jugador.GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        if (TieneParametro(animator, "ensuelo", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("ensuelo", true);
        }

        if (TieneParametro(animator, "Caminar", AnimatorControllerParameterType.Float))
        {
            animator.SetFloat("Caminar", 1f);
        }

        int estadoCaminar = Animator.StringToHash("Base Layer.Player_Run");

        if (animator.HasState(0, estadoCaminar) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash != estadoCaminar)
        {
            animator.Play(estadoCaminar, 0, 0f);
        }
    }

    private void DetenerAnimacionJugador()
    {
        if (jugador == null)
        {
            return;
        }

        Animator animator = jugador.GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        if (TieneParametro(animator, "Caminar", AnimatorControllerParameterType.Float))
        {
            animator.SetFloat("Caminar", 0f);
        }

        if (TieneParametro(animator, "ensuelo", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("ensuelo", true);
        }

        if (animator.HasState(0, Animator.StringToHash("Base Layer.Player_Idle")))
        {
            animator.Play("Base Layer.Player_Idle", 0, 0f);
        }
    }

    private bool TieneParametro(Animator animator, string nombre, AnimatorControllerParameterType tipo)
    {
        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.name == nombre && parametro.type == tipo)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator MoverJugadorEnCinematica()
    {
        if (jugador == null)
        {
            yield break;
        }

        while (Vector2.Distance(jugador.transform.position, DestinoJugador) > 0.01f)
        {
            if (textManager != null && textManager.getLineIndex() < 8)
            {
                yield return null;
                continue;
            }

            PrepararAnimacionJugador();

            Vector2 posicion = Vector2.MoveTowards(jugador.transform.position, DestinoJugador, velocidadJugadorCinematica * Time.deltaTime);
            jugador.transform.position = new Vector3(posicion.x, posicion.y, jugador.transform.position.z);
            yield return null;
        }

        jugador.transform.position = new Vector3(DestinoJugador.x, DestinoJugador.y, jugador.transform.position.z);
        DetenerAnimacionJugador();
    }

    public void SetMonedas()
    {
        cantMonedas++;

        if (txtmoneda != null)
        {
            txtmoneda.text = cantMonedas.ToString();
        }

        if (txtPersonaje != null)
        {
            txtPersonaje.text = "He cogido " + cantMonedas + " monedas.";
        }
    }
}
