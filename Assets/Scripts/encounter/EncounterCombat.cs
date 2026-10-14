
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterCombat : MonoBehaviour
{
    [Header("Configuracion del combate")]
    public float tiempoEntreTurnos = 0.5f;

    [Header("Cantidad de enemigos")]
    [Range(1, 4)]
    public int minimoEnemigos = 1;

    [Range(1, 4)]
    public int maximoEnemigos = 4;

    public bool CombateActivo { get; private set; }

    public event Action<bool> CombateTerminado;

    private List<CharacterData> heroes;
    private List<CharacterData> enemigos;
    private List<int> tiposEnemigos;

    public void IniciarCombate(int primerEnemigo)
    {
        if (CombateActivo)
            return;

        // Obtener los heroes del PartyManager.
        if (PartyManager.Instance != null)
        {
            heroes = PartyManager.Instance.heroes;
        }
        else
        {
            heroes = CharacterDatabase.CreateHeroes();
        }

        if (!HayVivos(heroes))
        {
            Debug.Log("No quedan heroes vivos.");
            return;
        }

        enemigos = new List<CharacterData>();
        tiposEnemigos = new List<int>();

        int minimo = Mathf.Clamp(minimoEnemigos, 1, 4);
        int maximo = Mathf.Clamp(maximoEnemigos, minimo, 4);

        int cantidad = UnityEngine.Random.Range(
            minimo, maximo + 1);

        Debug.Log("================================");
        Debug.Log("ENCUENTRO ALEATORIO");
        Debug.Log("Cantidad de enemigos: " + cantidad);

        for (int i = 0; i < cantidad; i++)
        {
            int tipo = i == 0
                ? Mathf.Clamp(primerEnemigo, 0, 3)
                : UnityEngine.Random.Range(0, 4);

            CharacterData enemigo =
                CharacterDatabase.CreateForestEnemy(tipo);

            enemigos.Add(enemigo);
            tiposEnemigos.Add(tipo);

            Debug.Log("Enemigo " + (i + 1) + ": " +
                enemigo.characterName +
                " | HP: " + enemigo.currentHealth +
                " | Ataque: " + enemigo.attacks[0]);
        }

        CombateActivo = true;

        StartCoroutine(SistemaTurnos());
    }

    private IEnumerator SistemaTurnos()
    {
        int ronda = 1;

        while (HayVivos(heroes) && HayVivos(enemigos))
        {
            Debug.Log("========== RONDA " + ronda + " ==========");

            // Turnos de los heroes.
            foreach (CharacterData heroe in heroes)
            {
                if (!heroe.IsAlive)
                    continue;

                if (!HayVivos(enemigos))
                    break;

                CharacterData objetivo = ElegirVivo(enemigos);

                RealizarAtaque(heroe, objetivo);

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            if (!HayVivos(enemigos))
                break;

            // Turnos de los enemigos.
            foreach (CharacterData enemigo in enemigos)
            {
                if (!enemigo.IsAlive)
                    continue;

                if (!HayVivos(heroes))
                    break;

                CharacterData objetivo = ElegirVivo(heroes);

                RealizarAtaque(enemigo, objetivo);

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoEntreTurnos));
            }

            ronda++;
        }

        FinalizarCombate();
    }

    private void RealizarAtaque(
        CharacterData atacante,
        CharacterData objetivo)
    {
        if (objetivo == null ||
            !objetivo.IsAlive ||
            !atacante.IsAlive)
            return;

        Debug.Log("--- TURNO DE " +
            atacante.characterName + " ---");

        Debug.Log(atacante.characterName +
            " intenta atacar a " + objetivo.characterName);

        // Tirada de exito 1D10.
        if (!DiceSystem.RollSuccess())
        {
            Debug.Log("El ataque falla.");
            return;
        }

        string ataque = atacante.GetRandomAttack();

        Debug.Log("Ataque: " + ataque);

        // Suma de todos los dados del ataque.
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

    private bool HayVivos(List<CharacterData> grupo)
    {
        return grupo.Exists(personaje => personaje.IsAlive);
    }

    private CharacterData ElegirVivo(
        List<CharacterData> grupo)
    {
        List<CharacterData> vivos = grupo.FindAll(
            personaje => personaje.IsAlive);

        if (vivos.Count == 0)
            return null;

        int indice = UnityEngine.Random.Range(0, vivos.Count);

        return vivos[indice];
    }

    private void FinalizarCombate()
    {
        bool victoria = !HayVivos(enemigos);

        Debug.Log("================================");

        if (victoria)
        {
            Debug.Log("VICTORIA DE LOS HEROES");
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

        CombateActivo = false;

        CombateTerminado?.Invoke(victoria);
    }

    private void MostrarRecompensas()
    {
        Debug.Log("=== RECOMPENSAS DEL COMBATE ===");

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning(
                "No se encontro PartyManager.");
            return;
        }

        // Recompensa por cada enemigo derrotado.
        for (int i = 0; i < tiposEnemigos.Count; i++)
        {
            int tipo = tiposEnemigos[i];

            switch (tipo)
            {
                case 0:
                    PartyManager.Instance.AgregarRecompensa(
                        "Pociones", 3);

                    PartyManager.Instance.AgregarRecompensa(
                        "Dinero", 100);
                    break;

                case 1:
                    PartyManager.Instance.AgregarRecompensa(
                        "Cofres", 1);
                    break;

                case 2:
                    PartyManager.Instance.AgregarRecompensa(
                        "Pociones", 4);

                    PartyManager.Instance.AgregarRecompensa(
                        "Dinero", 250);
                    break;

                case 3:
                    PartyManager.Instance.AgregarRecompensa(
                        "Pociones", 1);

                    PartyManager.Instance.AgregarRecompensa(
                        "Dinero", 350);
                    break;
            }
        }

        PartyManager.Instance.MostrarRecompensas();
    }
}
