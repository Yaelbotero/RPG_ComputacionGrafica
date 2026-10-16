using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG.UI
{
    /// <summary>
    /// Muestra la tirada de dados de un ataque, en pantalla y con animacion:
    ///  1) Cae el dado de ataque, rueda y se detiene en una cara.
    ///  2) Aparece un globo: gris con "¡PIFIA!" si fallo, o DORADO con "¡EXITO!" si acerto.
    ///  3) Si acerto y el ataque tiene mas dados, caen los dados de dano y el globo dorado muestra la suma.
    /// Todo se dibuja por codigo (sin imagenes externas) y usa tiempo sin escala, asi que funciona aunque el juego este en pausa.
    /// </summary>
    public class PanelDados : MonoBehaviour
    {
        private TMP_FontAsset fuente;
        private RectTransform raiz;
        private Image fondoOscuro;
        private TextMeshProUGUI titulo;
        private RectTransform globo;
        private Image globoImagen;
        private TextMeshProUGUI globoTexto;
        private RectTransform dadoPrincipal;
        private readonly List<RectTransform> dadosExtra = new List<RectTransform>();

        private static Sprite spriteGloboNormal;
        private static Sprite spriteGloboOro;
        private static readonly Dictionary<int, Sprite> spritesDado = new Dictionary<int, Sprite>();

        private static readonly Color Tinta = new Color32(0x3B, 0x2A, 0x1A, 255);

        /// <summary>Velocidad de la animacion (1 = normal). Subirla hace todo mas rapido.</summary>
        public float velocidad = 1f;

        public static PanelDados Crear(TMP_FontAsset fuenteTexto)
        {
            GameObject go = new GameObject("Panel de dados");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;
            CanvasScaler escala = go.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1280f, 720f);
            escala.matchWidthOrHeight = 0.5f;

            PanelDados panel = go.AddComponent<PanelDados>();
            panel.fuente = fuenteTexto;
            panel.Construir();
            go.SetActive(false);
            return panel;
        }

        // ------------------------------------------------------------ construccion

        private void Construir()
        {
            raiz = (RectTransform)transform;

            fondoOscuro = NuevaImagen("Fondo", raiz, null, new Color(0f, 0f, 0f, 0.55f));
            Estirar(fondoOscuro.rectTransform);
            fondoOscuro.raycastTarget = false;

            titulo = NuevoTexto("Titulo", raiz, 22, Color.white);
            titulo.rectTransform.anchorMin = titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            titulo.rectTransform.sizeDelta = new Vector2(420f, 70f);
            titulo.rectTransform.anchoredPosition = new Vector2(0f, -100f);

            globoImagen = NuevaImagen("Globo", raiz, null, Color.white);
            globo = globoImagen.rectTransform;
            globo.sizeDelta = new Vector2(360f, 180f);
            globo.anchoredPosition = new Vector2(50f, 145f);
            globoImagen.raycastTarget = false;
            globoTexto = NuevoTexto("Texto", globo, 40, Tinta);
            Estirar(globoTexto.rectTransform);
            globoTexto.rectTransform.offsetMin = new Vector2(26f, 54f);
            globoTexto.rectTransform.offsetMax = new Vector2(-26f, -16f);
            globoTexto.enableAutoSizing = true;
            globoTexto.fontSizeMin = 18f;
            globoTexto.fontSizeMax = 40f;
            globoTexto.alignment = TextAlignmentOptions.Center;

            dadoPrincipal = NuevoDado("Dado de ataque", 140f);
        }

        private RectTransform NuevoDado(string nombre, float tam)
        {
            Image img = NuevaImagen(nombre, raiz, null, Color.white);
            img.raycastTarget = false;
            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(tam, tam);
            TextMeshProUGUI num = NuevoTexto("Numero", rt, Mathf.RoundToInt(tam * 0.42f), Tinta);
            Estirar(num.rectTransform);
            return rt;
        }

        private static Image NuevaImagen(string nombre, Transform padre, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            return img;
        }

        private TextMeshProUGUI NuevoTexto(string nombre, Transform padre, int tamano, Color color)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            if (fuente != null) t.font = fuente;
            t.fontSize = tamano;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }

        private static void Estirar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // ------------------------------------------------------------ secuencia publica

        public IEnumerator Mostrar(string textoTitulo, ResultadoDado r)
        {
            gameObject.SetActive(true);
            LimpiarExtras();
            globo.gameObject.SetActive(false);
            dadoPrincipal.gameObject.SetActive(false);
            titulo.text = textoTitulo;

            // Fondo aparece suave.
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                fondoOscuro.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.55f, t / 0.2f));
                yield return null;
            }
            fondoOscuro.color = new Color(0f, 0f, 0f, 0.55f);

            // 1) Dado de ataque.
            bool hayExtras = r.carasExtras != null && r.carasExtras.Length > 0;
            dadoPrincipal.anchoredPosition = new Vector2(0f, -10f);   // el dado de ataque siempre en el centro
            PrepararDado(dadoPrincipal, r.carasPrincipal);
            yield return Tirar(dadoPrincipal, r.carasPrincipal, r.tiradaPrincipal, 1.1f, 140f);

            // 2) Globo de pifia o de exito.
            if (r.pifia)
            {
                MostrarGlobo(false, "¡PIFIA!\nEl ataque falla");
                yield return Aparecer(globo, 0.35f);
                yield return Temblar(globo, 0.5f);
                yield return Esperar(0.9f);
            }
            else
            {
                MostrarGlobo(true, hayExtras ? "¡ÉXITO!\nDados de daño" : "¡ÉXITO!\nDaño: " + r.danio);
                yield return Aparecer(globo, 0.35f);
                yield return Esperar(hayExtras ? 0.8f : 1.1f);

                // 3) Dados de dano y suma.
                if (hayExtras)
                {
                    int n = r.carasExtras.Length;
                    // Mini dados a los lados del dado central: izquierda, derecha, y si hay mas, abajo.
                    Vector2[] sitios = { new Vector2(-115f, 20f), new Vector2(115f, 20f), new Vector2(-65f, -75f), new Vector2(65f, -75f) };
                    for (int i = 0; i < n; i++)
                    {
                        RectTransform d = NuevoDado("Dado de daño " + (i + 1), 76f);
                        dadosExtra.Add(d);
                        d.anchoredPosition = sitios[Mathf.Min(i, sitios.Length - 1)];
                        PrepararDado(d, r.carasExtras[i]);
                    }

                    List<Coroutine> rodando = new List<Coroutine>();
                    for (int i = 0; i < n; i++)
                        rodando.Add(StartCoroutine(Tirar(dadosExtra[i], r.carasExtras[i], r.extras[i], 0.9f + 0.15f * i, 50f)));
                    for (int i = 0; i < rodando.Count; i++) yield return rodando[i];

                    string suma = r.tiradaPrincipal.ToString();
                    for (int i = 0; i < n; i++) suma += "+" + r.extras[i];
                    MostrarGlobo(true, "DAÑO: " + r.danio + "\n(" + suma + ")");
                    yield return Pulso(globo, 0.3f);
                    yield return Esperar(1.2f);
                }
                else
                {
                    yield return Pulso(globo, 0.3f);
                    yield return Esperar(0.4f);
                }
            }

            // Cierre.
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                fondoOscuro.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.55f, 0f, t / 0.2f));
                yield return null;
            }
            LimpiarExtras();
            gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ animaciones

        private IEnumerator Esperar(float segundos)
        {
            yield return new WaitForSecondsRealtime(segundos / Mathf.Max(0.1f, velocidad));
        }

        private void PrepararDado(RectTransform dado, int caras)
        {
            dado.gameObject.SetActive(true);
            dado.GetComponent<Image>().sprite = SpriteDado(caras);
            dado.localScale = Vector3.one;
            dado.localRotation = Quaternion.identity;
            dado.GetComponentInChildren<TextMeshProUGUI>().text = "?";
        }

        /// <summary>El dado cae desde arriba, rebota rodando, cambia de cara y se queda en el resultado.</summary>
        private IEnumerator Tirar(RectTransform dado, int caras, int resultado, float duracion, float altura)
        {
            TextMeshProUGUI num = dado.GetComponentInChildren<TextMeshProUGUI>();
            Vector2 baseY = dado.anchoredPosition;
            float total = duracion / Mathf.Max(0.1f, velocidad);
            float cambio = 0f;
            float giroDir = UnityEngine.Random.value < 0.5f ? -1f : 1f;

            for (float t = 0f; t < total; t += Time.unscaledDeltaTime)
            {
                float u = t / total;
                float rebote = Mathf.Abs(Mathf.Sin(u * Mathf.PI * 3.2f)) * altura * (1f - u) * (1f - u);
                float caida = (1f - Mathf.Clamp01(u * 3f)) * 260f;
                dado.anchoredPosition = baseY + new Vector2(0f, rebote + caida);
                dado.localRotation = Quaternion.Euler(0f, 0f, giroDir * (1f - u) * 900f);
                float s = 1f + 0.12f * Mathf.Sin(u * 20f) * (1f - u);
                dado.localScale = new Vector3(s, s, 1f);

                cambio -= Time.unscaledDeltaTime;
                if (cambio <= 0f)
                {
                    num.text = UnityEngine.Random.Range(1, caras + 1).ToString();
                    cambio = Mathf.Lerp(0.04f, 0.14f, u);
                }
                yield return null;
            }

            dado.anchoredPosition = baseY;
            dado.localRotation = Quaternion.identity;
            num.text = resultado.ToString();

            // Rebote final al caer en la cara elegida.
            for (float t = 0f; t < 0.18f; t += Time.unscaledDeltaTime)
            {
                float s = 1f + 0.3f * Mathf.Sin(t / 0.18f * Mathf.PI);
                dado.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            dado.localScale = Vector3.one;
        }

        private void MostrarGlobo(bool exito, string texto)
        {
            globo.gameObject.SetActive(true);
            globoImagen.sprite = exito ? SpriteGloboOro() : SpriteGloboNormal();
            globoTexto.text = texto;
            globoTexto.color = Tinta;
        }

        private IEnumerator Aparecer(RectTransform rt, float duracion)
        {
            for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
            {
                float u = t / duracion;
                float s = u < 0.7f ? Mathf.Lerp(0f, 1.15f, u / 0.7f) : Mathf.Lerp(1.15f, 1f, (u - 0.7f) / 0.3f);
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private IEnumerator Pulso(RectTransform rt, float duracion)
        {
            for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
            {
                float s = 1f + 0.18f * Mathf.Sin(t / duracion * Mathf.PI);
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private IEnumerator Temblar(RectTransform rt, float duracion)
        {
            Vector2 p = rt.anchoredPosition;
            for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
            {
                float k = 1f - t / duracion;
                rt.anchoredPosition = p + new Vector2(Mathf.Sin(t * 70f) * 12f * k, 0f);
                yield return null;
            }
            rt.anchoredPosition = p;
        }

        private void LimpiarExtras()
        {
            for (int i = 0; i < dadosExtra.Count; i++)
                if (dadosExtra[i] != null) Destroy(dadosExtra[i].gameObject);
            dadosExtra.Clear();
        }

        // ------------------------------------------------------------ sprites dibujados por codigo

        private static Sprite SpriteGloboNormal()
        {
            if (spriteGloboNormal == null)
                spriteGloboNormal = CrearGlobo(new Color32(0xE9, 0xE4, 0xD8, 255), new Color32(0xC9, 0xC2, 0xB2, 255), Tinta);
            return spriteGloboNormal;
        }

        private static Sprite SpriteGloboOro()
        {
            if (spriteGloboOro == null)
                spriteGloboOro = CrearGlobo(new Color32(0xFF, 0xE9, 0x8A, 255), new Color32(0xD4, 0xA0, 0x17, 255), new Color32(0x6B, 0x45, 0x0A, 255));
            return spriteGloboOro;
        }

        private static Sprite CrearGlobo(Color arriba, Color abajo, Color borde)
        {
            const int W = 512, H = 256;
            bool[] dentro = new bool[W * H];
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    bool caja = RectRedondeado(x, y, 14f, 64f, W - 14f, H - 14f, 56f);
                    bool cola = EnTriangulo(x, y, 90f, 70f, 190f, 70f, 70f, 8f);
                    dentro[y * W + x] = caja || cola;
                }
            }
            return Pintar(W, H, dentro, arriba, abajo, borde, 8);
        }

        private static Sprite SpriteDado(int caras)
        {
            Sprite s;
            if (spritesDado.TryGetValue(caras, out s) && s != null) return s;

            const int N = 256;
            bool[] dentro = new bool[N * N];
            float c = N * 0.5f;
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    bool d;
                    if (caras == 4) d = EnTriangulo(x, y, c, N - 18f, 16f, 28f, N - 16f, 28f);
                    else if (caras == 6) d = RectRedondeado(x, y, 24f, 24f, N - 24f, N - 24f, 40f);
                    else d = EnHexagono(x, y, c, c, c - 14f);
                    dentro[y * N + x] = d;
                }
            }
            s = Pintar(N, N, dentro, new Color32(0xF6, 0xEE, 0xD9, 255), new Color32(0xD9, 0xC7, 0x9C, 255), Tinta, 8);
            spritesDado[caras] = s;
            return s;
        }

        private static bool RectRedondeado(float x, float y, float x0, float y0, float x1, float y1, float radio)
        {
            if (x < x0 || x > x1 || y < y0 || y > y1) return false;
            float cx = Mathf.Clamp(x, x0 + radio, x1 - radio);
            float cy = Mathf.Clamp(y, y0 + radio, y1 - radio);
            float dx = x - cx, dy = y - cy;
            return dx * dx + dy * dy <= radio * radio;
        }

        // Triangulo con la punta en (px,py) y la base entre (ax,ay) y (bx,by).
        private static bool EnTriangulo(float x, float y, float ax, float ay, float bx, float by, float px, float py)
        {
            return PuntoEnTriangulo(x, y, ax, ay, bx, by, px, py);
        }

        private static bool PuntoEnTriangulo(float x, float y, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float d1 = (x - x2) * (y1 - y2) - (x1 - x2) * (y - y2);
            float d2 = (x - x3) * (y2 - y3) - (x2 - x3) * (y - y3);
            float d3 = (x - x1) * (y3 - y1) - (x3 - x1) * (y - y1);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        private static bool EnHexagono(float x, float y, float cx, float cy, float r)
        {
            float dx = Mathf.Abs(x - cx), dy = Mathf.Abs(y - cy);
            float ancho = r * 0.866f;
            if (dx > ancho) return false;
            return dy <= r - dx * 0.57735f;
        }

        private static Sprite Pintar(int w, int h, bool[] dentro, Color arriba, Color abajo, Color borde, int grosorBorde)
        {
            Color32[] px = new Color32[w * h];
            int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
            int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (!dentro[i]) { px[i] = new Color32(0, 0, 0, 0); continue; }

                    bool esBorde = false;
                    for (int k = 0; k < 8 && !esBorde; k++)
                    {
                        int nx = x + dx[k] * grosorBorde, ny = y + dy[k] * grosorBorde;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h || !dentro[ny * w + nx]) esBorde = true;
                    }
                    px[i] = esBorde ? (Color32)borde : (Color32)Color.Lerp(abajo, arriba, (float)y / h);
                }
            }

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
