using UnityEngine;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Escucha eventos de daño y genera un prefab con el número de daño en la UI.
    /// POR QUÉ: Desacopla la lógica de combate de la animación visual del texto de daño.
    /// NOTA: Se asume Canvas Overlay y que 'contenedorDano' es un RectTransform.
    /// </summary>
    public class GestorDanoFlotante : MonoBehaviour
    {
        public GameObject prefabDano;
        public Transform contenedorDano; // Un panel en el Canvas (RectTransform)

        [Header("Posiciones de Destino (UI)")]
        public Transform posicionHeroe;
        public Transform posicionLupus;
        public Transform posicionHelena;
        public Transform posicionEnemigo;

        private void OnEnable()
        {
            EventosJuego.OnDanoRecibido += GenerarDano;
        }

        private void OnDisable()
        {
            EventosJuego.OnDanoRecibido -= GenerarDano;
        }

        private void GenerarDano(string objetivo, int cantidad)
        {
            if (prefabDano == null || contenedorDano == null) return;
            if (string.IsNullOrEmpty(objetivo)) return;

            Transform targetTransform = null;
            switch (objetivo.ToLower())
            {
                case "heroe": case "jugador": targetTransform = posicionHeroe; break;
                case "lupus": targetTransform = posicionLupus; break;
                case "helena": targetTransform = posicionHelena; break;
                default: targetTransform = posicionEnemigo; break;
            }

            if (targetTransform != null)
            {
                GameObject obj = Instantiate(prefabDano, targetTransform.position, Quaternion.identity, contenedorDano);
                NumeroDanoFlotante numObj = obj.GetComponent<NumeroDanoFlotante>();
                if (numObj != null)
                {
                    numObj.Inicializar(cantidad);
                }
            }
        }
    }
}
