using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG.Dialogos
{
    // QUE HACE: Script que controla visualmente la caja de dialogo.
    // POR QUE: Separamos la vista (CajaDialogo) del controlador (GestorDialogos) para mantener modularidad.
    public class CajaDialogo : MonoBehaviour
    {
        [Header("Componentes UI")]
        public GameObject panel;
        public TextMeshProUGUI textoNombre;
        public TextMeshProUGUI textoDialogo;
        public Image imagenRetrato;
        public GameObject flechaContinuar;
        public Transform contenedorOpciones;

        private void Awake()
        {
            // POR QUE transform.Find: Buscar los hijos por nombre permite reorganizar UI sin romper referencias.
            if (panel == null) panel = transform.Find("Panel")?.gameObject;
            if (textoNombre == null && panel != null) textoNombre = panel.transform.Find("Nombre")?.GetComponent<TextMeshProUGUI>();
            if (textoDialogo == null && panel != null) textoDialogo = panel.transform.Find("Texto")?.GetComponent<TextMeshProUGUI>();
            if (imagenRetrato == null && panel != null) imagenRetrato = panel.transform.Find("Retrato")?.GetComponent<Image>();
            if (flechaContinuar == null && panel != null) flechaContinuar = panel.transform.Find("Flecha")?.gameObject;
            if (contenedorOpciones == null && panel != null) contenedorOpciones = panel.transform.Find("Opciones");

            // POR QUE ocultar en Awake: Evitamos problemas de orden de ejecucion en Start.
            if (panel != null) panel.SetActive(false);
        }

        public void Mostrar(bool estado)
        {
            if (panel != null) panel.SetActive(estado);
        }

        public void Configurar(string nombre, Sprite retrato)
        {
            if (textoNombre != null) textoNombre.text = nombre;
            if (imagenRetrato != null)
            {
                if (retrato != null)
                {
                    imagenRetrato.sprite = retrato;
                    imagenRetrato.gameObject.SetActive(true);
                }
                else
                {
                    imagenRetrato.gameObject.SetActive(false);
                }
            }
        }

        public void MostrarFlecha(bool mostrar)
        {
            if (flechaContinuar != null) flechaContinuar.SetActive(mostrar);
        }

        public void LimpiarOpciones()
        {
            if (contenedorOpciones == null) return;
            foreach (Transform child in contenedorOpciones)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
