using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using RPG.Datos;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>Tiradas de dados con la notacion del parcial: "1D10", "2D6", "1D10+1D4".</summary>
    public static class Dados
    {
        private static readonly Regex patron = new Regex(@"(\d+)\s*[dD]\s*(\d+)");

        public static int Tirar(string formula, out string detalle)
        {
            int total = 0;
            List<string> partes = new List<string>();
            foreach (Match m in patron.Matches(formula ?? string.Empty))
            {
                int cantidad = int.Parse(m.Groups[1].Value);
                int caras = int.Parse(m.Groups[2].Value);
                for (int i = 0; i < cantidad; i++)
                {
                    int r = UnityEngine.Random.Range(1, caras + 1);
                    total += r;
                    partes.Add(r.ToString());
                }
            }
            detalle = string.Join("+", partes.ToArray());
            return total;
        }
    }

    [Serializable]
    public class DatosAtaque
    {
        public string nombre = "Ataque";
        [Tooltip("Formula de dados, por ejemplo 1D10+1D4")] public string dados = "1D10";
    }

    [Serializable]
    public class PanelCombatiente
    {
        public string nombre;
        public GameObject raiz;
        public TextMeshProUGUI txtNombre;
        public TextMeshProUGUI txtHp;
        public Image barra;
        [NonSerialized] public int hp;
        [NonSerialized] public int hpMax;
        [NonSerialized] public float fillObjetivo = 1f;
    }

    /// <summary>
    /// Interfaz de combate por turnos estilo Pokemon: paneles de vida arriba, caja de mensajes abajo y menu
    /// ATACAR / ITEMS. Un solo heroe (el parcial permite 1 en lugar de 4).
    ///
    /// Como conectar la logica real de combate:
    ///  - Suscribirse a AtaqueElegido(indice) e ItemElegido(indice) (o usar los UnityEvent en el Inspector).
    ///  - Avisar cambios con EventosJuego.NotificarVidaCambiada / NotificarDanoRecibido / NotificarPifia / NotificarTurnoCambiado.
    ///  - Llamar a AbrirMenuPrincipal() cuando vuelva el turno del heroe.
    /// Con modoDemo activo, el propio script simula una pelea (solo para ver la interfaz funcionando).
    /// </summary>
    public class MenuCombatePokemon : MonoBehaviour
    {
        public enum Estado { Oculto, Mensaje, Principal, Ataques, Items }

        public static bool EnSubmenu { get; private set; }

        [Header("Modo")]
        public bool modoBoss;
        [Tooltip("Simula una pelea para probar la interfaz sin la logica real de combate.")] public bool modoDemo = true;

        [Header("Heroe (Heroe 2 del parcial)")]
        public string nombreHeroe = "Héroe";
        public int hpHeroe = 87;
        public int hpMaxHeroe = 87;
        public TextMeshProUGUI txtNombreHeroe;
        public TextMeshProUGUI txtHpHeroe;
        public Image barraHeroe;

        [Header("Enemigos (1 en el Bosque, Lupus y Helena en el Boss)")]
        public PanelCombatiente[] enemigos;

        [Header("Caja inferior")]
        public TextMeshProUGUI txtMensaje;
        public GameObject cajaMensaje;
        public GameObject menuPrincipal;
        public Button botonAtacar;
        public Button botonItems;
        public GameObject menuAtaques;
        public Button[] botonesAtaque;
        public Button botonVolverAtaques;
        public TextMeshProUGUI txtInfoAtaque;
        public GameObject menuItems;
        public Button[] botonesItem;
        public Button botonVolverItems;

        [Header("Ataques e items")]
        public List<DatosAtaque> ataques = new List<DatosAtaque>();
        public List<ItemVista> items = new List<ItemVista>();

        [Header("Eventos para la logica de combate")]
        public UnityEvent<int> AlElegirAtaque;
        public UnityEvent<int> AlElegirItem;
        public event Action<int> AtaqueElegido;
        public event Action<int> ItemElegido;

        [Header("Demo")]
        public int curacionDemo = 20;

        public Estado EstadoActual { get; private set; } = Estado.Oculto;

        private readonly Queue<string> mensajes = new Queue<string>();
        private Coroutine escritor;
        private bool turnoDemoEnCurso;
        private float tiempoUltimoMensaje;

        private void Awake()
        {
            if (botonAtacar != null) botonAtacar.onClick.AddListener(AbrirAtaques);
            if (botonItems != null) botonItems.onClick.AddListener(AbrirItems);
            if (botonVolverAtaques != null) botonVolverAtaques.onClick.AddListener(AbrirMenuPrincipal);
            if (botonVolverItems != null) botonVolverItems.onClick.AddListener(AbrirMenuPrincipal);

            for (int i = 0; i < botonesAtaque.Length; i++)
            {
                int indice = i;
                botonesAtaque[i].onClick.AddListener(() => ElegirAtaque(indice));
            }
            for (int i = 0; i < botonesItem.Length; i++)
            {
                int indice = i;
                botonesItem[i].onClick.AddListener(() => ElegirItem(indice));
            }
        }

        private void OnEnable()
        {
            EventosJuego.OnVidaCambiada += AlCambiarVida;
            EventosJuego.OnDanoRecibido += AlRecibirDano;
            EventosJuego.OnPifia += AlPifiar;
            EventosJuego.OnTurnoCambiado += AlCambiarTurno;
            EventosJuego.OnInventarioCambiado += AlCambiarInventario;
            ConfigurarPanelesIniciales();
            // La demo solo corre en la escena de vista previa "Interfaz"; en el juego real manda el combate.
            if (modoDemo && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Interfaz") IniciarDemo();
            else MostrarSolo(Estado.Oculto);
        }

        private void OnDisable()
        {
            EventosJuego.OnVidaCambiada -= AlCambiarVida;
            EventosJuego.OnDanoRecibido -= AlRecibirDano;
            EventosJuego.OnPifia -= AlPifiar;
            EventosJuego.OnTurnoCambiado -= AlCambiarTurno;
            EventosJuego.OnInventarioCambiado -= AlCambiarInventario;
            StopAllCoroutines();
            escritor = null;
            mensajes.Clear();
            turnoDemoEnCurso = false;
            EnSubmenu = false;
        }

        // ---------------------------------------------------------------- API publica

        public void IniciarCombate(string nombreEnemigo, int hp, int hpMax)
        {
            if (enemigos != null && enemigos.Length > 0) FijarVida(enemigos[0], nombreEnemigo, hp, hpMax);
            MostrarMensaje("¡Un " + nombreEnemigo + " salvaje bloquea el camino!");
            AbrirMenuPrincipalTrasMensajes();
        }

        public void ConfigurarAtaques(List<DatosAtaque> nuevos)
        {
            ataques = nuevos ?? new List<DatosAtaque>();
            RefrescarBotonesAtaque();
        }

        public void ConfigurarItems(List<ItemVista> nuevos)
        {
            items = nuevos ?? new List<ItemVista>();
            RefrescarBotonesItem();
        }

        public void MostrarMensaje(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return;
            mensajes.Enqueue(texto);
            if (escritor == null) escritor = StartCoroutine(ProcesarMensajes());
        }

        public void AbrirMenuPrincipal()
        {
            if (modoObjetivo)
            {
                Action<int> cb = alElegirObjetivo;
                modoObjetivo = false;
                alElegirObjetivo = null;
                if (cb != null) cb(-1);
            }
            MostrarSolo(Estado.Principal);
            if (txtMensaje != null) txtMensaje.text = "¿Qué hará " + nombreHeroe + "?";
            Seleccionar(botonAtacar);
        }

        public void AbrirAtaques()
        {
            RefrescarBotonesAtaque();
            MostrarSolo(Estado.Ataques);
            if (botonesAtaque.Length > 0) Seleccionar(botonesAtaque[0]);
        }

        public void AbrirItems()
        {
            RefrescarBotonesItem();
            MostrarSolo(Estado.Items);
            Seleccionar(HayItems() ? botonesItem[0] : botonVolverItems);
        }

        // ---------------------------------------------------------------- Combate real

        public bool HayMensajes { get { return escritor != null || mensajes.Count > 0; } }

        private static string TextoPifia(string formula)
        {
            Match m = Regex.Match(formula ?? string.Empty, @"(\d+)\s*[dD]\s*(\d+)");
            if (!m.Success) return string.Empty;
            int caras = int.Parse(m.Groups[2].Value);
            int umbral = DiceSystem.UmbralPifia(caras);
            return umbral <= 1 ? "Pifia: sale 1" : "Pifia: sale 1-" + umbral;
        }

        private bool modoObjetivo;
        private Action<int> alElegirObjetivo;

        /// <summary>Deja la interfaz lista para un combate real (sin demo): vidas, ataques del heroe e inventario.</summary>
        public void PrepararCombate(string[] nombres, int[] hps, int[] maxs, int hpH, int maxH,
                                    List<DatosAtaque> ataquesHeroe, List<ItemVista> itemsHeroe)
        {
            StopAllCoroutines();
            escritor = null;
            mensajes.Clear();
            turnoDemoEnCurso = false;
            modoObjetivo = false;
            alElegirObjetivo = null;
            modoDemo = false;

            hpHeroe = hpH;
            hpMaxHeroe = maxH;
            ActualizarPanelHeroe();
            if (barraHeroe != null) barraHeroe.fillAmount = maxH > 0 ? (float)hpH / maxH : 0f;

            if (enemigos != null)
            {
                for (int i = 0; i < enemigos.Length; i++)
                {
                    PanelCombatiente p = enemigos[i];
                    bool activo = i < nombres.Length;
                    if (p.raiz != null) p.raiz.SetActive(activo);
                    if (!activo) continue;
                    FijarVida(p, nombres[i], hps[i], maxs[i]);
                    if (p.barra != null) p.barra.fillAmount = p.fillObjetivo;
                }
            }

            // El fondo azul oscuro es solo de la vista previa: en el juego real se ve la escena.
            Transform fondoVistaPrevia = transform.Find("Fondo");
            if (fondoVistaPrevia != null) fondoVistaPrevia.gameObject.SetActive(false);

            // En el juego real los personajes se ven en la escena: se ocultan los recuadros grises de vista previa.
            foreach (Transform hijo in GetComponentsInChildren<Transform>(true))
                if (hijo.name.StartsWith("MarcadorSprite")) hijo.gameObject.SetActive(false);

            ConfigurarAtaques(ataquesHeroe);
            ConfigurarItems(itemsHeroe);
            MostrarSolo(Estado.Mensaje);
        }

        public void SetVidaHeroe(int hp, int max)
        {
            hpHeroe = hp;
            hpMaxHeroe = max;
            ActualizarPanelHeroe();
        }

        public void SetVidaEnemigo(int indice, int hp)
        {
            if (enemigos == null || indice < 0 || indice >= enemigos.Length) return;
            PanelCombatiente p = enemigos[indice];
            FijarVida(p, p.nombre, hp, p.hpMax);
        }

        public void OcultarTodo() { MostrarSolo(Estado.Oculto); }

        /// <summary>Muestra los enemigos vivos como botones para elegir a quien atacar. Devuelve -1 si se cancela.</summary>
        public void PedirObjetivo(string[] nombres, int[] hps, int[] maxs, bool[] vivos, Action<int> alElegir)
        {
            modoObjetivo = true;
            alElegirObjetivo = alElegir;
            MostrarSolo(Estado.Ataques);
            Button primero = null;
            for (int i = 0; i < botonesAtaque.Length; i++)
            {
                bool existe = i < nombres.Length && vivos[i];
                botonesAtaque[i].gameObject.SetActive(existe);
                if (!existe) continue;
                if (primero == null) primero = botonesAtaque[i];
                foreach (TextMeshProUGUI t in botonesAtaque[i].GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (t.name == "Etiqueta") t.text = nombres[i];
                    else if (t.name == "Dados") t.text = "HP " + hps[i] + "/" + maxs[i];
                }
            }
            Seleccionar(primero);
        }

        // ---------------------------------------------------------------- Seleccion

        private void ElegirAtaque(int indice)
        {
            if (modoObjetivo)
            {
                Action<int> cb = alElegirObjetivo;
                modoObjetivo = false;
                alElegirObjetivo = null;
                MostrarSolo(Estado.Mensaje);
                if (cb != null) cb(indice);
                return;
            }
            if (indice >= ataques.Count) return;
            MostrarSolo(Estado.Mensaje);
            if (AtaqueElegido != null) AtaqueElegido(indice);
            if (AlElegirAtaque != null) AlElegirAtaque.Invoke(indice);
            if (modoDemo && !turnoDemoEnCurso) StartCoroutine(TurnoDemoAtaque(indice));
        }

        private void ElegirItem(int indice)
        {
            if (indice >= items.Count) return;
            MostrarSolo(Estado.Mensaje);
            if (ItemElegido != null) ItemElegido(indice);
            if (AlElegirItem != null) AlElegirItem.Invoke(indice);
            if (modoDemo && !turnoDemoEnCurso) StartCoroutine(TurnoDemoItem(indice));
        }

        private bool HayItems() { return items != null && items.Count > 0; }

        private void Update()
        {
            if (EstadoActual == Estado.Ataques || EstadoActual == Estado.Items)
            {
                if (EntradaUI.Cancelar())
                {
                    EntradaUI.ConsumirCancelar();
                    AbrirMenuPrincipal();
                }
            }

            // Si se hizo clic fuera de un boton, se recupera el foco para seguir con el teclado.
            if ((EstadoActual == Estado.Principal || EstadoActual == Estado.Ataques || EstadoActual == Estado.Items)
                && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                Seleccionar(PrimerBotonActivo());
            }

            ActualizarCursores();
            AnimarBarras();
            UpdateInfoAtaque();
        }

        private Button PrimerBotonActivo()
        {
            if (EstadoActual == Estado.Principal) return botonAtacar;
            if (EstadoActual == Estado.Ataques) return botonesAtaque.Length > 0 ? botonesAtaque[0] : botonVolverAtaques;
            return HayItems() ? botonesItem[0] : botonVolverItems;
        }

        private void Seleccionar(Button b)
        {
            if (b == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(b.gameObject);
        }

        private void ActualizarCursores()
        {
            if (EventSystem.current == null) return;
            GameObject sel = EventSystem.current.currentSelectedGameObject;
            ActualizarCursor(botonAtacar, sel);
            ActualizarCursor(botonItems, sel);
            ActualizarCursor(botonVolverAtaques, sel);
            ActualizarCursor(botonVolverItems, sel);
            foreach (Button b in botonesAtaque) ActualizarCursor(b, sel);
            foreach (Button b in botonesItem) ActualizarCursor(b, sel);
        }

        private static void ActualizarCursor(Button b, GameObject seleccionado)
        {
            if (b == null) return;
            Transform cursor = b.transform.Find("Cursor");
            if (cursor != null) cursor.gameObject.SetActive(seleccionado == b.gameObject);
        }

        private void UpdateInfoAtaque()
        {
            if (txtInfoAtaque == null || EstadoActual != Estado.Ataques || EventSystem.current == null) return;
            GameObject sel = EventSystem.current.currentSelectedGameObject;
            for (int i = 0; i < botonesAtaque.Length; i++)
            {
                if (sel == botonesAtaque[i].gameObject && i < ataques.Count)
                {
                    txtInfoAtaque.text = "Tirada: " + ataques[i].dados + "\n" + TextoPifia(ataques[i].dados);
                    return;
                }
            }
            txtInfoAtaque.text = modoObjetivo ? "Elige el objetivo\n[Esc] volver" : "Elige un ataque\n[Esc] volver";
        }

        // ---------------------------------------------------------------- Estados de pantalla

        private void MostrarSolo(Estado estado)
        {
            EstadoActual = estado;
            EnSubmenu = estado == Estado.Ataques || estado == Estado.Items;
            if (cajaMensaje != null) cajaMensaje.SetActive(estado == Estado.Mensaje || estado == Estado.Principal || estado == Estado.Oculto);
            if (menuPrincipal != null) menuPrincipal.SetActive(estado == Estado.Principal);
            if (menuAtaques != null) menuAtaques.SetActive(estado == Estado.Ataques);
            if (menuItems != null) menuItems.SetActive(estado == Estado.Items);
        }

        private void RefrescarBotonesAtaque()
        {
            for (int i = 0; i < botonesAtaque.Length; i++)
            {
                bool existe = i < ataques.Count;
                botonesAtaque[i].gameObject.SetActive(existe);
                if (!existe) continue;
                TextMeshProUGUI[] textos = botonesAtaque[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI t in textos)
                {
                    if (t.name == "Etiqueta") t.text = ataques[i].nombre;
                    else if (t.name == "Dados") t.text = ataques[i].dados;
                }
            }
        }

        private void RefrescarBotonesItem()
        {
            for (int i = 0; i < botonesItem.Length; i++)
            {
                bool existe = i < items.Count;
                botonesItem[i].gameObject.SetActive(existe);
                if (!existe) continue;
                TextMeshProUGUI etiqueta = botonesItem[i].GetComponentInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI t in botonesItem[i].GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (t.name == "Etiqueta") etiqueta = t;
                if (etiqueta != null) etiqueta.text = items[i].nombre + "  x" + items[i].cantidad;
            }
        }

        // ---------------------------------------------------------------- Mensajes

        private IEnumerator ProcesarMensajes()
        {
            if (EstadoActual == Estado.Principal || EstadoActual == Estado.Oculto) MostrarSolo(Estado.Mensaje);
            while (mensajes.Count > 0)
            {
                string m = mensajes.Dequeue();
                if (txtMensaje != null)
                {
                    txtMensaje.text = m;
                    txtMensaje.maxVisibleCharacters = 0;
                    for (int i = 0; i <= m.Length; i++)
                    {
                        txtMensaje.maxVisibleCharacters = i;
                        yield return new WaitForSecondsRealtime(0.015f);
                    }
                    txtMensaje.maxVisibleCharacters = int.MaxValue;
                }
                yield return new WaitForSecondsRealtime(0.9f);
            }
            escritor = null;
        }

        private void AbrirMenuPrincipalTrasMensajes()
        {
            StartCoroutine(EsperarMensajesYAbrir());
        }

        private IEnumerator EsperarMensajesYAbrir()
        {
            while (escritor != null || mensajes.Count > 0) yield return null;
            AbrirMenuPrincipal();
        }

        // ---------------------------------------------------------------- Eventos del juego

        private void AlCambiarVida(string quien, int actual, int max)
        {
            if (string.IsNullOrEmpty(quien)) return;
            string q = quien.ToLowerInvariant();
            if (q == "heroe" || q == "jugador" || q == "héroe")
            {
                hpHeroe = actual;
                hpMaxHeroe = max;
                ActualizarPanelHeroe();
                return;
            }

            PanelCombatiente panel = BuscarEnemigo(quien);
            if (panel != null) FijarVida(panel, panel.nombre, actual, max);
        }

        private void AlRecibirDano(string objetivo, int cantidad)
        {
            MostrarMensaje(objetivo + " recibe " + cantidad + " de daño.");
        }

        private void AlPifiar(string quien)
        {
            MostrarMensaje("¡" + quien + " ha pifiado!");
        }

        private void AlCambiarTurno(string quien)
        {
            if (string.IsNullOrEmpty(quien)) return;
            string q = quien.ToLowerInvariant();
            if (q == "heroe" || q == "jugador" || q == "héroe") AbrirMenuPrincipalTrasMensajes();
        }

        private void AlCambiarInventario(List<ItemVista> lista)
        {
            ConfigurarItems(lista);
        }

        // ---------------------------------------------------------------- Paneles de vida

        private void ConfigurarPanelesIniciales()
        {
            if (txtNombreHeroe != null) txtNombreHeroe.text = nombreHeroe.ToUpperInvariant();
            ActualizarPanelHeroe();
            if (enemigos == null) return;
            for (int i = 0; i < enemigos.Length; i++)
            {
                PanelCombatiente p = enemigos[i];
                if (p.raiz != null) p.raiz.SetActive(modoBoss ? i < 2 : i == 0);
                if (p.hpMax <= 0)
                {
                    p.hpMax = Mathf.Max(1, p.hp);
                    if (p.hp <= 0) p.hp = p.hpMax;
                }
                ActualizarPanel(p);
            }
        }

        private void FijarVida(PanelCombatiente p, string nombre, int hp, int max)
        {
            p.nombre = nombre;
            p.hp = Mathf.Max(0, hp);
            p.hpMax = Mathf.Max(1, max);
            ActualizarPanel(p);
        }

        private void ActualizarPanel(PanelCombatiente p)
        {
            if (p.txtNombre != null) p.txtNombre.text = (p.nombre ?? string.Empty).ToUpperInvariant();
            if (p.txtHp != null) p.txtHp.text = "HP " + p.hp + "/" + p.hpMax;
            p.fillObjetivo = p.hpMax > 0 ? Mathf.Clamp01((float)p.hp / p.hpMax) : 0f;
        }

        private void ActualizarPanelHeroe()
        {
            if (txtHpHeroe != null) txtHpHeroe.text = "HP " + hpHeroe + "/" + hpMaxHeroe;
        }

        private PanelCombatiente BuscarEnemigo(string nombre)
        {
            if (enemigos == null) return null;
            foreach (PanelCombatiente p in enemigos)
                if (!string.IsNullOrEmpty(p.nombre) && string.Equals(p.nombre, nombre, StringComparison.OrdinalIgnoreCase)) return p;
            return enemigos.Length > 0 ? enemigos[0] : null;
        }

        private void AnimarBarras()
        {
            float objetivoHeroe = hpMaxHeroe > 0 ? Mathf.Clamp01((float)hpHeroe / hpMaxHeroe) : 0f;
            Animar(barraHeroe, objetivoHeroe);
            if (enemigos == null) return;
            foreach (PanelCombatiente p in enemigos) Animar(p.barra, p.fillObjetivo);
        }

        private static void Animar(Image barra, float objetivo)
        {
            if (barra == null) return;
            barra.fillAmount = Mathf.MoveTowards(barra.fillAmount, objetivo, Time.unscaledDeltaTime * 0.6f);
            barra.color = barra.fillAmount > 0.5f ? new Color32(0x2D, 0x80, 0x32, 255)
                        : barra.fillAmount > 0.2f ? new Color32(0xD4, 0xAF, 0x37, 255)
                        : new Color32(0xC8, 0x30, 0x15, 255);
        }

        // ---------------------------------------------------------------- Demo (solo para ver la interfaz)

        private void IniciarDemo()
        {
            if (ataques.Count == 0)
            {
                ataques.Add(new DatosAtaque { nombre = "Ataque 1", dados = "1D10" });
                ataques.Add(new DatosAtaque { nombre = "Ataque 2", dados = "1D10+1D4" });
                ataques.Add(new DatosAtaque { nombre = "Ataque 3", dados = "1D10+2D4" });
            }
            if (items.Count == 0) items.Add(new ItemVista { nombre = "Poción", cantidad = 3 });

            if (enemigos != null)
            {
                if (modoBoss)
                {
                    if (enemigos.Length > 0) { enemigos[0].nombre = "Lupus"; enemigos[0].hp = enemigos[0].hpMax = 82; }
                    if (enemigos.Length > 1) { enemigos[1].nombre = "Helena"; enemigos[1].hp = enemigos[1].hpMax = 35; }
                }
                else if (enemigos.Length > 0)
                {
                    enemigos[0].nombre = "Enemigo del bosque 1"; enemigos[0].hp = enemigos[0].hpMax = 13;
                }
            }

            ConfigurarPanelesIniciales();
            RefrescarBotonesAtaque();
            RefrescarBotonesItem();
            mensajes.Clear();
            MostrarMensaje(modoBoss ? "¡Lupus y Helena bloquean el camino!" : "¡Un enemigo del bosque bloquea el camino!");
            AbrirMenuPrincipalTrasMensajes();
        }

        private PanelCombatiente EnemigoVivo()
        {
            if (enemigos == null) return null;
            foreach (PanelCombatiente p in enemigos)
                if (p.raiz != null && p.raiz.activeSelf && p.hp > 0) return p;
            return null;
        }

        public IEnumerator EsperarMensajes()
        {
            while (escritor != null || mensajes.Count > 0) yield return null;
        }

        private IEnumerator TurnoDemoAtaque(int indice)
        {
            turnoDemoEnCurso = true;
            DatosAtaque ataque = ataques[indice];
            string detalle;
            int dano = Dados.Tirar(ataque.dados, out detalle);
            MostrarMensaje(nombreHeroe + " usa " + ataque.nombre + " (" + ataque.dados + ": " + detalle + ")");

            PanelCombatiente objetivo = EnemigoVivo();
            if (objetivo != null)
            {
                int nuevo = Mathf.Max(0, objetivo.hp - dano);
                EventosJuego.NotificarDanoRecibido(objetivo.nombre, dano);
                EventosJuego.NotificarVidaCambiada(objetivo.nombre, nuevo, objetivo.hpMax);
            }
            yield return EsperarMensajes();
            yield return StartCoroutine(TurnoEnemigoDemo());
        }

        private IEnumerator TurnoDemoItem(int indice)
        {
            turnoDemoEnCurso = true;
            ItemVista item = items[indice];
            if (item.cantidad > 0)
            {
                item.cantidad--;
                int nuevo = Mathf.Min(hpMaxHeroe, hpHeroe + curacionDemo);
                MostrarMensaje(nombreHeroe + " usa " + item.nombre + " y recupera " + (nuevo - hpHeroe) + " HP.");
                EventosJuego.NotificarVidaCambiada("Heroe", nuevo, hpMaxHeroe);
                if (item.cantidad <= 0) items.RemoveAt(indice);
            }
            else MostrarMensaje("No quedan " + item.nombre + ".");
            RefrescarBotonesItem();
            yield return EsperarMensajes();
            yield return StartCoroutine(TurnoEnemigoDemo());
        }

        private IEnumerator TurnoEnemigoDemo()
        {
            PanelCombatiente atacante = EnemigoVivo();
            if (atacante == null)
            {
                MostrarMensaje("¡Victoria!");
                yield return EsperarMensajes();
                turnoDemoEnCurso = false;
                IniciarDemo();
                yield break;
            }

            string formula = !modoBoss ? "1D6" : (UnityEngine.Random.value < 0.5f ? "1D4+1D6" : "2D6");
            string detalle;
            int dano = Dados.Tirar(formula, out detalle);
            MostrarMensaje(atacante.nombre + " ataca (" + formula + ": " + detalle + ")");
            EventosJuego.NotificarDanoRecibido(nombreHeroe, dano);
            EventosJuego.NotificarVidaCambiada("Heroe", Mathf.Max(0, hpHeroe - dano), hpMaxHeroe);
            yield return EsperarMensajes();

            if (modoBoss && enemigos.Length > 1 && enemigos[1].hp > 0 && atacante == enemigos[0])
            {
                // Tirada de ayuda de Lupus a Helena (1D10): >7 y <=9 es pifia, <7 es exito.
                int tirada = UnityEngine.Random.Range(1, 11);
                MostrarMensaje("Lupus llama a Helena... tirada 1D10: " + tirada);
                if (tirada > 7 && tirada <= 9) EventosJuego.NotificarPifia("Helena");
                else if (tirada < 7)
                {
                    string d2;
                    int ayuda = Dados.Tirar("1D8+1D6", out d2);
                    EventosJuego.NotificarDanoRecibido(nombreHeroe, ayuda);
                    EventosJuego.NotificarVidaCambiada("Heroe", Mathf.Max(0, hpHeroe - ayuda), hpMaxHeroe);
                }
                yield return EsperarMensajes();
            }

            turnoDemoEnCurso = false;
            if (hpHeroe <= 0)
            {
                MostrarMensaje(nombreHeroe + " ha caído...");
                yield return EsperarMensajes();
                EventosJuego.NotificarVidaCambiada("Heroe", hpMaxHeroe, hpMaxHeroe);
                IniciarDemo();
                yield break;
            }
            AbrirMenuPrincipal();
        }
    }
}
