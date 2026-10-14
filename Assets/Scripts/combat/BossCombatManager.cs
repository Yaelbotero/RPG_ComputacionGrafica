
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCombatManager : MonoBehaviour
{
    [Header("Configuracion")]
    public float tiempoEntreTurnos = 1f;

    [Header("Regeneracion de Lupus")]
    [Range(0f, 1f)]
    public float probabilidadCuracion = 0.25f;

    public int carasDadoCuracion = 6;

    private List<CharacterData> heroes;
    private List<CharacterData> bosses;
    private BossAI bossAI;

    private CharacterData lupus;
    private CharacterData helena;

    void Start()
    {
        IniciarCombate();
    }

    void IniciarCombate()
    {

        // Utilizar los heroes almacenados en PartyManager.
        if (PartyManager.Instance != null)
        {
            heroes = PartyManager.Instance.heroes;
        }
        else
        {
            // Permite probar BossTest independientemente.
            heroes = CharacterDatabase.CreateHeroes();
        }

        // Comprobar si quedan heroes vivos.
        if (!heroes.Exists(heroe => heroe.IsAlive))
        {
            Debug.Log("No quedan heroes vivos para combatir.");
            return;
        }

        // Crear a Lupus y Helena.
        bosses = CharacterDatabase.CreateBosses();


        lupus = bosses[0];
        helena = bosses[1];

        bossAI = new BossAI(
            probabilidadCuracion,
            carasDadoCuracion);

        Debug.Log("================================");
        Debug.Log("COMBATE FINAL: LUPUS Y HELENA");
        Debug.Log("================================");

        Debug.Log("FASE 1: DERROTAR A HELENA");
        Debug.Log("Lupus esta protegido.");
        Debug.Log("Lupus no puede recibir dano hasta que Helena muera.");

        Debug.Log("Lupus HP: " + lupus.currentHealth);
        Debug.Log("Helena HP: " + helena.currentHealth);

        StartCoroutine(SistemaTurnos());
    }

    IEnumerator SistemaTurnos()
    {
        int ronda = 1;
        bool segundaFaseAnunciada = false;

        while (HayVivos(heroes) && lupus.IsAlive)
        {
            Debug.Log("========== RONDA " + ronda + " ==========");

            // Turnos de los heroes.
            foreach (CharacterData hero in heroes)
            {
                if (!hero.IsAlive) continue;
                if (!lupus.IsAlive) break;

                CharacterData objetivo;

                // Lupus es invulnerable mientras Helena vive.
                if (helena.IsAlive)
                    objetivo = helena;
                else
                    objetivo = lupus;

                Debug.Log("--- TURNO DE " +
                    hero.characterName + " ---");

                Atacar(hero, objetivo);

                // Cambio a la segunda fase.
                if (!helena.IsAlive &&
                    !segundaFaseAnunciada)
                {
                    segundaFaseAnunciada = true;

                    Debug.Log("================================");
                    Debug.Log("HELENA HA SIDO DERROTADA");
                    Debug.Log("FASE 2: LUPUS ENTRA EN COMBATE");
                    Debug.Log("Lupus ya puede recibir dano.");
                    Debug.Log("================================");
                }

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            if (!lupus.IsAlive || !HayVivos(heroes))
                break;

            // Lupus siempre esta presente.
            // Mientras Helena vive, solo pide ayuda.
            // Cuando Helena muere, ataca directamente.
            bossAI.LupusTurn(lupus, helena, heroes);

            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoEntreTurnos));

            // Helena puede atacar en su propio turno.
            if (helena.IsAlive && HayVivos(heroes))
            {
                bossAI.HelenaTurn(helena, heroes);

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            ronda++;
        }

        FinalizarCombate();
    }

    void Atacar(CharacterData atacante, CharacterData objetivo)
    {
        if (objetivo == null || !objetivo.IsAlive)
            return;

        // Proteccion adicional de Lupus.
        if (objetivo == lupus && helena.IsAlive)
        {
            Debug.Log("Lupus es invulnerable mientras Helena vive.");
            return;
        }

        Debug.Log(atacante.characterName +
            " intenta atacar a " + objetivo.characterName);

        if (!DiceSystem.RollSuccess())
        {
            Debug.Log("El ataque falla.");
            return;
        }

        string ataque = atacante.GetRandomAttack();

        Debug.Log("Ataque: " + ataque);

        int dano = DiceSystem.RollAttack(ataque);

        objetivo.TakeDamage(dano);

        Debug.Log("Dano causado: " + dano);
        Debug.Log(objetivo.characterName +
            " HP: " + objetivo.currentHealth +
            "/" + objetivo.maxHealth);

        if (!objetivo.IsAlive)
        {
            Debug.Log(objetivo.characterName +
                " HA SIDO DERROTADO.");
        }
    }

    bool HayVivos(List<CharacterData> personajes)
    {
        return personajes.Exists(p => p.IsAlive);
    }

    void FinalizarCombate()
    {
        Debug.Log("================================");

        if (lupus.IsAlive)
        {
            Debug.Log("DERROTA DE LOS HEROES");
        }
        else
        {
            Debug.Log("VICTORIA DE LOS HEROES");

            int cofres = Random.Range(2, 6);

            Debug.Log("Lupus ha sido derrotado.");
            Debug.Log("Cofres obtenidos: " + cofres);

            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.AgregarRecompensa(
                    "Cofres", cofres);

                PartyManager.Instance.MostrarRecompensas();
            }
            else
            {
                Debug.LogWarning(
                    "No se encontro PartyManager. Los cofres no se guardaron.");
            }

        }

        Debug.Log("--- ESTADO FINAL ---");

        foreach (CharacterData hero in heroes)
        {
            Debug.Log(hero.characterName +
                ": " + hero.currentHealth + " HP");
        }

        Debug.Log("Helena: " + helena.currentHealth + " HP");
        Debug.Log("Lupus: " + lupus.currentHealth + " HP");

        Debug.Log("COMBATE FINALIZADO");
        Debug.Log("================================");
    }
}
