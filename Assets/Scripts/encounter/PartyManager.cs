using System.Collections.Generic;
using UnityEngine;
using RPG.Datos;
using RPG.Flujo;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    [Header("Personaje")]
    public CharacterData heroe;

    [Header("Inventario")]
    public int manzanas = 0;
    public int pociones = 0;

    [Header("Curacion de los objetos (puntos de vida)")]
    public int curacionPocion = 30;
    public int curacionManzana = 15;

    // Crea el PartyManager solo, para que el combate funcione desde cualquier escena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("PartyManager");
        go.AddComponent<PartyManager>();
    }

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
            EventosJuego.NotificarObjetoRecogido("Manzana", 1);
        }
        else
        {
            pociones++;
            Debug.Log("RECOMPENSA: +1 Pocion");
            EventosJuego.NotificarObjetoRecogido("Pocion", 1);
        }

        MostrarInventario();
        PublicarEstado();
    }

    // Probabilidades del botin al derrotar un enemigo.
    [Header("Botin de los enemigos")]
    [Range(0f, 1f)] public float probabilidadBotin = 0.7f;
    [Range(0f, 1f)] public float probabilidadPocion = 0.4f;

    // Un enemigo derrotado puede soltar una pocion o una manzana.
    // Devuelve el nombre del objeto soltado, o null si no solto nada.
    public string SoltarBotin()
    {
        if (Random.value > probabilidadBotin) return null;

        string nombre;
        if (Random.value < probabilidadPocion)
        {
            pociones++;
            nombre = "Poción";
        }
        else
        {
            manzanas++;
            nombre = "Manzana";
        }

        Debug.Log("BOTIN: +1 " + nombre);
        EventosJuego.NotificarObjetoRecogido(nombre, 1);
        PublicarEstado();
        return nombre;
    }

    // Usa un objeto del inventario. Devuelve los puntos de vida recuperados
    // (0 si no habia objeto o la vida ya estaba completa).
    public int UsarItem(string nombre)
    {
        if (heroe == null || !heroe.IsAlive) return 0;
        if (heroe.currentHealth >= heroe.maxHealth) return 0;

        string n = (nombre ?? "").ToLowerInvariant();
        int cura;

        if (n.StartsWith("man"))
        {
            if (manzanas <= 0) return 0;
            manzanas--;
            cura = curacionManzana;
        }
        else if (n.StartsWith("po"))
        {
            if (pociones <= 0) return 0;
            pociones--;
            cura = curacionPocion;
        }
        else return 0;

        int antes = heroe.currentHealth;
        heroe.Heal(cura);
        int curado = heroe.currentHealth - antes;

        Debug.Log("Curacion: +" + curado + " HP");
        PublicarEstado();
        return curado;
    }

    public int CuracionDe(string nombre)
    {
        string n = (nombre ?? "").ToLowerInvariant();
        if (n.StartsWith("man")) return curacionManzana;
        if (n.StartsWith("po")) return curacionPocion;
        return 0;
    }

    // Envia a la interfaz (HUD e inventario) la vida y los objetos actuales.
    public void PublicarEstado()
    {
        if (heroe != null)
            EventosJuego.NotificarVidaCambiada("Heroe", heroe.currentHealth, heroe.maxHealth);

        EventosJuego.NotificarInventarioCambiado(ListaItems());
    }

    public List<ItemVista> ListaItems()
    {
        List<ItemVista> lista = new List<ItemVista>();
        if (pociones > 0) lista.Add(new ItemVista { nombre = "Poción", cantidad = pociones });
        if (manzanas > 0) lista.Add(new ItemVista { nombre = "Manzana", cantidad = manzanas });
        return lista;
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
        PublicarEstado();
    }
}
