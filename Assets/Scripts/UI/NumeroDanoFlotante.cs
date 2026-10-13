using UnityEngine;
using TMPro;

namespace RPG.UI
{
    /// <summary>
    /// Controla la animación (subida y desvanecimiento) del número de daño sobre un personaje.
    /// POR QUÉ: Encapsula el ciclo de vida del prefab instanciado para que se destruya solo 
    /// sin intervención de GestorDanoFlotante.
    /// </summary>
    public class NumeroDanoFlotante : MonoBehaviour
    {
        [Header("Configuración Visual")]
        public TextMeshProUGUI textoDano;
        public Color colorDano = new Color32(0xC8, 0x30, 0x15, 255);
        public TMP_FontAsset fuenteTitulos;

        [Header("Animación")]
        [Tooltip("Pixeles por segundo (Asumiendo Screen Space - Overlay)")]
        public float velocidadSubida = 50f;
        public float tiempoVida = 1.2f;

        private float tiempoActual = 0f;
        private Color colorBase;

        private void Awake()
        {
            tiempoVida = Mathf.Max(tiempoVida, 0.01f);
            if (textoDano == null) textoDano = GetComponentInChildren<TextMeshProUGUI>();
            if (textoDano != null)
            {
                if (fuenteTitulos != null) textoDano.font = fuenteTitulos;
                textoDano.color = colorDano;
                colorBase = textoDano.color;
            }
        }

        public void Inicializar(int cantidad)
        {
            if (textoDano != null)
            {
                textoDano.text = $"-{cantidad}";
            }
            Destroy(gameObject, tiempoVida);
        }

        private void Update()
        {
            transform.position += Vector3.up * velocidadSubida * Time.deltaTime;
            
            if (textoDano != null)
            {
                tiempoActual += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, tiempoActual / tiempoVida);
                textoDano.color = new Color(colorBase.r, colorBase.g, colorBase.b, alpha);
            }
        }
    }
}
