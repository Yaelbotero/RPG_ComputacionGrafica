using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.UI
{
    /// <summary>
    /// Escena del combate estilo Pokemon: coloca heroe y enemigos sobre un piso
    /// (apoyando los pies de verdad) y anima los ataques por codigo.
    /// </summary>
    public static class CombateEscena
    {
        public const float PisoHeroeY = -1.5f;
        public const float PisoEnemigosY = 0.85f;
        public const float XHeroe = -4.7f;

        private static Sprite spritePlataforma;
        private static Sprite spriteHalo;
        private static readonly Dictionary<Sprite, float> cachePies = new Dictionary<Sprite, float>();

        // ---------------------------------------------------------------
        //  COLOCAR
        // ---------------------------------------------------------------
        public static void Colocar(Transform heroe, Transform[] enemigos, float[] escalas)
        {
            if (heroe != null)
            {
                // Con 2 enemigos hay 2 barras de vida y el heroe se achica para no chocar con ellas.
                int vivosIni = 0;
                if (enemigos != null) for (int k = 0; k < enemigos.Length; k++) if (enemigos[k] != null) vivosIni++;
                heroe.localScale = heroe.localScale * (vivosIni > 1 ? 1.08f : 1.28f);
                SpriteRenderer sh = heroe.GetComponent<SpriteRenderer>();
                ApoyarEnPiso(heroe, sh, XHeroe, PisoHeroeY);
                if (sh != null) sh.sortingOrder = 6;
                CrearHalo(XHeroe, PisoHeroeY, 3.4f, 5);
                CrearPlataforma(XHeroe, PisoHeroeY, 3.4f, 0.8f, 5);
            }

            if (enemigos == null) return;

            int n = 0;
            for (int i = 0; i < enemigos.Length; i++) if (enemigos[i] != null) n++;

            float[] xs = n > 1 ? new float[] { 6.0f, 3.2f } : new float[] { 4.8f };

            for (int i = 0; i < enemigos.Length && i < xs.Length; i++)
            {
                Transform e = enemigos[i];
                if (e == null) continue;

                if (escalas != null && i < escalas.Length && escalas[i] > 0f)
                    e.localScale = e.localScale * escalas[i];

                SpriteRenderer se = e.GetComponent<SpriteRenderer>();
                if (se != null) se.color = Color.white;
                ApoyarEnPiso(e, se, xs[i], PisoEnemigosY);
                if (se != null) se.sortingOrder = 5;
                CrearHalo(xs[i], PisoEnemigosY, 3.0f, 5);
            }

            // Un solo piso ancho para todos los enemigos, al mismo nivel.
            if (n > 1) CrearPlataforma((xs[0] + xs[1]) * 0.5f, PisoEnemigosY, 8.2f, 0.9f, 5);
            else CrearPlataforma(xs[0], PisoEnemigosY, 4.4f, 0.8f, 5);
        }

        /// <summary>Mueve el personaje para que su pixel mas bajo toque piesY.</summary>
        private static void ApoyarEnPiso(Transform t, SpriteRenderer sr, float x, float piesY)
        {
            Vector3 p = t.position;
            p.x = x;
            t.position = p;
            if (sr == null) return;

            float actual = PiesMundoY(sr);
            p = t.position;
            p.y += piesY - actual;
            t.position = p;
        }

        /// <summary>Altura mundial del pixel visible mas bajo del sprite actual.</summary>
        private static float PiesMundoY(SpriteRenderer sr)
        {
            Sprite s = sr.sprite;
            if (s == null) return sr.bounds.min.y;

            float filaMin;
            if (!cachePies.TryGetValue(s, out filaMin))
            {
                filaMin = 0f;
                try
                {
                    Texture2D tex = s.texture;
                    RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0,
                        RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    Graphics.Blit(tex, rt);
                    RenderTexture previa = RenderTexture.active;
                    RenderTexture.active = rt;

                    Rect r = s.textureRect;
                    int w = (int)r.width, h = (int)r.height;
                    Texture2D leido = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    leido.ReadPixels(new Rect(r.x, r.y, w, h), 0, 0);
                    leido.Apply();

                    RenderTexture.active = previa;
                    RenderTexture.ReleaseTemporary(rt);

                    Color32[] px = leido.GetPixels32();
                    UnityEngine.Object.Destroy(leido);

                    bool hallado = false;
                    for (int y = 0; y < h && !hallado; y++)
                    {
                        int cuenta = 0;
                        for (int x = 0; x < w; x++)
                            if (px[y * w + x].a > 40) cuenta++;
                        if (cuenta >= 3) { filaMin = y; hallado = true; }
                    }
                }
                catch (Exception) { filaMin = 0f; }
                cachePies[s] = filaMin;
            }

            float ppu = s.pixelsPerUnit;
            return sr.transform.position.y + (filaMin - s.pivot.y) / ppu * sr.transform.lossyScale.y;
        }

        private static void CrearHalo(float x, float piesY, float ancho, int orden)
        {
            if (spriteHalo == null)
            {
                const int N = 128;
                Color32[] hp = new Color32[N * N];
                for (int y = 0; y < N; y++)
                    for (int x2 = 0; x2 < N; x2++)
                    {
                        float dx = (x2 - N * 0.5f) / (N * 0.5f), dy = (y - N * 0.5f) / (N * 0.5f);
                        float d = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                        hp[y * N + x2] = new Color(0.75f, 0.85f, 1f, d * d * 0.35f);
                    }
                Texture2D ht = new Texture2D(N, N, TextureFormat.RGBA32, false);
                ht.filterMode = FilterMode.Bilinear;
                ht.SetPixels32(hp);
                ht.Apply();
                spriteHalo = Sprite.Create(ht, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
            }

            GameObject halo = new GameObject("Halo");
            halo.transform.position = new Vector3(x, piesY + ancho * 0.45f, 0f);
            halo.transform.localScale = Vector3.one * (ancho * 1.4f / 1.28f);
            SpriteRenderer hr = halo.AddComponent<SpriteRenderer>();
            hr.sprite = spriteHalo;
            hr.sortingOrder = orden - 3;
        }

        private static void CrearPlataforma(float x, float piesY, float ancho, float alto, int orden)
        {
            if (spritePlataforma == null)
            {
                const int W = 256, H = 64;
                Color32[] px = new Color32[W * H];
                for (int y = 0; y < H; y++)
                {
                    for (int x2 = 0; x2 < W; x2++)
                    {
                        float dx = (x2 - W * 0.5f) / (W * 0.5f), dy = (y - H * 0.5f) / (H * 0.5f);
                        float d = dx * dx + dy * dy;
                        if (d > 1f) { px[y * W + x2] = new Color32(0, 0, 0, 0); continue; }
                        bool borde = d > 0.86f;
                        Color c = borde ? new Color(0.20f, 0.24f, 0.30f, 0.9f)
                                        : Color.Lerp(new Color(0.62f, 0.70f, 0.78f, 0.7f), new Color(0.40f, 0.47f, 0.56f, 0.7f), d);
                        px[y * W + x2] = c;
                    }
                }
                Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.SetPixels32(px);
                tex.Apply();
                spritePlataforma = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f);
            }

            GameObject go = new GameObject("Plataforma");
            go.transform.position = new Vector3(x, piesY, 0f);
            go.transform.localScale = new Vector3(ancho / 2.56f, alto / 0.64f, 1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spritePlataforma;
            sr.sortingOrder = orden - 1;
        }

        // ---------------------------------------------------------------
        //  ANIMACIONES
        // ---------------------------------------------------------------

        /// <summary>
        /// Embestida del atacante hacia el objetivo. En el impacto llama a alImpactar
        /// (que aplica el dano y devuelve true si el objetivo murio). Despues el
        /// objetivo se sacude (o cae si murio) y el atacante vuelve a su sitio.
        /// </summary>
        public static IEnumerator Ataque(Transform atacante, Transform objetivo, bool acierta, Func<bool> alImpactar)
        {
            Vector3 ini = atacante != null ? atacante.position : Vector3.zero;

            if (atacante != null)
            {
                Disparar(atacante, "Atacar");
                Vector3 haciaObj = objetivo != null ? objetivo.position - ini : Vector3.right;
                haciaObj.z = 0f;
                Vector3 dir = haciaObj.normalized;
                Vector3 atras = ini - dir * 0.3f;
                Vector3 dest = ini + haciaObj * (acierta ? 0.5f : 0.22f);

                yield return Mover(atacante, ini, atras, 0.12f);
                yield return Mover(atacante, atras, dest, 0.12f);
            }

            bool murio = false;
            if (alImpactar != null) murio = alImpactar();

            if (acierta && objetivo != null)
            {
                if (murio) yield return Caer(objetivo);
                else yield return Golpe(objetivo);
            }
            else
            {
                yield return new WaitForSeconds(0.15f);
            }

            if (atacante != null)
            {
                Vector3 desde = atacante.position;
                yield return Mover(atacante, desde, ini, 0.2f);
            }
        }

        private static IEnumerator Mover(Transform t, Vector3 a, Vector3 b, float dur)
        {
            float k = 0f;
            while (k < 1f)
            {
                if (t == null) yield break;
                k += Time.deltaTime / Mathf.Max(0.01f, dur);
                float e = 1f - (1f - Mathf.Clamp01(k)) * (1f - Mathf.Clamp01(k));
                t.position = Vector3.Lerp(a, b, e);
                yield return null;
            }
            if (t != null) t.position = b;
        }

        /// <summary>Sacudida horizontal + destello rojo.</summary>
        public static IEnumerator Golpe(Transform t)
        {
            if (t == null) yield break;
            Disparar(t, "RecibirDano");
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            Color orig = sr != null ? sr.color : Color.white;
            Vector3 p0 = t.position;
            float tiempo = 0f;
            while (tiempo < 0.35f)
            {
                if (t == null) yield break;
                tiempo += Time.deltaTime;
                float s = Mathf.Sin(tiempo * 60f) * 0.14f * (1f - tiempo / 0.35f);
                t.position = p0 + new Vector3(s, 0f, 0f);
                if (sr != null)
                    sr.color = Color.Lerp(new Color(1f, 0.35f, 0.35f, 1f), orig, tiempo / 0.35f);
                yield return null;
            }
            if (t != null) t.position = p0;
            if (sr != null) sr.color = orig;
        }

        /// <summary>Muerte: el heroe usa su animacion Death; los demas se hunden y se desvanecen.</summary>
        public static IEnumerator Caer(Transform t)
        {
            if (t == null) yield break;
            if (Disparar(t, "Morir"))
            {
                yield return new WaitForSeconds(1.1f);
                yield break;
            }

            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            Color orig = sr != null ? sr.color : Color.white;
            Vector3 p0 = t.position;
            float tiempo = 0f;
            while (tiempo < 0.6f)
            {
                if (t == null) yield break;
                tiempo += Time.deltaTime;
                float k = tiempo / 0.6f;
                t.position = p0 + new Vector3(0f, -0.35f * k, 0f);
                if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f - k);
                yield return null;
            }
        }

        /// <summary>Dispara un trigger del Animator solo si existe en su controlador.</summary>
        private static bool Disparar(Transform t, string trigger)
        {
            Animator an = t.GetComponent<Animator>();
            if (an == null || !an.isActiveAndEnabled || an.runtimeAnimatorController == null) return false;
            foreach (AnimatorControllerParameter p in an.parameters)
            {
                if (p.name == trigger && p.type == AnimatorControllerParameterType.Trigger)
                {
                    an.SetTrigger(trigger);
                    return true;
                }
            }
            return false;
        }
    }
}
