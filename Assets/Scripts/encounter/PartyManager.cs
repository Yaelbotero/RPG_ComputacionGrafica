
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    [Header("Personaje")]
    public CharacterData heroe;

    [Header("Inventario")]
    public int manzanas = 0;
    public int pociones = 0;

    void Awake()
    {
        // Evitar administradores duplicados.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Conservar el objeto entre escenas.
        DontDestroyOnLoad(gameObject);

        // Crear el Heroe 2 con sus estadisticas.
        heroe = CharacterDatabase.CreateHero();
    }

    public void AgregarRecompensa()
    {
        // Elegir aleatoriamente una manzana o pocion.
        int resultado = Random.Range(0, 2);

        if (resultado == 0)
        {
            manzanas++;
            Debug.Log("RECOMPENSA: +1 Manzana");
        }
        else
        {
            pociones++;
            Debug.Log("RECOMPENSA: +1 Pocion");
        }

        MostrarInventario();
    }

    public void MostrarEstado()
    {
        Debug.Log("=== ESTADO DEL HEROE ===");

        Debug.Log("Personaje: " + heroe.characterName);
        Debug.Log("Fuerza: " + heroe.strength);

        Debug.Log("Resistencia: " +
            heroe.currentHealth + "/" + heroe.maxHealth);
    }

    public void MostrarInventario()
    {
        Debug.Log("=== INVENTARIO ===");
        Debug.Log("Manzanas: " + manzanas);
        Debug.Log("Pociones: " + pociones);
    }

    public void ReiniciarPartida()
    {
        heroe = CharacterDatabase.CreateHero();

        manzanas = 0;
        pociones = 0;

        Debug.Log("Partida reiniciada.");
    }
}
