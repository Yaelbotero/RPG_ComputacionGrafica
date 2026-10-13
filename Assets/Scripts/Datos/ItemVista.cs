using UnityEngine;

namespace RPG.Datos
{
    /// <summary>
    /// Estructura de datos simple para transferir información del inventario a la vista (UI).
    /// POR QUÉ: Se ubica en Datos para evitar dependencia circular entre Flujo y UI.
    /// </summary>
    [System.Serializable]
    public class ItemVista
    {
        public string nombre;
        public int cantidad;
        public Sprite icono;
    }
}
