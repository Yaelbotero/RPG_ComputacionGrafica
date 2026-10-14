using UnityEngine;
using System.Collections;
using TMPro;

// Dialogo del gato: igual que TextManagerNun pero sin caminata final (el gato no se mueve).
public class TextManagerGato : MonoBehaviour
{
    [SerializeField] private GameObject señalDialogo;
    [SerializeField] private GameObject dialogoPanel;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private DialogoNPC dialogo;
    // Respaldo: se usa solo si no hay un DialogoNPC asignado.
    [SerializeField, TextArea(4, 6)] private string[] lineasDeDialogo;
    [SerializeField] private float tiempoEscritura = 0.05f;
    [SerializeField] private bool mirarAlJugador = true;
    [SerializeField] private bool invertirMirada = false;

    private bool elJugadorEstaEnRango;
    private bool elJugadorEstaHablando;
    private int indiceLinea;
    private Transform jugadorEnRango;
    private Movimiento movimientoJugador;
    private SpriteRenderer spriteRendererGato;

    private void Awake()
    {
        spriteRendererGato = GetComponent<SpriteRenderer>();

        if (dialogo != null && dialogo.lineas != null && dialogo.lineas.Length > 0)
        {
            lineasDeDialogo = dialogo.lineas;
        }
    }

    private void Update()
    {
        if (!elJugadorEstaEnRango || !Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (!elJugadorEstaHablando)
        {
            MirarAlJugador();
            StartDialogo();
        }
        else if (textoDialogo != null && textoDialogo.text != lineasDeDialogo[indiceLinea])
        {
            StopAllCoroutines();
            textoDialogo.text = lineasDeDialogo[indiceLinea];
        }
        else
        {
            SiguienteLinea();
        }
    }

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
            return;
        }

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
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        elJugadorEstaEnRango = true;
        jugadorEnRango = collision.transform;
        movimientoJugador = collision.GetComponentInParent<Movimiento>();

        if (señalDialogo != null && !elJugadorEstaHablando)
        {
            señalDialogo.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        elJugadorEstaEnRango = false;
        jugadorEnRango = null;

        if (señalDialogo != null)
        {
            señalDialogo.SetActive(false);
        }
    }

    private void MirarAlJugador()
    {
        if (!mirarAlJugador || jugadorEnRango == null || spriteRendererGato == null)
        {
            return;
        }

        bool jugadorALaIzquierda = jugadorEnRango.position.x < transform.position.x;
        spriteRendererGato.flipX = invertirMirada ? !jugadorALaIzquierda : jugadorALaIzquierda;
    }
}
