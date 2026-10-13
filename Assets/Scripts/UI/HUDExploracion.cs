using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Muestra la vida y el oro en las escenas de exploración.
    /// POR QUÉ: Aisla la UI de la lógica, actualizándose por eventos. Se usa transform.Find 
    /// en Awake para que el diseñador no tenga que asignar referencias a mano, evitando errores comunes.
    /// </summary>
    public class HUDExploracion : MonoBehaviour
    {
        [Header("Configuración Visual")]
        public Color colorVida = new Color32(0x2D, 0x80, 0x32, 255);
        public Color colorOro = new Color32(0x7A, 0x4E, 0x0A, 255);
        public TMP_FontAsset fuenteCuerpo;

        private TextMeshProUGUI textoVida;
        private Image barraVida;
        private TextMeshProUGUI textoOro;
        private GameObject promptInteraccion;

        private void Awake()
        {
            Transform tVida = transform.Find("TextoVida");
            if (tVida != null) textoVida = tVida.GetComponent<TextMeshProUGUI>();

            Transform tBarra = transform.Find("BarraVida");
            if (tBarra != null) barraVida = tBarra.GetComponent<Image>();

            Transform tOro = transform.Find("TextoOro");
            if (tOro != null) textoOro = tOro.GetComponent<TextMeshProUGUI>();

            Transform tPrompt = transform.Find("PromptInteraccion");
            if (tPrompt != null)
            {
                promptInteraccion = tPrompt.gameObject;
                promptInteraccion.SetActive(false);
            }

            AplicarEstilo();
        }

        private void AplicarEstilo()
        {
            if (fuenteCuerpo != null)
            {
                if (textoVida != null) textoVida.font = fuenteCuerpo;
                if (textoOro != null) textoOro.font = fuenteCuerpo;
            }

            if (barraVida != null) barraVida.color = colorVida;
            if (textoOro != null) textoOro.color = colorOro;
        }

        private void OnEnable()
        {
            EventosJuego.OnVidaCambiada += ActualizarVida;
            EventosJuego.OnOroCambiado += ActualizarOro;

            // POR QUÉ: Inicializa el HUD si se activa después de que el oro cambió.
            // Para la vida, los sistemas de combate/exploración deben re-notificar 
            // OnVidaCambiada al cargar la escena (OnEscenaCargada).
            if (RPG.Flujo.GameManager.Instancia != null)
            {
                ActualizarOro(RPG.Flujo.GameManager.Instancia.oroRecolectado);
            }
        }

        private void OnDisable()
        {
            EventosJuego.OnVidaCambiada -= ActualizarVida;
            EventosJuego.OnOroCambiado -= ActualizarOro;
        }

        private void ActualizarVida(string quien, int actual, int max)
        {
            if (!string.IsNullOrEmpty(quien) && (quien.ToLower() == "heroe" || quien.ToLower() == "jugador"))
            {
                if (textoVida != null) textoVida.text = $"HP: {actual}/{max}";
                if (barraVida != null && max > 0) barraVida.fillAmount = (float)actual / max;
            }
        }

        private void ActualizarOro(int oroActual)
        {
            if (textoOro != null) textoOro.text = $"Dinero: {oroActual}";
        }

        // POR QUÉ: Permite a los triggers del entorno (ej. carteles, NPCs) 
        // activar o desactivar el mensaje de interacción.
        public void MostrarPrompt(bool mostrar)
        {
            if (promptInteraccion != null) promptInteraccion.SetActive(mostrar);
        }
    }
}
