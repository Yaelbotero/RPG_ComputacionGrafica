
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using RPG.UI;
using RPG.Flujo;

public class EncounterCombat : MonoBehaviour
{
    [Header("Configuracion")]
    public float tiempoEntreTurnos = 1f;
    public float esperaAlFinalizar = 2f;
    public float esperaAnimacionAtaque = 0.5f;
    public float esperaAnimacionDano = 0.5f;
    public float esperaAnimacionMuerte = 1f;

    [Header("Escena de regreso")]
    public string escenaExploracion = "Bosque";

    [Header("Sprites de los cuatro enemigos")]
    public Sprite[] spritesEnemigos = new Sprite[4];

    [Header("Animator Controllers por tipo")]
    public RuntimeAnimatorController[] controladoresEnemigos =
        new RuntimeAnimatorController[4];

    [Header("Objetos visuales")]
    public SpriteRenderer enemigoVisual1;
    public SpriteRenderer enemigoVisual2;

    [Header("Animators de los objetos visuales")]
    public Animator animatorEnemigo1;
    public Animator animatorEnemigo2;

    // Tipos de enemigo que pueden aparecer en el bosque (sin la bruja = Helena).
    private static readonly int[] TiposBosque = { 0, 1, 3 };

    private CharacterData heroe;
    private List<CharacterData> enemigos;
    private List<int> tiposEnemigos;

    private int ataqueElegido = -1;
    private int objetivoElegido = -1;

    // 0 = esperando
    // 1 = elegir ataque
    // 2 = elegir objetivo
    private int estado = 0;

    public bool CombateActivo { get; private set; }

    // true cuando la interfaz grafica (menu + dados) maneja el combate.
    private bool usarUI = false;

    void Start()
    {
        // En la escena de jefes manda BossCombat: este combate del bosque no debe arrancar.
        if (FindFirstObjectByType<BossCombat>() != null)
        {
            enabled = false;
            return;
        }

        IniciarCombate();
    }

    void Update()
    {
        if (!CombateActivo || usarUI || Keyboard.current == null)
            return;

        Keyboard teclado = Keyboard.current;
        int numero = -1;

        if (teclado.digit1Key.wasPressedThisFrame ||
            teclado.numpad1Key.wasPressedThisFrame)
        {
            numero = 0;
        }
        else if (teclado.digit2Key.wasPressedThisFrame ||
                 teclado.numpad2Key.wasPressedThisFrame)
        {
            numero = 1;
        }
        else if (teclado.digit3Key.wasPressedThisFrame ||
                 teclado.numpad3Key.wasPressedThisFrame)
        {
            numero = 2;
        }

        if (numero == -1)
            return;

        if (estado == 1 && numero < heroe.attacks.Count)
        {
            ataqueElegido = numero;
        }
        else if (estado == 2 &&
                 numero < enemigos.Count &&
                 enemigos[numero].IsAlive)
        {
            objetivoElegido = numero;
        }
    }

    public void IniciarCombate()
    {
        if (CombateActivo)
            return;

        if (PartyManager.Instance == null)
        {
            Debug.LogError(
                "Falta PartyManager. Inicia desde Pueblo.");
            return;
        }

        heroe = PartyManager.Instance.heroe;

        if (heroe == null || !heroe.IsAlive)
        {
            Debug.Log("El heroe no puede combatir.");
            return;
        }

        enemigos = new List<CharacterData>();
        tiposEnemigos = new List<int>();

        int cantidad = Mathf.Clamp(
            EncounterManager.cantidadEnemigos, 1, 2);

        // Solo se elige aleatoriamente el tipo.
        // Sus estadisticas siempre son fijas.
        for (int i = 0; i < cantidad; i++)
        {
            // El tipo 2 (la bruja) es Helena, enemiga especial de la pelea final: no sale en el bosque.
            int tipo = TiposBosque[Random.Range(0, TiposBosque.Length)];

            CharacterData enemigo =
                CharacterDatabase.CreateForestEnemy(tipo);

            enemigos.Add(enemigo);
            tiposEnemigos.Add(tipo);
        }

        ConfigurarEnemigosVisuales();

        CombateActivo = true;

        Debug.Log("=== INICIO DEL COMBATE ===");
        Debug.Log("Cantidad de enemigos: " + cantidad);

        Debug.Log(heroe.characterName +
            " | Fuerza: " + heroe.strength +
            " | HP: " + heroe.currentHealth +
            "/" + heroe.maxHealth);

        MostrarEnemigos();
        PartyManager.Instance.MostrarInventario();

        StartCoroutine(ArrancarCombate());
    }

    IEnumerator ArrancarCombate()
    {
        yield return StartCoroutine(CombateUI.Preparar(
            heroe, NombresVisibles(), VidasActuales(), VidasMaximas(),
            enemigos.Count > 1
                ? "¡Dos enemigos del bosque bloquean el camino!"
                : "¡Un enemigo del bosque bloquea el camino!"));

        usarUI = CombateUI.Activo;

        if (usarUI)
        {
            // Estilo Pokemon: heroe abajo a la izquierda, enemigos arriba a la derecha.
            GameObject personaje = GameObject.Find("Personaje");
            Transform[] visuales = enemigos.Count > 1
                ? new Transform[] { enemigoVisual1.transform, enemigoVisual2.transform }
                : new Transform[] { enemigoVisual1.transform };
            CombateUI.ColocarPokemon(
                personaje != null ? personaje.transform : null, visuales,
                enemigos.Count > 1
                    ? new float[] { 0.9f, 0.9f }
                    : new float[] { 1f });
        }

        yield return StartCoroutine(SistemaTurnos());
    }

    string[] NombresVisibles()
    {
        // Se quita el prefijo "Enemigo " para que el nombre quepa en el recuadro.
        string[] n = new string[enemigos.Count];
        for (int i = 0; i < n.Length; i++)
        {
            string baseNombre = enemigos[i].characterName.Replace("Enemigo ", "");
            n[i] = enemigos.Count > 1
                ? baseNombre + " " + (char)('A' + i)
                : baseNombre;
        }
        return n;
    }

    int[] VidasActuales()
    {
        int[] v = new int[enemigos.Count];
        for (int i = 0; i < v.Length; i++) v[i] = enemigos[i].currentHealth;
        return v;
    }

    int[] VidasMaximas()
    {
        int[] v = new int[enemigos.Count];
        for (int i = 0; i < v.Length; i++) v[i] = enemigos[i].maxHealth;
        return v;
    }

    bool[] EnemigosVivos()
    {
        bool[] v = new bool[enemigos.Count];
        for (int i = 0; i < v.Length; i++) v[i] = enemigos[i].IsAlive;
        return v;
    }

    // Ataque con tirada de dados animada (pifia / exito / dano).
IEnumerator AtacarConDados(
        CharacterData atacante, string nombreAtacante,
        CharacterData objetivo, string nombreObjetivo,
        string nombreAtaque, string formula, float velocidad)
    {
        ResultadoDado r = DiceSystem.TirarAtaque(formula);

        yield return StartCoroutine(CombateUI.Tirada(
            nombreAtacante, nombreAtaque, r, velocidad));

        if (r.pifia)
        {
            Debug.Log(nombreAtacante + " pifia (" + r.tiradaPrincipal + ")");
            EventosJuego.NotificarPifia(nombreAtacante);
        }

        // Embestida + golpe/caida, aplicando el dano justo en el impacto.
        yield return StartCoroutine(RPG.UI.CombateEscena.Ataque(
            VisualDe(atacante), VisualDe(objetivo), !r.pifia,
            () =>
            {
                if (!r.pifia)
                {
                    objetivo.TakeDamage(r.danio);
                    Debug.Log(nombreAtacante + " causa " + r.danio + " a " + nombreObjetivo);
                    EventosJuego.NotificarDanoRecibido(nombreObjetivo, r.danio);
                }
                CombateUI.ActualizarVidas(heroe, enemigos);
                return !objetivo.IsAlive;
            }));

        // Un enemigo derrotado puede soltar una pocion o una manzana.
        if (!objetivo.IsAlive && objetivo != heroe && PartyManager.Instance != null)
        {
            string botin = PartyManager.Instance.SoltarBotin();
            if (botin != null)
                CombateUI.Mensaje(nombreObjetivo + " soltó: " + botin + " (+1 al inventario).");
        }

        CombateUI.ActualizarVidas(heroe, enemigos);
        yield return StartCoroutine(CombateUI.EsperarMensajes());
    }

    // Objeto visual (sprite) de un personaje del combate.
    Transform VisualDe(CharacterData c)
    {
        if (c == heroe)
        {
            GameObject p = GameObject.Find("Personaje");
            return p != null ? p.transform : null;
        }
        int i = enemigos.IndexOf(c);
        SpriteRenderer v = i >= 0 ? ObtenerVisual(i) : null;
        return v != null ? v.transform : null;
    }

    void ConfigurarEnemigosVisuales()
    {
        // Configurar ambos espacios visuales.
        for (int i = 0; i < 2; i++)
        {
            SpriteRenderer visual = ObtenerVisual(i);
            Animator animator = ObtenerAnimator(i);

            if (visual == null)
                continue;

            // Evitar que quede el controlador del
            // enemigo anterior en esta posicion.
            if (animator != null)
            {
                animator.runtimeAnimatorController = null;
            }

            visual.gameObject.SetActive(false);

            if (i >= enemigos.Count)
                continue;

            int tipo = tiposEnemigos[i];

            // Asignar el sprite base del tipo.
            if (spritesEnemigos != null &&
                tipo < spritesEnemigos.Length &&
                spritesEnemigos[tipo] != null)
            {
                visual.sprite = spritesEnemigos[tipo];
            }

            // Asignar su Animator Controller.
            if (animator != null &&
                controladoresEnemigos != null &&
                tipo < controladoresEnemigos.Length &&
                controladoresEnemigos[tipo] != null)
            {
                animator.runtimeAnimatorController =
                    controladoresEnemigos[tipo];
            }

            visual.gameObject.SetActive(true);
        }
    }

    IEnumerator SistemaTurnos()
    {
        int ronda = 1;

        while (heroe.IsAlive && HayEnemigosVivos())
        {
            Debug.Log("=== RONDA " + ronda + " ===");

            ataqueElegido = -1;
            objetivoElegido = -1;

            if (usarUI)
            {
                // TURNO DEL HEROE con menu e interfaz.
                CombateUI.Accion accion = new CombateUI.Accion();

                yield return StartCoroutine(CombateUI.ElegirAccion(
                    accion, heroe, NombresVisibles(), VidasActuales(),
                    VidasMaximas(), EnemigosVivos(), true));

                // Si uso un objeto (tipo 1) ya se curo y pierde el turno.
                if (accion.tipo == 0)
                {
                    ataqueElegido = accion.indice;
                    objetivoElegido = accion.objetivo;

                    CharacterData objetivoUI = enemigos[objetivoElegido];
                    string[] nombres = NombresVisibles();

                    yield return StartCoroutine(AtacarConDados(
                        heroe, "Héroe", objetivoUI, nombres[objetivoElegido],
                        "Ataque " + (ataqueElegido + 1),
                        heroe.attacks[ataqueElegido], 1f));

                    yield return StartCoroutine(AnimarResultadoEnemigo(
                        objetivoElegido, !objetivoUI.IsAlive));
                }
            }
            else
            {
                Debug.Log("TURNO DEL HEROE");
                Debug.Log("ELIGE UN ATAQUE:");

                for (int i = 0; i < heroe.attacks.Count; i++)
                {
                    Debug.Log((i + 1) + ". " + heroe.attacks[i]);
                }

                estado = 1;

                yield return new WaitUntil(
                    () => ataqueElegido >= 0);

                estado = 0;

                string ataque = heroe.attacks[ataqueElegido];

                // Contar enemigos vivos.
                int cantidadVivos = 0;
                int unicoVivo = -1;

                for (int i = 0; i < enemigos.Count; i++)
                {
                    if (enemigos[i].IsAlive)
                    {
                        cantidadVivos++;
                        unicoVivo = i;
                    }
                }

                if (cantidadVivos == 1)
                {
                    objetivoElegido = unicoVivo;
                }
                else
                {
                    Debug.Log("ELIGE OBJETIVO:");
                    MostrarEnemigos();
                    Debug.Log("Pulsa 1 o 2.");

                    estado = 2;

                    yield return new WaitUntil(
                        () => objetivoElegido >= 0);

                    estado = 0;
                }

                CharacterData objetivo =
                    enemigos[objetivoElegido];

                // ATAQUE DEL HEROE
                Atacar(heroe, objetivo, ataque);

                // Hurt si sobrevive, Death si muere.
                yield return StartCoroutine(
                    AnimarResultadoEnemigo(
                        objetivoElegido,
                        !objetivo.IsAlive));
            }

            if (!HayEnemigosVivos())
                break;

            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoEntreTurnos));

            // TURNOS DE LOS ENEMIGOS
            for (int i = 0; i < enemigos.Count; i++)
            {
                CharacterData enemigo = enemigos[i];

                if (!enemigo.IsAlive || !heroe.IsAlive)
                    continue;

                Debug.Log("TURNO DE " +
                    enemigo.characterName);

                // Reproducir el ataque del tipo correcto.
                ActivarAnimacion(i, "Atacar");

                yield return new WaitForSeconds(
                    Mathf.Max(0f, esperaAnimacionAtaque));

                if (usarUI)
                {
                    yield return StartCoroutine(AtacarConDados(
                        enemigo, NombresVisibles()[i],
                        heroe, "Héroe",
                        "Ataque", enemigo.attacks[0], 1.6f));
                }
                else
                {
                    Atacar(enemigo, heroe, enemigo.attacks[0]);
                }

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            ronda++;
        }

        estado = 0;

        yield return StartCoroutine(FinalizarCombate());
    }

    Animator ObtenerAnimator(int indice)
    {
        return indice == 0
            ? animatorEnemigo1
            : animatorEnemigo2;
    }

    SpriteRenderer ObtenerVisual(int indice)
    {
        return indice == 0
            ? enemigoVisual1
            : enemigoVisual2;
    }

    void ActivarAnimacion(int indice, string trigger)
    {
        Animator animator = ObtenerAnimator(indice);

        if (animator == null ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null)
        {
            return;
        }

        // Evitar advertencias si el controlador
        // no contiene el parametro solicitado.
        foreach (AnimatorControllerParameter parametro
                 in animator.parameters)
        {
            if (parametro.name == trigger &&
                parametro.type ==
                AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(trigger);
                return;
            }
        }
    }

    IEnumerator AnimarResultadoEnemigo(
        int indice,
        bool murio)
    {
        SpriteRenderer visual = ObtenerVisual(indice);

        if (murio)
        {
            ActivarAnimacion(indice, "Morir");

            yield return new WaitForSeconds(
                Mathf.Max(0f, esperaAnimacionMuerte));

            if (visual != null)
                visual.gameObject.SetActive(false);
        }
        else
        {
            ActivarAnimacion(indice, "RecibirDano");

            yield return new WaitForSeconds(
                Mathf.Max(0f, esperaAnimacionDano));
        }
    }

    void Atacar(
        CharacterData atacante,
        CharacterData objetivo,
        string formula)
    {
        if (!atacante.IsAlive || !objetivo.IsAlive)
            return;

        Debug.Log(atacante.characterName +
            " ataca a " + objetivo.characterName);

        Debug.Log("Ataque: " + formula);

        int dano = DiceSystem.RollAttack(formula);

        objetivo.TakeDamage(dano);

        Debug.Log("Dano causado: " + dano);

        Debug.Log(objetivo.characterName +
            " | HP: " + objetivo.currentHealth +
            "/" + objetivo.maxHealth);

        if (!objetivo.IsAlive)
        {
            Debug.Log(objetivo.characterName +
                " DERROTADO");
        }
    }

    bool HayEnemigosVivos()
    {
        return enemigos.Exists(e => e.IsAlive);
    }

    void MostrarEnemigos()
    {
        Debug.Log("=== ENEMIGOS ===");

        for (int i = 0; i < enemigos.Count; i++)
        {
            CharacterData enemigo = enemigos[i];

            Debug.Log((i + 1) + ". " +
                enemigo.characterName +
                " | Fuerza: " + enemigo.strength +
                " | HP: " + enemigo.currentHealth +
                "/" + enemigo.maxHealth +
                (enemigo.IsAlive ? "" : " (DERROTADO)"));
        }
    }

    IEnumerator FinalizarCombate()
    {
        CombateActivo = false;

        if (heroe.IsAlive)
        {
            Debug.Log("=== VICTORIA ===");

            if (usarUI)
            {
                CombateUI.Mensaje("¡Victoria!");
                yield return StartCoroutine(CombateUI.EsperarMensajes());
            }

            if (!usarUI) PartyManager.Instance.AgregarRecompensa();
            EncounterManager.RegistrarVictoria();

            Debug.Log("Vida conservada: " +
                heroe.currentHealth +
                "/" + heroe.maxHealth);

            yield return new WaitForSeconds(
                Mathf.Max(0f, esperaAlFinalizar));

            if (usarUI) CombateUI.Terminar(true);

            SceneManager.LoadScene(escenaExploracion);
        }
        else
        {
            Debug.Log("=== DERROTA ===");
            Debug.Log("FIN DE LA PARTIDA");

            if (usarUI)
            {
                CombateUI.Mensaje("Héroe ha caído... Fin de la partida.");
                yield return StartCoroutine(CombateUI.EsperarMensajes());
                yield return new WaitForSeconds(1f);
                CombateUI.Terminar(false);
            }

            PartyManager.Instance.ReiniciarPartida();
            EncounterManager.ReiniciarEncuentros();
            SceneManager.LoadScene("Menu");
        }
    }
}
