using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using RPG.Datos;
using RPG.Dialogos;
using RPG.UI;

/// <summary>
/// Arma (o rearma) la escena Assets/Interfaz/Interfaz.unity con todas las interfaces del juego, organizadas por parte del juego.
/// Menu: RPG > Construir escena Interfaz. Es seguro repetirlo: borra el objeto INTERFAZ anterior y lo vuelve a crear.
/// </summary>
public static class ConstructorInterfaz
{
    private const string RutaEscena = "Assets/Interfaz/Interfaz.unity";

    private static readonly Color Panel = new Color32(26, 20, 16, 255);
    private static readonly Color Dorado = new Color32(212, 175, 55, 255);
    private static readonly Color Crema = new Color32(240, 230, 210, 255);
    private static readonly Color Verde = new Color32(0x2D, 0x80, 0x32, 255);
    private static readonly Color Morado = new Color32(0x52, 0x22, 0x5E, 255);
    private static readonly Color Tinta = new Color32(62, 38, 18, 255);
    private static readonly Color TintaOro = new Color32(122, 78, 10, 255);
    private static readonly Color TintaNombre = new Color32(120, 36, 24, 255);
    private static readonly Color BotonBase = new Color32(110, 86, 48, 255);

    private static TMP_FontAsset fuenteTitulos;
    private static Sprite spriteUI;
    private static Sprite spriteBlanco;
    private static GameObject prefabBotonOpcion;
    private static GameObject prefabItem;
    private static GameObject prefabDano;

    [MenuItem("RPG/Construir escena Interfaz")]
    public static void Construir()
    {
        UnityEngine.SceneManagement.Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        foreach (GameObject viejo in escena.GetRootGameObjects())
            if (viejo.name == "INTERFAZ" || viejo.name == "EventSystem") UnityEngine.Object.DestroyImmediate(viejo);

        fuenteTitulos = null; // La fuente PermanentMarker del menu trae color morado propio (poco contraste): se usa la fuente por defecto en negrita.
        spriteUI = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        spriteBlanco = ObtenerSpriteBlanco();
        prefabBotonOpcion = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BotonOpcionPrefab.prefab");
        prefabItem = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ItemUIPrefab.prefab");
        prefabDano = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NumeroDanoPrefab.prefab");

        GameObject raiz = new GameObject("INTERFAZ");
        GestorInterfaz gestor = raiz.AddComponent<GestorInterfaz>();

        // ---------------- Grupos por parte del juego
        GrupoInterfaz g1 = CrearGrupo(raiz.transform, "ESCENA 1 · Juego Pueblo (taberna, de noche)", ParteJuego.Escena1_JuegoPueblo,
            new[] { "Juego Pueblo" }, "Taberna · Escena 1", false, false, Conversaciones(1, 5));
        GrupoInterfaz g2 = CrearGrupo(raiz.transform, "ESCENA 2 · Pueblo Mañana", ParteJuego.Escena2_PuebloManana,
            new[] { "Pueblo Mañana" }, "Pueblo de mañana · Escena 2", false, false, Conversaciones(6, 10));
        GrupoInterfaz g3 = CrearGrupo(raiz.transform, "ESCENA 3 · Bosque (encuentros y combates)", ParteJuego.Escena3_Bosque,
            new[] { "Bosque", "Combate" }, "Bosque · Escena 3", true, false, Conversaciones(11, 12));
        GrupoInterfaz g4 = CrearGrupo(raiz.transform, "BOSS y FINAL · escena por crear", ParteJuego.Boss_Final,
            new[] { "Boss" }, "Guarida de Lupus · Boss y final", true, true, Conversaciones(13, 15));

        // ---------------- Siempre disponibles
        GameObject menu = Vacio("MENÚ · disponible en cualquier momento de la partida", raiz.transform);
        CrearPausa(menu.transform);
        GameObject objetos = Vacio("OBJETOS · disponible en cualquier momento de la partida", raiz.transform);
        CrearInventario(objetos.transform);
        CrearNotificaciones(objetos.transform);

        // ---------------- Ayuda de vista previa
        GameObject ayuda = CrearAyuda(raiz.transform);

        gestor.grupos = new List<GrupoInterfaz> { g1, g2, g3, g4 };
        gestor.ayudaVistaPrevia = ayuda;

        // Solo el primer grupo queda visible al abrir la escena; GestorInterfaz activa el que corresponda al jugar.
        g2.raiz.SetActive(false);
        g3.raiz.SetActive(false);
        g4.raiz.SetActive(false);

        // Fondo oscuro para la vista previa (la camara de esta escena era blanca).
        Camera cam = Camera.main;
        if (cam != null) { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color32(22, 24, 30, 255); }

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        es.transform.SetAsLastSibling();

        ConfigurarBuildSettings();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("[ConstructorInterfaz] Escena Interfaz construida y guardada.");
    }

    // =====================================================================================
    //  GRUPOS
    // =====================================================================================

    private static GrupoInterfaz CrearGrupo(Transform padre, string nombre, ParteJuego parte, string[] escenas, string zona,
        bool conCombate, bool boss, List<DatosConv> datos)
    {
        GameObject raiz = Vacio(nombre, padre);
        GrupoInterfaz g = new GrupoInterfaz { etiqueta = nombre, parte = parte, escenasUnity = escenas, raiz = raiz };

        g.escenarioNormal = CrearHud(raiz.transform, zona);
        CrearConversaciones(raiz.transform, parte, datos, g);
        if (conCombate)
        {
            g.combate = CrearCombate(raiz.transform, boss, out g.menuCombate);
            g.combate.SetActive(false);
        }
        return g;
    }

    // =====================================================================================
    //  1) ESCENARIO NORMAL (HUD)
    // =====================================================================================

    private static GameObject CrearHud(Transform padre, string zona)
    {
        Canvas canvas = NuevoCanvas("HUD · escenario normal", padre, 10);
        canvas.gameObject.AddComponent<HUDExploracion>();
        Transform t = canvas.transform;

        Image fondo = Perg("FondoEstado", t);
        Ancla(fondo.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(580, 130));

        TextMeshProUGUI vida = Texto("TextoVida", t, "HP: 87/87", 34, Tinta, TextAlignmentOptions.Left);
        Ancla(vida.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(55, -42), new Vector2(530, 44));

        Image fondoBarra = Caja("FondoBarraVida", t, new Color32(10, 8, 6, 255), true);
        Ancla(fondoBarra.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(55, -98), new Vector2(530, 28));
        Image barra = Barra("BarraVida", t, Verde);
        Ancla(barra.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(55, -98), new Vector2(530, 28));

        Image fondoOro = Perg("FondoOro", t);
        Ancla(fondoOro.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(320, 80));
        TextMeshProUGUI oro = Texto("TextoOro", t, "ORO: 0g", 38, TintaOro, TextAlignmentOptions.Center);
        Ancla(oro.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(320, 80));

        TextMeshProUGUI lugar = Texto("TextoZona", t, zona.ToUpperInvariant(), 26, Crema, TextAlignmentOptions.Center);
        Ancla(lugar.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(760, 40));

        Image prompt = Perg("PromptInteraccion", t);
        Ancla(prompt.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(520, 70));
        TextMeshProUGUI promptTxt = Texto("Texto", prompt.transform, "[E] Hablar / Interactuar", 30, Tinta, TextAlignmentOptions.Center);
        Estirar(promptTxt.rectTransform);

        return canvas.gameObject;
    }

    // =====================================================================================
    //  2) CONVERSACIONES (hablando con un personaje)
    // =====================================================================================

    private class DatosConv
    {
        public int numero;
        public string titulo;
        public string tipo;
        public string fuente;
        public string id;
        public string json;      // id del JSON en Resources/Dialogos
        public string asset;     // ScriptableObject DialogoNPC ya usado por el juego
    }

    private static List<DatosConv> Conversaciones(int desde, int hasta)
    {
        List<DatosConv> todas = new List<DatosConv>
        {
            new DatosConv { numero = 1, titulo = "Taberna: Jorge y la inscripción de la espada", tipo = "Charla con PNJ", asset = "DialogoBar", id = "dialogo_bar" },
            new DatosConv { numero = 2, titulo = "Taberna: bienvenida de Jorge (guion de la interfaz)", tipo = "Charla con PNJ", json = "taberna_intro", id = "taberna_intro" },
            new DatosConv { numero = 3, titulo = "Jorge de noche: vuelve adentro", tipo = "Charla con PNJ", asset = "DialogoBar_Noche", id = "dialogo_bar_noche" },
            new DatosConv { numero = 4, titulo = "Helena (la monja) propone el viaje", tipo = "Charla con PNJ", asset = "DialogoNun", id = "dialogo_nun" },
            new DatosConv { numero = 5, titulo = "Helena de noche: propuesta con elección", tipo = "Cinemática con elección", json = "helena_noche", id = "helena_noche" },
            new DatosConv { numero = 6, titulo = "Anciano Raimundo", tipo = "Charla con PNJ", json = "aldeano_anciano", id = "aldeano_anciano" },
            new DatosConv { numero = 7, titulo = "Guardia Rodrigo", tipo = "Charla con PNJ", json = "aldeano_guardia", id = "aldeano_guardia" },
            new DatosConv { numero = 8, titulo = "Herrero Mateo", tipo = "Charla con PNJ", json = "aldeano_herrero", id = "aldeano_herrero" },
            new DatosConv { numero = 9, titulo = "Posadera Teresa", tipo = "Charla con PNJ", json = "aldeano_posadera", id = "aldeano_posadera" },
            new DatosConv { numero = 10, titulo = "Salida del pueblo hacia el bosque", tipo = "Cinemática", json = "salida_pueblo", id = "salida_pueblo" },
            new DatosConv { numero = 11, titulo = "Helena en el bosque (territorio de los groks)", tipo = "Charla con PNJ", asset = "DialogoNun_Bosque", id = "dialogo_nun_bosque" },
            new DatosConv { numero = 12, titulo = "Llegada a Sástago / aldea de Zuhayr", tipo = "Cinemática", json = "llegada_sastago", id = "llegada_sastago" },
            new DatosConv { numero = 13, titulo = "Lupus despierta (introducción del jefe)", tipo = "Cinemática de jefe", json = "lupus_intro", id = "lupus_intro" },
            new DatosConv { numero = 14, titulo = "Final: victoria", tipo = "Final", json = "fin_victoria", id = "fin_victoria" },
            new DatosConv { numero = 15, titulo = "Final: derrota", tipo = "Final", json = "fin_derrota", id = "fin_derrota" }
        };
        return todas.FindAll(c => c.numero >= desde && c.numero <= hasta);
    }

    private static void CrearConversaciones(Transform padre, ParteJuego parte, List<DatosConv> datos, GrupoInterfaz g)
    {
        GameObject raiz = Vacio("CHARLAS · hablando con un personaje", padre);

        Canvas canvas = NuevoCanvas("Caja de diálogo", raiz.transform, 20);
        CajaDialogo caja = canvas.gameObject.AddComponent<CajaDialogo>();
        Transform t = canvas.transform;

        RectTransform panel = UI("Panel", t);
        Ancla(panel, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1500, 270));
        Image fondo = panel.gameObject.AddComponent<Image>();
        fondo.sprite = ObtenerSpritePergamino();
        fondo.type = Image.Type.Sliced;
        fondo.color = Color.white;

        TextMeshProUGUI nombre = Texto("Nombre", panel, "Hablante", 38, TintaNombre, TextAlignmentOptions.Left, true);
        Ancla(nombre.rectTransform, new Vector2(0, 1), new Vector2(0, 0), new Vector2(40, 8), new Vector2(520, 56));
        Image etiqueta = Perg("FondoNombre", panel);
        etiqueta.rectTransform.SetAsFirstSibling();
        Ancla(etiqueta.rectTransform, new Vector2(0, 1), new Vector2(0, 0), new Vector2(30, 4), new Vector2(480, 56));
        nombre.transform.SetAsLastSibling();
        nombre.rectTransform.sizeDelta = new Vector2(460, 56);
        nombre.rectTransform.anchoredPosition = new Vector2(40, 4);
        nombre.alignment = TextAlignmentOptions.Left;

        TextMeshProUGUI texto = Texto("Texto", panel, "Texto del diálogo...", 36, Tinta, TextAlignmentOptions.TopLeft);
        Estirar(texto.rectTransform, 40, 30, 40, 30);

        TextMeshProUGUI flecha = Texto("Flecha", panel, "▼", 36, TintaNombre, TextAlignmentOptions.Center);
        Ancla(flecha.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 14), new Vector2(60, 50));

        TextMeshProUGUI numero = Texto("Numero", panel, "Conversación 1", 24, new Color32(115, 90, 58, 255), TextAlignmentOptions.Right);
        Ancla(numero.rectTransform, new Vector2(1, 1), new Vector2(1, 0), new Vector2(-30, 8), new Vector2(420, 40));

        RectTransform opciones = UI("Opciones", panel);
        Ancla(opciones, new Vector2(1, 1), new Vector2(1, 0), new Vector2(-30, 70), new Vector2(560, 160));
        VerticalLayoutGroup vl = opciones.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 10;
        vl.childAlignment = TextAnchor.LowerRight;
        vl.childControlHeight = true;
        vl.childControlWidth = true;
        vl.childForceExpandHeight = false;

        caja.panel = panel.gameObject;
        caja.textoNombre = nombre;
        caja.textoDialogo = texto;
        caja.flechaContinuar = flecha.gameObject;
        caja.contenedorOpciones = opciones;

        ReproductorConversacion reproductor = raiz.AddComponent<ReproductorConversacion>();
        reproductor.caja = caja;
        reproductor.textoNumero = numero;
        reproductor.prefabBotonOpcion = prefabBotonOpcion;
        g.conversaciones = raiz;
        g.reproductor = reproductor;

        foreach (DatosConv d in datos)
        {
            GameObject go = Vacio("Conversación " + d.numero + " · " + d.titulo, raiz.transform);
            ConversacionDatos cd = go.AddComponent<ConversacionDatos>();
            cd.numero = d.numero;
            cd.titulo = d.titulo;
            cd.parte = parte;
            cd.tipo = d.tipo;
            cd.id = d.id;
            LlenarTextos(cd, d);
            reproductor.conversaciones.Add(cd);
        }
    }

    private static void LlenarTextos(ConversacionDatos cd, DatosConv d)
    {
        if (!string.IsNullOrEmpty(d.json))
        {
            TextAsset ta = Resources.Load<TextAsset>("Dialogos/" + d.json);
            cd.fuente = "Resources/Dialogos/" + d.json + ".json (guion de la interfaz)";
            if (ta == null) { Debug.LogWarning("No se encontró el JSON " + d.json); cd.lineas = new LineaConversacion[0]; return; }
            DatosDialogo datos = JsonUtility.FromJson<DatosDialogo>(ta.text);
            List<LineaConversacion> lineas = new List<LineaConversacion>();
            foreach (RPG.Datos.LineaDialogo l in datos.lineas)
                lineas.Add(new LineaConversacion { hablante = l.hablante, texto = l.texto });
            cd.lineas = lineas.ToArray();
            if (datos.eleccion != null && datos.eleccion.opciones != null && datos.eleccion.opciones.Length > 0)
            {
                cd.preguntaFinal = datos.eleccion.pregunta;
                cd.opcionesFinal = datos.eleccion.opciones;
            }
        }
        else if (!string.IsNullOrEmpty(d.asset))
        {
            DialogoNPC so = AssetDatabase.LoadAssetAtPath<DialogoNPC>("Assets/Data/DialogosNPC/" + d.asset + ".asset");
            cd.fuente = "Data/DialogosNPC/" + d.asset + ".asset (texto que ya usa el juego)";
            List<LineaConversacion> lineas = new List<LineaConversacion>();
            if (so != null)
            {
                foreach (string cruda in so.lineas)
                {
                    string hablante = "Narrador";
                    string texto = cruda;
                    int i = cruda.IndexOf(':');
                    if (i > 0 && i < 30)
                    {
                        hablante = NormalizarHablante(cruda.Substring(0, i));
                        texto = cruda.Substring(i + 1).Trim();
                    }
                    lineas.Add(new LineaConversacion { hablante = hablante, texto = texto });
                }
            }
            else Debug.LogWarning("No se encontró el asset " + d.asset);
            cd.lineas = lineas.ToArray();
        }
    }

    private static string NormalizarHablante(string crudo)
    {
        string c = crudo.Trim();
        if (c.IndexOf("Jorge", StringComparison.OrdinalIgnoreCase) >= 0) return "Jorge";
        if (c.IndexOf("PROTA", StringComparison.OrdinalIgnoreCase) >= 0) return "Héroe";
        if (c.IndexOf("HELENA", StringComparison.OrdinalIgnoreCase) >= 0) return "Helena";
        if (c.Contains("?")) return "???";
        return c.Substring(0, 1).ToUpperInvariant() + c.Substring(1).ToLowerInvariant();
    }

    // =====================================================================================
    //  3) COMBATE TIPO POKEMON
    // =====================================================================================

    private static GameObject CrearCombate(Transform padre, bool boss, out MenuCombatePokemon menu)
    {
        Canvas canvas = NuevoCanvas(boss ? "COMBATE · tipo Pokémon (Boss: Lupus y Helena)" : "COMBATE · tipo Pokémon (encuentros del Bosque)", padre, 30);
        menu = canvas.gameObject.AddComponent<MenuCombatePokemon>();
        menu.modoBoss = boss;
        menu.nombreHeroe = "Héroe";
        menu.hpHeroe = menu.hpMaxHeroe = 87;
        Transform t = canvas.transform;

        // Fondo y marcadores de sprite (para que el equipo ponga ahi al personaje y al enemigo)
        Image fondo = Caja("Fondo", t, new Color32(24, 30, 38, 245), false);
        Estirar(fondo.rectTransform);

        Image marcadorHeroe = Caja("MarcadorSpriteHeroe", t, new Color32(255, 255, 255, 22), false);
        Ancla(marcadorHeroe.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(220, 330), new Vector2(340, 340));
        TextMeshProUGUI mh = Texto("Texto", marcadorHeroe.transform, "Sprite del héroe", 26, new Color32(255, 255, 255, 110), TextAlignmentOptions.Center);
        Estirar(mh.rectTransform);

        Image marcadorEnemigo = Caja("MarcadorSpriteEnemigo", t, new Color32(255, 255, 255, 22), false);
        Ancla(marcadorEnemigo.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-220, -110), new Vector2(340, 340));
        TextMeshProUGUI me = Texto("Texto", marcadorEnemigo.transform, boss ? "Sprites de Lupus y Helena" : "Sprite del enemigo", 26, new Color32(255, 255, 255, 110), TextAlignmentOptions.Center);
        Estirar(me.rectTransform);

        // Paneles de vida
        PanelCombatiente a = PanelEnemigo("PanelEnemigoA", t, new Vector2(60, -60), boss ? "Lupus" : "Enemigo del bosque 1", boss ? 82 : 13);
        PanelCombatiente b = PanelEnemigo("PanelEnemigoB", t, new Vector2(60, -250), "Helena", 35);
        b.raiz.SetActive(boss);
        menu.enemigos = new[] { a, b };

        Image heroePanel = Perg("PanelHeroe", t);
        Ancla(heroePanel.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-60, 370), new Vector2(700, 170));
        TextMeshProUGUI nh = Texto("Nombre", heroePanel.transform, "HÉROE", 40, Tinta, TextAlignmentOptions.Left, true);
        Ancla(nh.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -26), new Vector2(400, 50));
        Image fh = Caja("FondoBarra", heroePanel.transform, new Color32(10, 8, 6, 255), true);
        Ancla(fh.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(44, 40), new Vector2(612, 26));
        Image bh = Barra("Barra", heroePanel.transform, Verde);
        Ancla(bh.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(44, 40), new Vector2(612, 26));
        TextMeshProUGUI hh = Texto("HP", heroePanel.transform, "HP 87/87", 30, Tinta, TextAlignmentOptions.Right);
        Ancla(hh.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-46, -30), new Vector2(300, 44));
        menu.txtNombreHeroe = nh;
        menu.txtHpHeroe = hh;
        menu.barraHeroe = bh;

        // Caja inferior
        Image inferior = Perg("CajaInferior", t);
        Ancla(inferior.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1800, 310));
        Transform ci = inferior.transform;

        // Mensaje (izquierda)
        GameObject cajaMsg = Vacio("CajaMensaje", ci);
        RectTransform rm = cajaMsg.AddComponent<RectTransform>();
        Ancla(rm, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1060, 310));
        TextMeshProUGUI msg = Texto("Mensaje", rm, "¿Qué hará Héroe?", 44, Tinta, TextAlignmentOptions.TopLeft);
        Estirar(msg.rectTransform, 80, 50, 30, 50);

        // Menu principal (derecha): ATACAR / ITEMS
        RectTransform principal = UI("MenuPrincipal", ci);
        Ancla(principal, new Vector2(1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(740, 310));
        Image sep = Caja("Separador", principal, new Color32(110, 70, 30, 255), false);
        Ancla(sep.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(4, 230));
        Button atacar = Boton("BotonAtacar", principal, "ATACAR", new Vector2(310, 100), 42, true);
        Ancla((RectTransform)atacar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(310, 100));
        Button items = Boton("BotonItems", principal, "ITEMS", new Vector2(310, 100), 42, true);
        Ancla((RectTransform)items.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(310, 100));

        // Submenu de ataques
        RectTransform menuAtq = UI("MenuAtaques", ci);
        Estirar(menuAtq);
        Button[] btnAtq = new Button[3];
        string[] nombres = { "Ataque 1", "Ataque 2", "Ataque 3" };
        string[] dados = { "1D10", "1D10+1D4", "1D10+2D4" };
        for (int i = 0; i < 3; i++)
        {
            btnAtq[i] = Boton("BotonAtaque" + (i + 1), menuAtq, nombres[i], new Vector2(430, 100), 40, false);
            Ancla((RectTransform)btnAtq[i].transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(70 + (i % 2) * 560, -55 - (i / 2) * 115), new Vector2(540, 95));
            TextMeshProUGUI d = Texto("Dados", btnAtq[i].transform, dados[i], 28, Dorado, TextAlignmentOptions.Right);
            Ancla(d.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-18, 8), new Vector2(240, 36));
            menu.ataques.Add(new DatosAtaque { nombre = nombres[i], dados = dados[i] });
        }
        Button volverAtq = Boton("BotonVolver", menuAtq, "VOLVER", new Vector2(300, 80), 40, true);
        Ancla((RectTransform)volverAtq.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(630, -170), new Vector2(300, 80));
        TextMeshProUGUI info = Texto("InfoAtaque", menuAtq, "Elige un ataque\n[Esc] volver", 32, Tinta, TextAlignmentOptions.TopLeft);
        Ancla(info.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-70, 0), new Vector2(480, 200));
        info.rectTransform.pivot = new Vector2(1, 0.5f);
        menuAtq.gameObject.SetActive(false);

        // Submenu de items
        RectTransform menuIt = UI("MenuItems", ci);
        Estirar(menuIt);
        Button[] btnIt = new Button[6];
        for (int i = 0; i < 6; i++)
        {
            btnIt[i] = Boton("BotonItem" + (i + 1), menuIt, "Item", new Vector2(540, 80), 34, false);
            int col = i % 3, fila = i / 3;
            Ancla((RectTransform)btnIt[i].transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(70 + col * 540, -55 - fila * 100), new Vector2(540, 80));
        }
        Button volverIt = Boton("BotonVolver", menuIt, "VOLVER", new Vector2(300, 70), 36, true);
        Ancla((RectTransform)volverIt.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-70, 45), new Vector2(300, 70));
        TextMeshProUGUI ayudaIt = Texto("Ayuda", menuIt, "Elige un item para usarlo · [Esc] volver", 28, new Color32(115, 90, 58, 255), TextAlignmentOptions.Left);
        Ancla(ayudaIt.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(80, 50), new Vector2(900, 40));
        menuIt.gameObject.SetActive(false);

        menu.txtMensaje = msg;
        menu.cajaMensaje = cajaMsg;
        menu.menuPrincipal = principal.gameObject;
        menu.botonAtacar = atacar;
        menu.botonItems = items;
        menu.menuAtaques = menuAtq.gameObject;
        menu.botonesAtaque = btnAtq;
        menu.botonVolverAtaques = volverAtq;
        menu.txtInfoAtaque = info;
        menu.menuItems = menuIt.gameObject;
        menu.botonesItem = btnIt;
        menu.botonVolverItems = volverIt;

        // Numeros de dano flotantes
        RectTransform flotantes = UI("NumerosDanoFlotante", t);
        Estirar(flotantes);
        GestorDanoFlotante gd = canvas.gameObject.AddComponent<GestorDanoFlotante>();
        gd.prefabDano = prefabDano;
        gd.contenedorDano = flotantes;
        gd.posicionHeroe = heroePanel.transform;
        gd.posicionEnemigo = a.raiz.transform;
        gd.posicionLupus = a.raiz.transform;
        gd.posicionHelena = b.raiz.transform;

        return canvas.gameObject;
    }

    private static PanelCombatiente PanelEnemigo(string nombre, Transform padre, Vector2 pos, string titulo, int hp)
    {
        Image panel = Perg(nombre, padre);
        Ancla(panel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), pos, new Vector2(700, 170));
        TextMeshProUGUI n = Texto("Nombre", panel.transform, titulo.ToUpperInvariant(), 34, Tinta, TextAlignmentOptions.Left, true);
        n.textWrappingMode = TextWrappingModes.NoWrap;
        Ancla(n.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -28), new Vector2(430, 50));
        Image fb = Caja("FondoBarra", panel.transform, new Color32(10, 8, 6, 255), true);
        Ancla(fb.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(44, 40), new Vector2(612, 26));
        Image barra = Barra("Barra", panel.transform, Verde);
        Ancla(barra.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(44, 40), new Vector2(612, 26));
        TextMeshProUGUI h = Texto("HP", panel.transform, "HP " + hp + "/" + hp, 30, Tinta, TextAlignmentOptions.Right);
        Ancla(h.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-46, -30), new Vector2(300, 44));
        return new PanelCombatiente { nombre = titulo, raiz = panel.gameObject, txtNombre = n, txtHp = h, barra = barra, hp = hp, hpMax = hp };
    }

    // =====================================================================================
    //  MENU (pausa) y OBJETOS (inventario, notificaciones): siempre disponibles
    // =====================================================================================

    private static void CrearPausa(Transform padre)
    {
        Canvas canvas = NuevoCanvas("Menú de pausa (Esc)", padre, 50);
        MenuPausa pausa = canvas.gameObject.AddComponent<MenuPausa>();
        pausa.mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Scripts/Menu/AUDIO/EFECTOS.mixer");
        pausa.escenaMenu = "Menu";
        Transform t = canvas.transform;

        RectTransform panel = UI("Panel", t);
        Estirar(panel);
        Image dim = panel.gameObject.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 170);

        Image caja = Perg("Caja", panel);
        Ancla(caja.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 760));
        TextMeshProUGUI titulo = Texto("Titulo", caja.transform, "PAUSA", 80, TintaNombre, TextAlignmentOptions.Center, true);
        Ancla(titulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(560, 110));

        string[] etiquetas = { "CONTINUAR", "AJUSTES", "MENU PRINCIPAL", "SALIR" };
        Button[] btns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            btns[i] = Boton("Boton" + etiquetas[i].Replace(" ", ""), caja.transform, etiquetas[i], new Vector2(480, 96), 44, true);
            Ancla((RectTransform)btns[i].transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -190 - i * 120), new Vector2(480, 96));
            TextMeshProUGUI et = btns[i].transform.Find("Etiqueta").GetComponent<TextMeshProUGUI>();
            et.alignment = TextAlignmentOptions.Center;
            Estirar(et.rectTransform, 16, 0, 16, 0);
        }
        pausa.botonContinuar = btns[0];
        pausa.botonAjustes = btns[1];
        pausa.botonMenuPrincipal = btns[2];
        pausa.botonSalir = btns[3];

        // Ajustes de volumen
        Image ajustes = Perg("PanelAjustes", panel);
        Ancla(ajustes.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 800));
        TextMeshProUGUI ta = Texto("Titulo", ajustes.transform, "AJUSTES", 72, TintaNombre, TextAlignmentOptions.Center, true);
        Ancla(ta.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(600, 100));
        pausa.sliderMusica = CrearSlider("Musica", ajustes.transform, "Música", -230);
        pausa.sliderEfectos = CrearSlider("Efectos", ajustes.transform, "Efectos", -340);
        Button volver = Boton("BotonVolver", ajustes.transform, "VOLVER", new Vector2(360, 90), 44, true);
        Ancla((RectTransform)volver.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(360, 90));
        pausa.botonVolverAjustes = volver;
        pausa.panelAjustes = ajustes.gameObject;
        ajustes.gameObject.SetActive(false);

        pausa.panel = panel.gameObject;
        panel.gameObject.SetActive(false);
    }

    private static Slider CrearSlider(string nombre, Transform padre, string etiqueta, float y)
    {
        DefaultControls.Resources recursos = new DefaultControls.Resources
        {
            standard = spriteUI,
            background = spriteUI,
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd")
        };
        GameObject go = DefaultControls.CreateSlider(recursos);
        go.name = "Slider" + nombre;
        go.transform.SetParent(padre, false);
        RectTransform r = (RectTransform)go.transform;
        Ancla(r, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(170, y), new Vector2(400, 32));
        Slider s = go.GetComponent<Slider>();
        s.minValue = -60f;
        s.maxValue = 0f;
        s.value = 0f;
        TextMeshProUGUI lbl = Texto("Etiqueta" + nombre, padre, etiqueta, 40, Tinta, TextAlignmentOptions.Left);
        Ancla(lbl.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-200, y), new Vector2(240, 48));
        return s;
    }

    private static void CrearInventario(Transform padre)
    {
        Canvas canvas = NuevoCanvas("Inventario (I / Tab)", padre, 40);
        PanelInventario inv = canvas.gameObject.AddComponent<PanelInventario>();
        Transform t = canvas.transform;

        RectTransform panel = UI("PanelPrincipal", t);
        Estirar(panel);
        Image dim = panel.gameObject.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 150);

        Image ventana = Perg("Ventana", panel);
        Ancla(ventana.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 720));
        TextMeshProUGUI titulo = Texto("Titulo", ventana.transform, "INVENTARIO", 76, TintaNombre, TextAlignmentOptions.Center, true);
        Ancla(titulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(900, 100));
        TextMeshProUGUI pista = Texto("Pista", ventana.transform, "[I] o [Tab] para cerrar", 26, new Color32(115, 90, 58, 255), TextAlignmentOptions.Center);
        Ancla(pista.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(700, 36));

        RectTransform contenedor = UI("ContenedorItems", ventana.transform);
        Ancla(contenedor, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(980, 460));
        GridLayoutGroup grid = contenedor.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(310, 96);
        grid.spacing = new Vector2(16, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        inv.panelPrincipal = panel.gameObject;
        inv.prefabItemUI = prefabItem;
        inv.contenedorItems = contenedor;
        panel.gameObject.SetActive(false);
    }

    private static void CrearNotificaciones(Transform padre)
    {
        Canvas canvas = NuevoCanvas("Notificaciones (objeto recogido, pifia)", padre, 60);
        Notificaciones n = canvas.gameObject.AddComponent<Notificaciones>();
        Transform t = canvas.transform;

        Image toast = Perg("ToastRecoleccion", t);
        Ancla(toast.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -130), new Vector2(460, 112));
        TextMeshProUGUI tt = Texto("Texto", toast.transform, "+1 Poción", 34, Tinta, TextAlignmentOptions.Center);
        Estirar(tt.rectTransform);

        Image pifia = Perg("BannerPifia", t);
        Ancla(pifia.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -190), new Vector2(1000, 150));
        TextMeshProUGUI pt = Texto("Texto", pifia.transform, "!! PIFIA DE HELENA !!", 56, Color.white, TextAlignmentOptions.Center, true);
        Estirar(pt.rectTransform);

        n.panelToast = toast.gameObject;
        n.textoToast = tt;
        n.panelPifia = pifia.gameObject;
        n.textoPifia = pt;
        n.fondoPifia = pifia;
        n.fuenteTitulos = fuenteTitulos;
        toast.gameObject.SetActive(false);
        pifia.gameObject.SetActive(false);
    }

    private static GameObject CrearAyuda(Transform padre)
    {
        Canvas canvas = NuevoCanvas("Ayuda (solo vista previa)", padre, 100);
        TextMeshProUGUI txt = Texto("Texto", canvas.transform,
            "VISTA PREVIA  ·  F1-F4 grupo (Escena 1, 2, 3, Boss)  ·  N escenario normal  ·  C siguiente conversación  ·  K combate  ·  I inventario  ·  Esc pausa",
            22, new Color32(255, 255, 255, 170), TextAlignmentOptions.Center);
        Ancla(txt.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(1800, 30));
        return canvas.gameObject;
    }

    // =====================================================================================
    //  BUILD SETTINGS
    // =====================================================================================

    private static void ConfigurarBuildSettings()
    {
        string[] rutas =
        {
            "Assets/Escenas/Menu.unity",
            "Assets/Scenes/Juego Pueblo.unity",
            "Assets/Scenes/Pueblo Mañana.unity",
            "Assets/Scenes/Bosque.unity",
            "Assets/Interfaz/Interfaz.unity"
        };
        List<EditorBuildSettingsScene> lista = new List<EditorBuildSettingsScene>();
        foreach (string r in rutas)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(r) != null) lista.Add(new EditorBuildSettingsScene(r, true));
            else Debug.LogWarning("[ConstructorInterfaz] Falta la escena " + r);
        }
        EditorBuildSettings.scenes = lista.ToArray();
    }

    // =====================================================================================
    //  HELPERS DE UI
    // =====================================================================================

    private static Sprite ObtenerSpriteBlanco()
    {
        const string ruta = "Assets/Interfaz/Blanco.png";
        if (AssetDatabase.LoadAssetAtPath<Sprite>(ruta) == null)
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            System.IO.File.WriteAllBytes(ruta, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(ruta);
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    private static GameObject Vacio(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        return go;
    }

    private static RectTransform UI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    private static void Estirar(RectTransform r, float izq = 0, float abajo = 0, float der = 0, float arriba = 0)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(izq, abajo);
        r.offsetMax = new Vector2(-der, -arriba);
    }

    private static void Ancla(RectTransform r, Vector2 ancla, Vector2 pivote, Vector2 pos, Vector2 tam)
    {
        r.anchorMin = ancla;
        r.anchorMax = ancla;
        r.pivot = pivote;
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
    }

    private static Canvas NuevoCanvas(string nombre, Transform padre, int orden)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(padre, false);
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = orden;
        CanvasScaler s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight = 0.5f;
        return c;
    }

    private static Image Caja(string nombre, Transform padre, Color color, bool borde)
    {
        RectTransform r = UI(nombre, padre);
        Image img = r.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        if (borde)
        {
            Outline o = r.gameObject.AddComponent<Outline>();
            o.effectColor = Dorado;
            o.effectDistance = new Vector2(3, -3);
        }
        return img;
    }

    private static Image Perg(string nombre, Transform padre)
    {
        RectTransform r = UI(nombre, padre);
        Image img = r.gameObject.AddComponent<Image>();
        img.sprite = ObtenerSpritePergamino();
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = false;
        return img;
    }

    private static Sprite ObtenerSpritePergamino()
    {
        const string ruta = "Assets/Interfaz/Pergamino.png";
        if (AssetDatabase.LoadAssetAtPath<Sprite>(ruta) == null)
        {
            int W = 256, H = 256;
            Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            Color baseC = new Color32(226, 201, 154, 255);
            Color manchaC = new Color32(176, 138, 84, 255);
            Color bordeC = new Color32(104, 66, 30, 255);
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float u = x / (float)W, v = y / (float)H;
                float n = Mathf.PerlinNoise(u * 6f + 3.1f, v * 6f + 7.7f) * 0.55f + Mathf.PerlinNoise(u * 22f, v * 22f) * 0.3f + Mathf.PerlinNoise(u * 90f, v * 90f) * 0.15f;
                float fibra = Mathf.PerlinNoise(u * 4f + 11f, v * 160f) * 0.12f;
                Color c = Color.Lerp(baseC, manchaC, Mathf.Clamp01((n - 0.35f) * 1.6f) * 0.75f);
                c = Color.Lerp(c, manchaC, fibra);
                float dx = Mathf.Min(x, W - 1 - x), dy = Mathf.Min(y, H - 1 - y);
                float d = Mathf.Min(dx, dy);
                float ruido = (Mathf.PerlinNoise(u * 40f + 5f, v * 40f + 9f) - 0.5f) * 14f;
                float dd = d + ruido;
                float quemado = Mathf.Clamp01(1f - dd / 34f);
                quemado = quemado * quemado;
                c = Color.Lerp(c, bordeC, quemado * 0.9f);
                float a = Mathf.Clamp01((dd - 1.5f) / 3f);
                c.a = a;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(ruta, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(ruta);
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spriteBorder = new Vector4(48, 48, 48, 48);
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    private static Image Barra(string nombre, Transform padre, Color color)
    {
        RectTransform r = UI(nombre, padre);
        Image img = r.gameObject.AddComponent<Image>();
        img.sprite = spriteBlanco != null ? spriteBlanco : spriteUI;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = 0;
        img.fillAmount = 1f;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI Texto(string nombre, Transform padre, string contenido, float tam, Color color,
        TextAlignmentOptions alineacion, bool titulo = false)
    {
        RectTransform r = UI(nombre, padre);
        TextMeshProUGUI t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tam;
        t.color = color;
        t.alignment = alineacion;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        if (titulo) { t.fontStyle = FontStyles.Bold; if (fuenteTitulos != null) t.font = fuenteTitulos; }
        return t;
    }

    private static Button Boton(string nombre, Transform padre, string etiqueta, Vector2 tam, float fuente, bool titulo)
    {
        RectTransform r = UI(nombre, padre);
        r.sizeDelta = tam;
        Image img = r.gameObject.AddComponent<Image>();
        img.color = BotonBase;
        Outline o = r.gameObject.AddComponent<Outline>();
        o.effectColor = Dorado;
        o.effectDistance = new Vector2(2, -2);

        Button b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.normalColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cb.selectedColor = Color.white;
        cb.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        cb.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.05f;
        b.colors = cb;

        TextMeshProUGUI txt = Texto("Etiqueta", r, etiqueta, fuente, Color.white, TextAlignmentOptions.Left, titulo);
        Estirar(txt.rectTransform, 84, 0, 16, 0);
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.textWrappingMode = TextWrappingModes.NoWrap;
        txt.overflowMode = TextOverflowModes.Overflow;

        TextMeshProUGUI cursor = Texto("Cursor", r, "►", fuente, Color.white, TextAlignmentOptions.Center);
        Ancla(cursor.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(34, 0), new Vector2(50, tam.y));
        cursor.gameObject.SetActive(false);
        return b;
    }
}
