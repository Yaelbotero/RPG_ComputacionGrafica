
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EncounterCombat : MonoBehaviour
{
    [Header("Configuracion")]
    public float tiempoEntreTurnos = 1f;
    public float esperaAlFinalizar = 2f;

    [Header("Escena de regreso")]
    public string escenaExploracion = "Bosque";

    [Header("Sprites de los cuatro enemigos")]
    public Sprite[] spritesEnemigos = new Sprite[4];

    [Header("Objetos visuales")]
    public SpriteRenderer enemigoVisual1;
    public SpriteRenderer enemigoVisual2;

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

    void Start()
    {
        IniciarCombate();
    }

    void Update()
    {
        if (!CombateActivo)
            return;

        Keyboard teclado = Keyboard.current;

        if (teclado == null)
            return;

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

        if (estado == 1)
        {
            if (numero < heroe.attacks.Count)
                ataqueElegido = numero;
        }
        else if (estado == 2)
        {
            if (numero < enemigos.Count &&
                enemigos[numero].IsAlive)
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
                "Falta PartyManager. Inicia desde Prueba de combate.");
            return;
        }

        // Usar el MISMO heroe entre escenas.
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

        // Cada enemigo puede ser de cualquiera de los 4 tipos.
        for (int i = 0; i < cantidad; i++)
        {
            int tipo = Random.Range(0, 4);

            CharacterData enemigo =
                CharacterDatabase.CreateForestEnemy(tipo);

            enemigos.Add(enemigo);
            tiposEnemigos.Add(tipo);
        }

        ConfigurarSprites();

        CombateActivo = true;

        Debug.Log("=== INICIO DEL COMBATE ===");
        Debug.Log("Cantidad de enemigos: " + cantidad);

        Debug.Log(heroe.characterName +
            " | Fuerza: " + heroe.strength +
            " | Vida: " + heroe.currentHealth +
            "/" + heroe.maxHealth);

        MostrarEnemigos();

        PartyManager.Instance.MostrarInventario();

        StartCoroutine(SistemaTurnos());
    }

    void ConfigurarSprites()
    {
        if (enemigoVisual1 != null)
            enemigoVisual1.gameObject.SetActive(false);

        if (enemigoVisual2 != null)
            enemigoVisual2.gameObject.SetActive(false);

        for (int i = 0; i < enemigos.Count; i++)
        {
            SpriteRenderer visual =
                i == 0 ? enemigoVisual1 : enemigoVisual2;

            int tipo = tiposEnemigos[i];

            if (visual == null)
                continue;

            if (spritesEnemigos != null &&
                tipo < spritesEnemigos.Length &&
                spritesEnemigos[tipo] != null)
            {
                visual.sprite = spritesEnemigos[tipo];
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

            Debug.Log("TURNO DEL HEROE");
            Debug.Log("ELIGE UN ATAQUE:");

            for (int i = 0; i < heroe.attacks.Count; i++)
            {
                Debug.Log((i + 1) + ". " +
                    heroe.attacks[i]);
            }

            estado = 1;

            yield return new WaitUntil(
                () => ataqueElegido >= 0);

            estado = 0;

            string ataque = heroe.attacks[ataqueElegido];

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

            Atacar(heroe, objetivo, ataque);

            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoEntreTurnos));

            if (!HayEnemigosVivos())
                break;

            // Los enemigos vivos atacan automaticamente.
            foreach (CharacterData enemigo in enemigos)
            {
                if (!enemigo.IsAlive || !heroe.IsAlive)
                    continue;

                Debug.Log("TURNO DE " +
                    enemigo.characterName);

                Atacar(enemigo, heroe, enemigo.attacks[0]);

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            ronda++;
        }

        estado = 0;

        yield return StartCoroutine(FinalizarCombate());
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

            int indice = enemigos.IndexOf(objetivo);

            if (indice >= 0)
            {
                SpriteRenderer visual =
                    indice == 0
                    ? enemigoVisual1
                    : enemigoVisual2;

                if (visual != null)
                    visual.gameObject.SetActive(false);
            }
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

        Debug.Log("================================");

        if (heroe.IsAlive)
        {
            Debug.Log("VICTORIA DEL HEROE");

            // Entregar una manzana o pocion.
            PartyManager.Instance.AgregarRecompensa();

            // Guardar el patrullero como derrotado.
            EncounterManager.RegistrarVictoria();

            Debug.Log("Vida conservada: " +
                heroe.currentHealth +
                "/" + heroe.maxHealth);

            Debug.Log("Regresando a exploracion...");

            yield return new WaitForSeconds(
                Mathf.Max(0f, esperaAlFinalizar));

            SceneManager.LoadScene(escenaExploracion);
        }
        else
        {
            Debug.Log("DERROTA DEL HEROE");

            Debug.Log("Vida final: " +
                heroe.currentHealth +
                "/" + heroe.maxHealth);

            Debug.Log("FIN DE LA PARTIDA");

            // No regresar con 0 HP.
        }
    }
}
