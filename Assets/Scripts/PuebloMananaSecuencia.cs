using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Secuencia exclusiva de la escena "Pueblo Mañana". Se coloca en Nun; no es usada por "Juego Pueblo".
/// </summary>
public class PuebloMananaSecuencia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NunController nun;
    [SerializeField] private SpriteRenderer nunSprite;
    [SerializeField] private GameObject jugador;
    [SerializeField] private GameObject senalDialogo;
    [SerializeField] private GameObject dialogoPanel;
    [SerializeField] private TMP_Text textoDialogo;

    [Header("Posiciones")]
    [SerializeField] private Vector2 nunInicio = new Vector2(9.8f, -3.171f);
    [SerializeField] private Vector2 nunEncuentro = new Vector2(1.85f, -3.171f);
    [SerializeField] private Vector2 jugadorPuerta = new Vector2(-4.28f, -2.975f);
    [SerializeField] private Vector2 jugadorEncuentro = new Vector2(0.08f, -2.975f);
    [SerializeField] private Vector2 nunFinal = new Vector2(12.6f, -3.171f);
    [SerializeField] private Vector2 jugadorFinal = new Vector2(11.05f, -2.975f);

    [Header("Ajustes")]
    [SerializeField] private float esperaInicial = 1f;
    [SerializeField] private float velocidadJugador = 4f;
    [SerializeField] private float retrasoJugadorAlSalir = 1f;
    [SerializeField] private float tiempoEscritura = 0.04f;
    [SerializeField] private GameObject canvasFinal;
    [SerializeField] private float retrasoCanvasFinal = 2f;
    [SerializeField] private string[] lineas =
    {
        "PROTA: Hola, muy buenos días, iniciemos el viaje.",
        "HELENA: Por supuesto."
    };

    private enum Fase { Inicio, EsperandoInteraccion, Dialogo, Final }

    private Fase fase = Fase.Inicio;
    private bool jugadorEnRango;
    private int indice;
    private Coroutine escritura;
    private Movimiento movimiento;
    private Rigidbody2D rbJugador;
    private Animator animJugador;

    private void Start()
    {
        movimiento = jugador != null ? jugador.GetComponent<Movimiento>() : null;
        rbJugador = jugador != null ? jugador.GetComponent<Rigidbody2D>() : null;
        animJugador = jugador != null ? jugador.GetComponent<Animator>() : null;

        if (nun == null || jugador == null || rbJugador == null)
        {
            Debug.LogError("PuebloMananaSecuencia: faltan referencias (nun/jugador/Rigidbody2D).");
            enabled = false;
            return;
        }

        if (movimiento != null) movimiento.enabled = false;
        if (senalDialogo != null) senalDialogo.SetActive(false);
        if (dialogoPanel != null) dialogoPanel.SetActive(false);
        jugador.SetActive(false);

        transform.position = new Vector3(nunInicio.x, nunInicio.y, transform.position.z);
        if (nunSprite != null) nunSprite.flipX = false;

        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        yield return new WaitForSeconds(esperaInicial);

        // Nun camina hacia la izquierda, de cara al bar.
        if (nunSprite != null) nunSprite.flipX = true;
        nun.IniciarCaminata(nunEncuentro);
        yield return new WaitUntil(() => nun.CaminataTerminada);

        jugador.transform.position = new Vector3(jugadorPuerta.x, jugadorPuerta.y, jugador.transform.position.z);
        jugador.transform.rotation = Quaternion.identity;
        jugador.SetActive(true);
        yield return CorrerJugador(jugadorEncuentro);

        fase = Fase.EsperandoInteraccion;
        if (jugadorEnRango && senalDialogo != null) senalDialogo.SetActive(true);
    }

    private IEnumerator CorrerJugador(Vector2 destino)
    {
        float dir = Mathf.Sign(destino.x - rbJugador.position.x);
        if (dir < 0f) jugador.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else jugador.transform.rotation = Quaternion.identity;

        while (Mathf.Abs(destino.x - rbJugador.position.x) > 0.05f)
        {
            PonerAnimacion(1f);
            rbJugador.linearVelocity = new Vector2(dir * velocidadJugador, rbJugador.linearVelocity.y);
            yield return new WaitForFixedUpdate();
            if (Mathf.Sign(destino.x - rbJugador.position.x) != dir) break;
        }

        rbJugador.linearVelocity = new Vector2(0f, rbJugador.linearVelocity.y);
        rbJugador.position = new Vector2(destino.x, rbJugador.position.y);
        PonerAnimacion(0f);
    }

    private void PonerAnimacion(float caminar)
    {
        if (animJugador == null) return;
        animJugador.SetBool("ensuelo", true);
        animJugador.SetFloat("Caminar", caminar);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (fase == Fase.EsperandoInteraccion && jugadorEnRango)
        {
            IniciarDialogo();
        }
        else if (fase == Fase.Dialogo)
        {
            AvanzarDialogo();
        }
    }

    private void IniciarDialogo()
    {
        fase = Fase.Dialogo;
        if (senalDialogo != null) senalDialogo.SetActive(false);
        if (dialogoPanel != null) dialogoPanel.SetActive(true);
        if (nunSprite != null) nunSprite.flipX = jugador.transform.position.x < transform.position.x;
        indice = 0;
        MostrarLinea();
    }

    private void AvanzarDialogo()
    {
        if (textoDialogo != null && textoDialogo.text != lineas[indice])
        {
            if (escritura != null) StopCoroutine(escritura);
            escritura = null;
            textoDialogo.text = lineas[indice];
            return;
        }

        indice++;
        if (indice < lineas.Length)
        {
            MostrarLinea();
        }
        else
        {
            fase = Fase.Final;
            if (dialogoPanel != null) dialogoPanel.SetActive(false);
            StartCoroutine(Partir());
        }
    }

    private void MostrarLinea()
    {
        if (escritura != null) StopCoroutine(escritura);
        escritura = StartCoroutine(Escribir(lineas[indice]));
    }

    private IEnumerator Escribir(string linea)
    {
        if (textoDialogo == null) yield break;
        textoDialogo.text = string.Empty;
        foreach (char letra in linea)
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(tiempoEscritura);
        }
        escritura = null;
    }

    private IEnumerator Partir()
    {
        // Primero Nun, después el personaje.
        if (nunSprite != null) nunSprite.flipX = nunFinal.x < transform.position.x;
        nun.IniciarCaminata(nunFinal);
        yield return new WaitForSeconds(retrasoJugadorAlSalir);
        yield return CorrerJugador(jugadorFinal);
        yield return new WaitUntil(() => nun.CaminataTerminada);
        if (movimiento != null) movimiento.enabled = true;

        yield return new WaitForSeconds(retrasoCanvasFinal);
        if (canvasFinal != null) canvasFinal.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other) => ActualizarRango(other, true);
    private void OnTriggerStay2D(Collider2D other) { if (!jugadorEnRango) ActualizarRango(other, true); }
    private void OnTriggerExit2D(Collider2D other) => ActualizarRango(other, false);

    private void ActualizarRango(Collider2D other, bool dentro)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnRango = dentro;
        if (senalDialogo != null && fase == Fase.EsperandoInteraccion) senalDialogo.SetActive(dentro);
    }
}
