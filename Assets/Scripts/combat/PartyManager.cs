
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    public List<CharacterData> heroes;

    // Recompensas acumuladas del grupo.
    public Dictionary<string, int> recompensas =
        new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        heroes = CharacterDatabase.CreateHeroes();

        recompensas.Add("Pociones", 0);
        recompensas.Add("Dinero", 0);
        recompensas.Add("Cofres", 0);
    }

    public bool HayHeroesVivos()
    {
        return heroes.Exists(heroe => heroe.IsAlive);
    }

    public void MostrarEstado()
    {
        Debug.Log("=== ESTADO DEL GRUPO ===");

        foreach (CharacterData heroe in heroes)
        {
            Debug.Log(heroe.characterName +
                ": " + heroe.currentHealth +
                "/" + heroe.maxHealth + " HP");
        }
    }

    public void AgregarRecompensa(string tipo, int cantidad)
    {
        if (cantidad <= 0)
            return;

        if (!recompensas.ContainsKey(tipo))
        {
            recompensas.Add(tipo, 0);
        }

        recompensas[tipo] += cantidad;

        Debug.Log("Recompensa: +" + cantidad +
            " " + tipo);
    }

    public void MostrarRecompensas()
    {
        Debug.Log("=== RECOMPENSAS ACUMULADAS ===");

        foreach (KeyValuePair<string, int> item in recompensas)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }

    public void ReiniciarGrupo()
    {
        heroes = CharacterDatabase.CreateHeroes();

        recompensas.Clear();
        recompensas.Add("Pociones", 0);
        recompensas.Add("Dinero", 0);
        recompensas.Add("Cofres", 0);

        Debug.Log("Grupo y recompensas reiniciados.");
    }
}
