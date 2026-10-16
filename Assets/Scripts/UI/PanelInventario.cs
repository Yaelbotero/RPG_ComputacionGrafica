using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPG.Flujo;
using RPG.Datos;

namespace RPG.UI
{
    /// <summary>
    /// Gestiona la visualización del inventario del jugador.
    /// POR QUÉ: Se usa un Canvas / Panel separado para mostrar de forma limpia los ítems. 
    /// Se controla mediante Input System directo en código para no depender de configuraciones en el proyecto.
    /// </summary>
    public class PanelInventario : MonoBehaviour
    {
        [Header("Referencias")]
        public GameObject panelPrincipal;
        public GameObject prefabItemUI; 
        public Transform contenedorItems;

        [Header("Tipografía")]
        public TMP_FontAsset fuenteCuerpo;

        private InputAction accionToggleI;
        private InputAction accionToggleTab;

        private void Awake()
        {
            if (panelPrincipal != null) panelPrincipal.SetActive(false);

            accionToggleI = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/i");
            accionToggleTab = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/tab");

            accionToggleI.performed += TogglePanel;
            accionToggleTab.performed += TogglePanel;
        }

        private void OnEnable()
        {
            accionToggleI.Enable();
            accionToggleTab.Enable();
            EventosJuego.OnInventarioCambiado += ActualizarInventario;
        }

        private void OnDisable()
        {
            accionToggleI.Disable();
            accionToggleTab.Disable();
            EventosJuego.OnInventarioCambiado -= ActualizarInventario;
        }

        private void OnDestroy()
        {
            accionToggleI.performed -= TogglePanel;
            accionToggleTab.performed -= TogglePanel;
            accionToggleI.Dispose();
            accionToggleTab.Dispose();
        }

        private void TogglePanel(InputAction.CallbackContext ctx)
        {
            if (panelPrincipal != null)
            {
                panelPrincipal.SetActive(!panelPrincipal.activeSelf);
            }
        }

        // Usa un objeto desde el inventario (solo cuando no hay un combate en curso: ahi se usa desde el menu ITEMS).
        private void UsarFueraDeCombate(string nombre)
        {
            if (CombateUI.Activo || PartyManager.Instance == null) return;
            PartyManager.Instance.UsarItem(nombre);
        }

        private void ActualizarInventario(List<ItemVista> items)
        {
            if (contenedorItems == null || prefabItemUI == null || items == null) return;

            // POR QUÉ: Buscamos recrear la lista visual desde cero para reflejar el estado actual exacto.
            foreach (Transform child in contenedorItems)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in items)
            {
                GameObject obj = Instantiate(prefabItemUI, contenedorItems);
                
                // POR QUÉ: Busca en los hijos para encontrar el componente de texto sin necesidad de un script específico en el prefab.
                TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    int cura = PartyManager.Instance != null ? PartyManager.Instance.CuracionDe(item.nombre) : 0;
                    txt.text = cura > 0 ? $"{item.nombre} x{item.cantidad}  (+{cura} HP)" : $"{item.nombre} x{item.cantidad}";
                    if (fuenteCuerpo != null) txt.font = fuenteCuerpo;
                }

                // Fuera de combate, al hacer clic en un objeto curativo se usa.
                if (PartyManager.Instance != null && PartyManager.Instance.CuracionDe(item.nombre) > 0)
                {
                    Button boton = obj.GetComponent<Button>();
                    if (boton == null) boton = obj.AddComponent<Button>();
                    Image fondo = obj.GetComponent<Image>();
                    if (fondo == null)
                    {
                        fondo = obj.AddComponent<Image>();
                        fondo.color = new Color(1f, 1f, 1f, 0f);
                    }
                    boton.targetGraphic = fondo;
                    string nombreItem = item.nombre;
                    boton.onClick.AddListener(() => UsarFueraDeCombate(nombreItem));
                }

                Transform iconoTransform = obj.transform.Find("Icono");
                if (iconoTransform != null)
                {
                    Image img = iconoTransform.GetComponent<Image>();
                    if (img != null && item.icono != null)
                    {
                        img.sprite = item.icono;
                    }
                }
            }
        }
    }
}
