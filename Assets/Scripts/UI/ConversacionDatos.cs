using System;
using UnityEngine;

namespace RPG.UI
{
    /// <summary>A que parte del juego pertenece una conversacion o interfaz.</summary>
    public enum ParteJuego
    {
        Escena1_JuegoPueblo = 1,
        Escena2_PuebloManana = 2,
        Escena3_Bosque = 3,
        Boss_Final = 4
    }

    [Serializable]
    public class LineaConversacion
    {
        public string hablante;
        [TextArea(2, 5)] public string texto;
    }

    /// <summary>
    /// Una conversacion completa ("Conversacion 1, 2, 3...") guardada en un GameObject para verla y editarla en el Inspector.
    /// Todos los textos del juego quedan reunidos en la escena Interfaz, agrupados por la parte del juego a la que pertenecen.
    /// </summary>
    public class ConversacionDatos : MonoBehaviour
    {
        [Header("Identificacion")]
        public int numero;
        public string titulo;
        public ParteJuego parte;
        [Tooltip("Charla con PNJ, Cinematica, Final...")] public string tipo = "Charla con PNJ";
        [Tooltip("De donde salen los textos: JSON de la interfaz o el ScriptableObject que ya usa el juego.")] public string fuente;
        [Tooltip("Id usado por las banderas del juego (por ejemplo helena_noche).")] public string id;

        [Header("Texto")]
        public LineaConversacion[] lineas;

        [Header("Eleccion al final (opcional)")]
        public string preguntaFinal;
        public string[] opcionesFinal;

        public bool TieneEleccion { get { return opcionesFinal != null && opcionesFinal.Length > 0; } }
    }
}
