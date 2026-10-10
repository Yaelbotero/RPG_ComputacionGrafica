using UnityEngine;
using System.Collections;
using TMPro;

public class TextManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogoPanel;
    [SerializeField] private TMP_Text dialogoText;
    [SerializeField, TextArea(4, 5)] private string[] lineasDialogo;

    private float tiempoEscritura = 0.05f;
    private bool enDialogo;
    private bool dialogoTerminado;
    private int lineIndex;
    private Coroutine escrituraActual;

    private void Start()
    {
    }

    public int getLineIndex()
    {
        return lineIndex;
    }

    private void SiguienteLinea()
    {
        lineIndex++;
        if (lineIndex < lineasDialogo.Length)
        {
            escrituraActual = StartCoroutine(ShowLine());
        }
        else
        {
            dialogoTerminado = true;
            enDialogo = false;
            if (dialogoPanel != null)
            {
                dialogoPanel.SetActive(false);
            }
        }
    }

    private IEnumerator ShowLine()
    {
        dialogoText.text = string.Empty;

        foreach (char ch in lineasDialogo[lineIndex])
        {
            dialogoText.text += ch;
            yield return new WaitForSeconds(tiempoEscritura);
        }

        escrituraActual = null;
    }

    private void Update()
    {
        if (!enDialogo && !dialogoTerminado)
        {
            if (dialogoPanel == null || dialogoText == null || lineasDialogo == null || lineasDialogo.Length == 0)
            {
                dialogoTerminado = true;
                return;
            }

            enDialogo = true;
            dialogoPanel.SetActive(true);
            lineIndex = 0;
            escrituraActual = StartCoroutine(ShowLine());
            return;
        }

        if (!enDialogo || dialogoTerminado || dialogoText == null || lineasDialogo == null || lineIndex < 0 || lineIndex >= lineasDialogo.Length)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (dialogoText.text != lineasDialogo[lineIndex])
        {
            if (escrituraActual != null)
            {
                StopCoroutine(escrituraActual);
                escrituraActual = null;
            }

            dialogoText.text = lineasDialogo[lineIndex];
            return;
        }

        SiguienteLinea();
    }
}
