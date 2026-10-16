
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using RPG.UI;
using RPG.Flujo;

public class BossCombat : MonoBehaviour
{
    [Header("Personajes visuales")]
    public SpriteRenderer lupusVisual;
    public SpriteRenderer helenaVisual;

    [Header("Configuracion")]
    public float tiempoEntreTurnos = 1f;

    private CharacterData heroe;
    private CharacterData lupus;
    private CharacterData helena;

    private int ataqueElegido = -1;
    private int objetivoElegido = -1;

    // 0 = esperando
    // 1 = elegir ataque
    // 2 = elegir objetivo
    private int estado = 0;

    private bool combateActivo = false;

    // true cuando la interfaz grafica (menu + dados) maneja el combate.
    private bool usarUI = false;

    [Header("Escena al ganar")]
    public string escenaVictoria = "Final";

    void Start()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError(
                "Inicia desde Bosque para conservar la vida.");
            return;
        }

        heroe = PartyManager.Instance.heroe;

        if (!heroe.IsAlive)
        {
            Debug.Log("El heroe no tiene vida.");
            return;
        }

        List<CharacterData> jefes =
            CharacterDatabase.CreateBosses();

        lupus = jefes[0];
        helena = jefes[1];

        combateActivo = true;

        Debug.Log("=== COMBATE CONTRA JEFES ===");
        MostrarEstado();

        StartCoroutine(ArrancarCombate());
    }

    IEnumerator ArrancarCombate()
    {
        // Panel 0 = Lupus, panel 1 = Helena.
        yield return StartCoroutine(CombateUI.Preparar(
            heroe,
            new string[] { "Lupus", "Helena" },
            new int[] { lupus.currentHealth, helena.currentHealth },
            new int[] { lupus.maxHealth, helena.maxHealth },
            "¡Lupus y Helena bloquean el camino!"));

        usarUI = CombateUI.Activo;

        if (usarUI)
        {
            // Estilo Pokemon: heroe abajo a la izquierda, jefes arriba a la derecha.
            GameObject personaje = GameObject.Find("Personaje");
            CombateUI.ColocarPokemon(
                personaje != null ? personaje.transform : null,
                new Transform[]
                {
                    lupusVisual != null ? lupusVisual.transform : null,
                    helenaVisual != null ? helenaVisual.transform : null
                },
                new float[] { 0.65f, 1.1f });
        }

        yield return StartCoroutine(SistemaTurnos());
    }

    List<CharacterData> Enemigos()
    {
        return new List<CharacterData> { lupus, helena };
    }

    // Ataque con tirada de dados animada (pifia / exito / dano).
IEnumerator AtacarConDados(
        CharacterData atacante, string nombreAtacante,
        CharacterData objetivo, string nombreObjetivo,
        string nombreAtaque, string formula, float velocidad)
    {
        if (!atacante.IsAlive || !objetivo.IsAlive)
            yield break;

        ResultadoDado r = DiceSystem.TirarAtaque(formula);

        yield return StartCoroutine(CombateUI.Tirada(
            nombreAtacante, nombreAtaque, r, velocidad));

        if (r.pifia)
            EventosJuego.NotificarPifia(nombreAtacante);

        yield return StartCoroutine(RPG.UI.CombateEscena.Ataque(
            VisualDe(atacante), VisualDe(objetivo), !r.pifia,
            () =>
            {
                if (!r.pifia)
                {
                    objetivo.TakeDamage(r.danio);
                    EventosJuego.NotificarDanoRecibido(nombreObjetivo, r.danio);
                }
                CombateUI.ActualizarVidas(heroe, Enemigos());
                return !objetivo.IsAlive;
            }));

        if (!objetivo.IsAlive)
        {
            Debug.Log(objetivo.characterName + " DERROTADO");

            if (objetivo == helena && helenaVisual != null)
                helenaVisual.gameObject.SetActive(false);

            if (objetivo == lupus && lupusVisual != null)
                lupusVisual.gameObject.SetActive(false);
        }

        CombateUI.ActualizarVidas(heroe, Enemigos());
        yield return StartCoroutine(CombateUI.EsperarMensajes());
    }

    Transform VisualDe(CharacterData c)
    {
        if (c == heroe)
        {
            GameObject p = GameObject.Find("Personaje");
            return p != null ? p.transform : null;
        }
        if (c == lupus && lupusVisual != null) return lupusVisual.transform;
        if (c == helena && helenaVisual != null) return helenaVisual.transform;
        return null;
    }

    void Update()
    {
        if (!combateActivo || usarUI || Keyboard.current == null)
            return;

        Keyboard teclado = Keyboard.current;
        int numero = -1;

        if (teclado.digit1Key.wasPressedThisFrame ||
            teclado.numpad1Key.wasPressedThisFrame)
            numero = 0;

        else if (teclado.digit2Key.wasPressedThisFrame ||
                 teclado.numpad2Key.wasPressedThisFrame)
            numero = 1;

        else if (teclado.digit3Key.wasPressedThisFrame ||
                 teclado.numpad3Key.wasPressedThisFrame)
            numero = 2;

        if (numero == -1)
            return;

        if (estado == 1 && numero < heroe.attacks.Count)
            ataqueElegido = numero;

        if (estado == 2)
        {
            if (numero == 0 && helena.IsAlive)
                objetivoElegido = 0;

            if (numero == 1 && lupus.IsAlive)
                objetivoElegido = 1;
        }
    }

    IEnumerator SistemaTurnos()
    {
        int ronda = 1;

        while (heroe.IsAlive && (helena.IsAlive || lupus.IsAlive))
        {
            Debug.Log("=== RONDA " + ronda + " ===");

            ataqueElegido = -1;
            objetivoElegido = -1;

            if (usarUI)
            {
                // El jefe a atacar no se elige: Helena primero, luego Lupus.
                CombateUI.Accion accion = new CombateUI.Accion();

                yield return StartCoroutine(CombateUI.ElegirAccion(
                    accion, heroe,
                    new string[] { "Lupus", "Helena" },
                    new int[] { lupus.currentHealth, helena.currentHealth },
                    new int[] { lupus.maxHealth, helena.maxHealth },
                    new bool[] { lupus.IsAlive, helena.IsAlive },
                    false));

                if (accion.tipo == 0)
                {
                    ataqueElegido = accion.indice;

                    bool atacaHelena = helena.IsAlive;
                    if (atacaHelena)
                        CombateUI.Mensaje("Lupus es invulnerable mientras Helena viva.");

                    CharacterData objetivoUI = atacaHelena ? helena : lupus;

                    yield return StartCoroutine(AtacarConDados(
                        heroe, "Héroe",
                        objetivoUI, atacaHelena ? "Helena" : "Lupus",
                        "Ataque " + (ataqueElegido + 1),
                        heroe.attacks[ataqueElegido], 1f));
                }
            }
            else
            {
                Debug.Log("ELIGE TU ATAQUE:");

                for (int i = 0; i < heroe.attacks.Count; i++)
                    Debug.Log((i + 1) + ". " + heroe.attacks[i]);

                estado = 1;

                yield return new WaitUntil(
                    () => ataqueElegido >= 0);

                estado = 0;

                // Helena debe ser derrotada primero.
                if (helena.IsAlive)
                {
                    Debug.Log(
                        "Lupus es invulnerable mientras Helena viva.");
                    objetivoElegido = 0;
                }
                else
                {
                    objetivoElegido = 1;
                }

                CharacterData objetivo =
                    objetivoElegido == 0 ? helena : lupus;

                Atacar(
                    heroe,
                    objetivo,
                    heroe.attacks[ataqueElegido]);
            }

            yield return new WaitForSeconds(
                tiempoEntreTurnos);

            if (!lupus.IsAlive && !helena.IsAlive)
                break;

            // Helena ataca mientras este viva.
            if (helena.IsAlive && heroe.IsAlive)
            {
                if (usarUI)
                    yield return StartCoroutine(AtacarConDados(
                        helena, "Helena", heroe, "Héroe",
                        "Ataque", helena.attacks[0], 1.6f));
                else
                    Atacar(helena, heroe, helena.attacks[0]);

                yield return new WaitForSeconds(
                    tiempoEntreTurnos);
            }

            // Lupus comienza a atacar cuando Helena muere.
            if (!helena.IsAlive && lupus.IsAlive && heroe.IsAlive)
            {
                string ataqueLupus =
                    lupus.attacks[Random.Range(0, lupus.attacks.Count)];

                if (usarUI)
                    yield return StartCoroutine(AtacarConDados(
                        lupus, "Lupus", heroe, "Héroe",
                        "Ataque", ataqueLupus, 1.6f));
                else
                    Atacar(lupus, heroe, ataqueLupus);

                yield return new WaitForSeconds(
                    tiempoEntreTurnos);
            }

            MostrarEstado();
            ronda++;
        }

        combateActivo = false;
        estado = 0;

        if (heroe.IsAlive)
        {
            Debug.Log("=== VICTORIA CONTRA LOS JEFES ===");

            if (usarUI)
            {
                CombateUI.Mensaje("¡Has derrotado a Lupus y Helena!");
                yield return StartCoroutine(CombateUI.EsperarMensajes());
                yield return new WaitForSeconds(1f);
                CombateUI.Terminar(true);
            }

            SceneManager.LoadScene(escenaVictoria);
        }
        else
        {
            Debug.Log("=== DERROTA DEL HEROE ===");

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

    void Atacar(
        CharacterData atacante,
        CharacterData objetivo,
        string formula)
    {
        if (!atacante.IsAlive || !objetivo.IsAlive)
            return;

        int dano = DiceSystem.RollAttack(formula);

        objetivo.TakeDamage(dano);

        Debug.Log(atacante.characterName +
            " ataca a " + objetivo.characterName +
            " | Dano: " + dano +
            " | HP restante: " + objetivo.currentHealth);

        if (!objetivo.IsAlive)
        {
            Debug.Log(objetivo.characterName + " DERROTADO");

            if (objetivo == helena && helenaVisual != null)
                helenaVisual.gameObject.SetActive(false);

            if (objetivo == lupus && lupusVisual != null)
                lupusVisual.gameObject.SetActive(false);
        }
    }

    void MostrarEstado()
    {
        Debug.Log(
            "Heroe: " + heroe.currentHealth + "/" + heroe.maxHealth +
            " | Helena: " + helena.currentHealth + "/" + helena.maxHealth +
            " | Lupus: " + lupus.currentHealth + "/" + lupus.maxHealth);
    }
}
