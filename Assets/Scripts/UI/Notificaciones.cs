using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Muestra notificaciones temporales en pantalla (Toasts de objetos y Banners de Pifia).
    /// POR QUÉ: Aisla el comportamiento visual transitorio del resto del HUD, usando una cola para evitar solapamientos.
    /// </summary>
    public class Notificaciones : MonoBehaviour
    {
        [Header("Toast Recolección")]
        public GameObject panelToast;
        public TextMeshProUGUI textoToast;
        public float duracionToast = 2f;
        public int toastsMaximos = 5;

        [Header("Banner Pifia")]
        public GameObject panelPifia;
        public TextMeshProUGUI textoPifia;
        public UnityEngine.UI.Image fondoPifia;
        public Color colorPifia = new Color32(0x52, 0x22, 0x5E, 255); // #52225E
        public float duracionPifia = 1.5f;

        [Header("Tipografía")]
        public TMP_FontAsset fuenteTitulos; // Para Pifia
        public TMP_FontAsset fuenteCuerpo;  // Para Toast

        private Queue<string> colaToasts = new Queue<string>();
        private bool mostrandoToast = false;
        private Coroutine corrutinaPifia;

        private void Awake()
        {
            if (panelToast != null) panelToast.SetActive(false);
            if (panelPifia != null) panelPifia.SetActive(false);

            if (fuenteTitulos != null && textoPifia != null) textoPifia.font = fuenteTitulos;
            if (fuenteCuerpo != null && textoToast != null) textoToast.font = fuenteCuerpo;

            // Aplicar color de pifia al banner
            if (fondoPifia != null) fondoPifia.color = colorPifia;
            if (textoPifia != null) textoPifia.color = Color.white;
        }

        private void OnEnable()
        {
            EventosJuego.OnObjetoRecogido += EncolarToastRecoleccion;
            EventosJuego.OnPifia += ActivarPifia;
        }

        private void OnDisable()
        {
            EventosJuego.OnObjetoRecogido -= EncolarToastRecoleccion;
            EventosJuego.OnPifia -= ActivarPifia;

            StopAllCoroutines();
            colaToasts.Clear();
            mostrandoToast = false;
            if (panelToast != null) panelToast.SetActive(false);
            if (panelPifia != null) panelPifia.SetActive(false);
            corrutinaPifia = null;
        }

        private void EncolarToastRecoleccion(string nombre, int cantidad)
        {
            colaToasts.Enqueue($"+{cantidad} {nombre}");
            
            // Límite de toasts para evitar sobrecarga de memoria visual
            if (colaToasts.Count > toastsMaximos)
            {
                colaToasts.Dequeue();
            }

            if (!mostrandoToast)
            {
                StartCoroutine(ProcesarColaToasts());
            }
        }

        private IEnumerator ProcesarColaToasts()
        {
            mostrandoToast = true;
            while (colaToasts.Count > 0)
            {
                string mensaje = colaToasts.Dequeue();
                if (panelToast != null && textoToast != null)
                {
                    textoToast.text = mensaje;
                    panelToast.SetActive(true);
                    
                    // Usar WaitForSecondsRealtime para que la pausa del juego no congele el toast
                    yield return new WaitForSecondsRealtime(duracionToast);
                    
                    panelToast.SetActive(false);
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
            mostrandoToast = false;
        }

        private void ActivarPifia(string quien)
        {
            if (string.IsNullOrEmpty(quien)) return;
            if (panelPifia != null && textoPifia != null)
            {
                textoPifia.text = $"!! PIFIA DE {quien.ToUpper()} !!";
                if (corrutinaPifia != null) StopCoroutine(corrutinaPifia);
                panelPifia.SetActive(true);
                corrutinaPifia = StartCoroutine(OcultarPifia());
            }
        }

        private IEnumerator OcultarPifia()
        {
            yield return new WaitForSecondsRealtime(duracionPifia);
            if (panelPifia != null) panelPifia.SetActive(false);
            corrutinaPifia = null;
        }
    }
}
