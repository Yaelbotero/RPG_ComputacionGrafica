using System;

namespace RPG.Datos
{
    // QUE HACE: Estructura serializable para configurar dialogos.
    [Serializable]
    public class LineaDialogo
    {
        public string hablante;
        public string texto;
        public string retrato;
    }

    // QUE HACE: Estructura para almacenar las opciones al final de un dialogo.
    [Serializable]
    public class EleccionDialogo
    {
        public string pregunta;
        public string[] opciones;
    }

    // QUE HACE: Contenedor principal del archivo JSON completo.
    [Serializable]
    public class DatosDialogo
    {
        public string id;
        public LineaDialogo[] lineas;
        public EleccionDialogo eleccion;
    }
}
