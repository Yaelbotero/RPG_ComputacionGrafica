using UnityEngine;
using System.Collections;
using TMPro;

public class TextManagerNun : MonoBehaviour
{
    [SerializeField] private GameObject señalDialogo;
    [SerializeField] private GameObject dialogoPanel;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private DialogoNPC dialogo;
    [SerializeField] private TextManager barista;
    [SerializeField] private DialogoNPC dialogoNoche;
    // Respaldo: se usa solo si no hay un DialogoNPC asignado.
    [SerializeField, TextArea(4,6)]private string[] lineasDeDialogo;
    private float tiempoEscritura = 0.05f;
    private bool elJugadorEstaEnRango = false;
    private bool elJugadorEstaHablando = false;
    private int indiceLinea = 0;
    private Transform jugadorEnRango;
    private Movimiento movimientoJugador;
    private SpriteRenderer spriteRendererMonja;
    [SerializeField] private Vector2 destinoAlTerminar = new Vector2(9.45f, -3.171f);
    [SerializeField] private bool caminarAlTerminar = true;
    private NunController nunController;
    private bool yaSeFue = false;

    private void Awake()
    {
        spriteRendererMonja = GetComponent<SpriteRenderer>();
        nunController = GetComponent<NunController>();

        if (dialogo != null && dialogo.lineas != null && dialogo.lineas.Length > 0)
        {
            lineasDeDialogo = dialogo.lineas;
        }
    }

    private void Update()
    {
        if (elJugadorEstaEnRango && Input.GetKeyDown(KeyCode.E))
        {
            if (!elJugadorEstaHablando)
            {
                MirarAlJugador();
                StartDialogo();
            }
            else if (textoDialogo != null && textoDialogo.text != lineasDeDialogo[indiceLinea])
            {
                StopAllCoroutines();
                textoDialogo.text = lineasDeDialogo[indiceLinea];
                Debug.Log("Se detuvo la escritura y se mostró la línea completa.");
            }
            else
            {
                SiguienteLinea();
                Debug.Log("Se pasó a la siguiente línea del diálogo.");
            }
            if (indiceLinea >= lineasDeDialogo.Length)
            {
                elJugadorEstaHablando = false;
                BloquearJugador(false);
                if (caminarAlTerminar)
                {
                    MirarAlLadoOpuesto();
                    CaminarAlDestino();
                }
            }
        }
    }

    // Al terminar la conversacion la monja camina (con animacion) hasta el destino.
    private void CaminarAlDestino()
    {
        if (yaSeFue || nunController == null)
        {
            return;
        }

        yaSeFue = true;
        if (spriteRendererMonja != null)
        {
            spriteRendererMonja.flipX = destinoAlTerminar.x < transform.position.x;
        }
        nunController.IniciarCaminata(destinoAlTerminar);
        StartCoroutine(AvisarAlLlegar());
    }

    private IEnumerator AvisarAlLlegar()
    {
        yield return new WaitUntil(() => nunController.CaminataTerminada);
        PlayerManager.puertaHabilitada = true;
        if (barista != null && dialogoNoche != null)
        {
            barista.IniciarDialogo(dialogoNoche);
        }
    }

    // Bloquea o libera el movimiento y el ataque del jugador durante la conversacion.
    private void BloquearJugador(bool bloquear)
    {
        if (movimientoJugador != null)
        {
            movimientoJugador.bloqueado = bloquear;
        }
    }

    private void StartDialogo()
    {
        if (dialogoPanel == null || textoDialogo == null || lineasDeDialogo == null || lineasDeDialogo.Length == 0)
        {
            return;
        }

        elJugadorEstaHablando = true;
        BloquearJugador(true);
        dialogoPanel.SetActive(true);
        if (señalDialogo != null)
        {
            señalDialogo.SetActive(false);
        }

        indiceLinea = 0;
        StartCoroutine(MostrarLinea());
    }

    private void SiguienteLinea()
    {
        indiceLinea++;
        if (indiceLinea < lineasDeDialogo.Length)
        {
            StartCoroutine(MostrarLinea());
        }
        else
        {
            elJugadorEstaHablando = false;
            BloquearJugador(false);

            if (dialogoPanel != null)
            {
                dialogoPanel.SetActive(false);
            }

            if (señalDialogo != null && elJugadorEstaEnRango)
            {
                señalDialogo.SetActive(true);
            }
        }
    }

    private IEnumerator MostrarLinea()
    {
        if (textoDialogo == null || lineasDeDialogo == null || indiceLinea < 0 || indiceLinea >= lineasDeDialogo.Length)
        {
            yield break;
        }

        textoDialogo.text = string.Empty;

        foreach (char letra in lineasDeDialogo[indiceLinea])
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(tiempoEscritura);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            elJugadorEstaEnRango = true;
            jugadorEnRango = collision.transform;
            movimientoJugador = collision.GetComponentInParent<Movimiento>();

            if (señalDialogo != null && !elJugadorEstaHablando)
            {
                señalDialogo.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            elJugadorEstaEnRango = false;
            jugadorEnRango = null;

            if (señalDialogo != null)
            {
                señalDialogo.SetActive(false);
            }
        }
    }

    private void MirarAlJugador()
    {
        if (jugadorEnRango == null || spriteRendererMonja == null)
        {
            return;
        }

        spriteRendererMonja.flipX = jugadorEnRango.position.x < transform.position.x;
    }

    private void MirarAlLadoOpuesto()
    {
        if (spriteRendererMonja == null)
        {
            return;
        }

        spriteRendererMonja.flipX = !spriteRendererMonja.flipX;
    }
}
