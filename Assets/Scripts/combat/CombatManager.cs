
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Configuracion de la simulacion")]
    public float tiempoEntreTurnos = 1f;

    [Range(1, 4)]
    public int enemigoInicial = 1;

    private List<CharacterData> heroes;
    private CharacterData enemigo;

    private bool combateActivo = false;

    void Start()
    {
        IniciarCombate();
    }

    public void IniciarCombate()
    {
        if (combateActivo)
            return;

        heroes = CharacterDatabase.CreateHeroes();

        enemigo = CharacterDatabase.CreateForestEnemy(
            enemigoInicial - 1);

        combateActivo = true;

        Debug.Log("================================");
        Debug.Log("INICIO DEL COMBATE");
        Debug.Log("================================");

        Debug.Log("Enemigo: " + enemigo.characterName);
        Debug.Log("Fuerza: " + enemigo.strength);
        Debug.Log("Vida: " + enemigo.currentHealth);

        StartCoroutine(SistemaTurnos());
    }

    IEnumerator SistemaTurnos()
    {
        int ronda = 1;

        while (enemigo.IsAlive && HayHeroesVivos())
        {
            Debug.Log("========== RONDA " + ronda + " ==========");

            // Turnos de los cuatro heroes.
            foreach (CharacterData heroe in heroes)
            {
                if (!heroe.IsAlive)
                    continue;

                if (!enemigo.IsAlive)
                    break;

                Debug.Log("TURNO DE " + heroe.characterName);

                RealizarAtaque(heroe, enemigo);

                yield return new WaitForSeconds(
                    tiempoEntreTurnos);
            }

            // Turno del enemigo.
            if (enemigo.IsAlive && HayHeroesVivos())
            {
                CharacterData objetivo = ElegirHeroeVivo();

                Debug.Log("TURNO DE " + enemigo.characterName);

                RealizarAtaque(enemigo, objetivo);

                yield return new WaitForSeconds(
                    tiempoEntreTurnos);
            }

            ronda++;
        }

        FinalizarCombate();
    }

    void RealizarAtaque(
        CharacterData atacante,
        CharacterData objetivo)
    {
        Debug.Log(atacante.characterName +
            " intenta atacar a " +
            objetivo.characterName);

        // Primera tirada: comprobar exito.
        bool exito = DiceSystem.RollSuccess();

        if (!exito)
        {
            Debug.Log("El ataque falla. No hay dano.");
            return;
        }

        // Elegir uno de los ataques del personaje.
        string ataque = atacante.GetRandomAttack();

        Debug.Log("Ataque: " + ataque);

        // Segunda tirada: calcular dano.
        int dano = DiceSystem.RollAttack(ataque);

        objetivo.TakeDamage(dano);

        Debug.Log(atacante.characterName +
            " causa " + dano + " de dano.");

        Debug.Log(objetivo.characterName +
            " tiene " + objetivo.currentHealth +
            "/" + objetivo.maxHealth + " HP.");

        if (!objetivo.IsAlive)
        {
            Debug.Log(objetivo.characterName +
                " HA SIDO DERROTADO.");
        }
    }

    bool HayHeroesVivos()
    {
        foreach (CharacterData heroe in heroes)
        {
            if (heroe.IsAlive)
                return true;
        }

        return false;
    }

    CharacterData ElegirHeroeVivo()
    {
        List<CharacterData> vivos = heroes.FindAll(
            heroe => heroe.IsAlive);

        if (vivos.Count == 0)
            return null;

        int indice = Random.Range(0, vivos.Count);

        return vivos[indice];
    }

    void FinalizarCombate()
    {
        Debug.Log("================================");

        if (!enemigo.IsAlive)
        {
            Debug.Log("VICTORIA DE LOS HEROES");
            Debug.Log("Enemigo derrotado: " +
                enemigo.characterName);

            MostrarRecompensas();
        }
        else
        {
            Debug.Log("DERROTA DE LOS HEROES");
        }

        Debug.Log("=== ESTADO FINAL DE LOS HEROES ===");

        foreach (CharacterData heroe in heroes)
        {
            Debug.Log(heroe.characterName +
                ": " + heroe.currentHealth +
                "/" + heroe.maxHealth + " HP");
        }

        Debug.Log("COMBATE FINALIZADO");
        Debug.Log("================================");

        combateActivo = false;
    }

    void MostrarRecompensas()
    {
        Debug.Log("=== RECOMPENSAS ===");

        switch (enemigoInicial)
        {
            case 1:
                Debug.Log("3 pociones y 100 de dinero");
                break;

            case 2:
                Debug.Log("1 cofre de tesoro");
                break;

            case 3:
                Debug.Log("4 pociones y 250 de dinero");
                break;

            case 4:
                Debug.Log("1 pocion y 350 de dinero");
                break;
        }
    }
}
