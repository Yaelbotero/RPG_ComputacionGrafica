using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace RPG.Flujo
{
    /// <summary>
    /// Gestiona las transiciones entre escenas con un efecto de fundido a negro (fade).
    /// Recuerda la escena anterior.
    /// </summary>
    public class GestorEscenas : MonoBehaviour
    {
        public static GestorEscenas Instancia { get; private set; }

        [Header("UI Transición")]
        public CanvasGroup panelFade;
        public float duracionFade = 0.5f;

        public string EscenaAnterior { get; private set; } = "";
        
        private bool enTransicion = false;

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            // POR QUÉ: GestorEscenas debe sobrevivir las transiciones para gestionar el fade y no perder su referencia al CanvasGroup.
            DontDestroyOnLoad(gameObject);

            if (panelFade != null)
            {
                panelFade.alpha = 0f;
                panelFade.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Inicia la carga asíncrona de una escena con efecto visual de fade.
        /// </summary>
        public void CargarEscena(string nombreEscena)
        {
            if (enTransicion) return;

            if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                Debug.LogError($"La escena '{nombreEscena}' no puede ser cargada. ¿Está en los Build Settings?");
                return;
            }

            StartCoroutine(TransicionEscena(nombreEscena));
        }

        private IEnumerator TransicionEscena(string nuevaEscena)
        {
            enTransicion = true;
            float duracionSegura = Mathf.Max(duracionFade, 0.01f);

            // 1. Oscurecer la pantalla
            if (panelFade != null)
            {
                panelFade.blocksRaycasts = true;
                float tiempo = 0;
                while (tiempo < duracionSegura)
                {
                    tiempo += Time.unscaledDeltaTime;
                    panelFade.alpha = Mathf.Lerp(0f, 1f, tiempo / duracionSegura);
                    yield return null;
                }
                panelFade.alpha = 1f;
            }

            // POR QUÉ: Permite a los sistemas (como GameManager o Combate) saber de dónde venimos para ajustar posiciones o lógica.
            EscenaAnterior = SceneManager.GetActiveScene().name;

            // 2. Cargar la nueva escena
            AsyncOperation operacion = SceneManager.LoadSceneAsync(nuevaEscena);
            if (operacion == null)
            {
                enTransicion = false;
                yield break;
            }

            while (!operacion.isDone)
            {
                yield return null;
            }

            EventosJuego.NotificarEscenaCargada(nuevaEscena);

            // 3. Aclarar la pantalla
            if (panelFade != null)
            {
                float tiempo = 0;
                while (tiempo < duracionSegura)
                {
                    tiempo += Time.unscaledDeltaTime;
                    panelFade.alpha = Mathf.Lerp(1f, 0f, tiempo / duracionSegura);
                    yield return null;
                }
                panelFade.alpha = 0f;
                panelFade.blocksRaycasts = false;
            }

            enTransicion = false;
        }
    }
}
