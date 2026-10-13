using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Dialogos;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Reproduce una ConversacionDatos sobre una CajaDialogo (interfaz de "hablando con un personaje").
    /// Estructuras de datos: Queue (lineas pendientes, FIFO), Stack (historial para retroceder con Q, LIFO)
    /// y Dictionary (color por hablante).
    /// Teclas: Espacio/Enter/E avanzan, Q retrocede, 1/2 eligen opcion.
    /// </summary>
    public class ReproductorConversacion : MonoBehaviour
    {
        public static ReproductorConversacion Actual { get; private set; }
        public static bool Activa { get { return Actual != null && Actual.enCurso; } }

        [Header("Vista")]
        public CajaDialogo caja;
        public TextMeshProUGUI textoNumero;
        public GameObject prefabBotonOpcion;
        public float velocidadEscritura = 0.02f;

        [Header("Conversaciones de este grupo")]
        public List<ConversacionDatos> conversaciones = new List<ConversacionDatos>();

        public event Action<ConversacionDatos, string> EleccionTomada;
        public event Action<ConversacionDatos> Terminada;

        private readonly Queue<LineaConversacion> pendientes = new Queue<LineaConversacion>();
        private readonly Stack<LineaConversacion> historial = new Stack<LineaConversacion>();
        private LineaConversacion linea;
        private ConversacionDatos conversacion;
        private bool enCurso;
        private bool escribiendo;
        private Coroutine escritura;
        private int cicloSiguiente;

        private static readonly Dictionary<string, Color> coloresHablante = new Dictionary<string, Color>
        {
            { "heroe", new Color32(20, 70, 140, 255) },
            { "prota", new Color32(20, 70, 140, 255) },
            { "helena", new Color32(110, 40, 140, 255) },
            { "jorge", new Color32(130, 70, 10, 255) },
            { "lupus", new Color32(150, 25, 20, 255) },
            { "narrador", new Color32(70, 60, 50, 255) }
        };

        private void OnEnable() { Actual = this; }
        private void OnDisable() { if (Actual == this) Actual = null; }

        public bool Reproducir(int numero)
        {
            foreach (ConversacionDatos c in conversaciones)
                if (c != null && c.numero == numero) return Reproducir(c);
            Debug.LogWarning("ReproductorConversacion: no existe la conversacion " + numero + " en este grupo.");
            return false;
        }

        /// <summary>Reproduce la siguiente conversacion de la lista (util para revisar todas una por una).</summary>
        public bool ReproducirSiguiente()
        {
            if (conversaciones.Count == 0 || enCurso) return false;
            cicloSiguiente = cicloSiguiente % conversaciones.Count;
            return Reproducir(conversaciones[cicloSiguiente++]);
        }

        public bool Reproducir(ConversacionDatos c)
        {
            if (enCurso || c == null || c.lineas == null || c.lineas.Length == 0 || caja == null) return false;

            conversacion = c;
            enCurso = true;
            Actual = this;
            pendientes.Clear();
            historial.Clear();
            foreach (LineaConversacion l in c.lineas) pendientes.Enqueue(l);

            if (textoNumero != null) textoNumero.text = "Conversación " + c.numero;
            EventosJuego.NotificarDialogoIniciado(string.IsNullOrEmpty(c.id) ? c.titulo : c.id);
            caja.Mostrar(true);
            MostrarSiguiente();
            return true;
        }

        private void Update()
        {
            if (!enCurso) return;

            if (EntradaUI.Confirmar()) Avanzar();
            else if (EntradaUI.Retroceder()) Retroceder();
            else if (EntradaUI.Tecla(KeyCode.Alpha1)) ElegirOpcion(0);
            else if (EntradaUI.Tecla(KeyCode.Alpha2)) ElegirOpcion(1);
        }

        private void Avanzar()
        {
            if (escribiendo)
            {
                if (escritura != null) StopCoroutine(escritura);
                caja.textoDialogo.maxVisibleCharacters = int.MaxValue;
                escribiendo = false;
                FinDeLinea();
                return;
            }

            // Con opciones en pantalla solo se avanza eligiendo una.
            if (caja.contenedorOpciones != null && caja.contenedorOpciones.childCount > 0) return;

            if (linea != null) historial.Push(linea);
            MostrarSiguiente();
        }

        private void Retroceder()
        {
            if (escribiendo || historial.Count == 0) return;

            // Se vuelve a poner en la cola la linea actual y la anterior (LIFO del historial).
            List<LineaConversacion> temporal = new List<LineaConversacion>(pendientes);
            if (linea != null) temporal.Insert(0, linea);
            temporal.Insert(0, historial.Pop());

            pendientes.Clear();
            foreach (LineaConversacion l in temporal) pendientes.Enqueue(l);

            caja.LimpiarOpciones();
            MostrarSiguiente();
        }

        private void MostrarSiguiente()
        {
            if (pendientes.Count == 0)
            {
                if (conversacion.TieneEleccion) MostrarOpciones();
                else Terminar();
                return;
            }

            linea = pendientes.Dequeue();
            caja.Configurar(linea.hablante, null);
            if (caja.textoNombre != null) caja.textoNombre.color = ColorDe(linea.hablante);
            caja.MostrarFlecha(false);

            if (escritura != null) StopCoroutine(escritura);
            escritura = StartCoroutine(Escribir(linea.texto));
        }

        private IEnumerator Escribir(string texto)
        {
            escribiendo = true;
            caja.textoDialogo.text = texto;
            caja.textoDialogo.maxVisibleCharacters = 0;
            for (int i = 0; i <= texto.Length; i++)
            {
                caja.textoDialogo.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(velocidadEscritura);
            }
            escribiendo = false;
            FinDeLinea();
        }

        private void FinDeLinea()
        {
            caja.MostrarFlecha(pendientes.Count > 0 || conversacion.TieneEleccion);
            if (pendientes.Count == 0 && conversacion.TieneEleccion) MostrarOpciones();
        }

        private void MostrarOpciones()
        {
            if (caja.contenedorOpciones == null || prefabBotonOpcion == null || caja.contenedorOpciones.childCount > 0) return;

            caja.textoDialogo.text = conversacion.preguntaFinal;
            caja.textoDialogo.maxVisibleCharacters = int.MaxValue;
            caja.MostrarFlecha(false);

            for (int i = 0; i < conversacion.opcionesFinal.Length; i++)
            {
                GameObject boton = Instantiate(prefabBotonOpcion, caja.contenedorOpciones);
                TextMeshProUGUI etiqueta = boton.GetComponentInChildren<TextMeshProUGUI>();
                if (etiqueta != null) etiqueta.text = "[" + (i + 1) + "] " + conversacion.opcionesFinal[i];
                Button b = boton.GetComponent<Button>();
                int indice = i;
                if (b != null) b.onClick.AddListener(() => ElegirOpcion(indice));
            }
        }

        private void ElegirOpcion(int indice)
        {
            if (!enCurso || !conversacion.TieneEleccion || escribiendo || pendientes.Count > 0) return;
            if (indice < 0 || indice >= conversacion.opcionesFinal.Length) return;

            string opcion = conversacion.opcionesFinal[indice];
            if (conversacion.id == "helena_noche" && indice == 0 && RPG.Flujo.GameManager.Instancia != null)
                RPG.Flujo.GameManager.Instancia.viajeAceptado = true;

            if (EleccionTomada != null) EleccionTomada(conversacion, opcion);
            Terminar();
        }

        private void Terminar()
        {
            ConversacionDatos terminada = conversacion;
            if (RPG.Flujo.GameManager.Instancia != null && terminada != null && !string.IsNullOrEmpty(terminada.id))
                RPG.Flujo.GameManager.Instancia.MarcarDialogoVisto(terminada.id);

            enCurso = false;
            linea = null;
            conversacion = null;
            caja.LimpiarOpciones();
            caja.Mostrar(false);
            EventosJuego.NotificarDialogoTerminado();
            if (Terminada != null) Terminada(terminada);
        }

        private static Color ColorDe(string hablante)
        {
            if (string.IsNullOrEmpty(hablante)) return new Color32(62, 38, 18, 255);
            string h = hablante.ToLowerInvariant();
            foreach (KeyValuePair<string, Color> par in coloresHablante)
                if (h.Contains(par.Key)) return par.Value;
            return new Color32(62, 38, 18, 255);
        }
    }
}
