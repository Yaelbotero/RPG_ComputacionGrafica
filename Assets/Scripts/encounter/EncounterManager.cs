
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    [Header("Escena de combate")]
    public string escenaCombate = "Combate";

    [Header("NPC acompañante")]
    public Transform nun;

    public static int cantidadEnemigos = 1;
    public static int enemigoActual = -1;

    private static HashSet<int> enemigosDerrotados =
        new HashSet<int>();

    // Posiciones guardadas antes del combate.
    private static Vector3 posicionJugador;
    private static Vector3 posicionNun;

    private static bool tienePosicionJugador = false;
    private static bool tienePosicionNun = false;

    private bool cambiandoEscena = false;

    // Reiniciar datos al comenzar una partida nueva.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarDatos()
    {
        enemigosDerrotados.Clear();

        enemigoActual = -1;
        cantidadEnemigos = 1;

        tienePosicionJugador = false;
        tienePosicionNun = false;

        posicionJugador = Vector3.zero;
        posicionNun = Vector3.zero;
    }

    void Start()
    {
        // Restaurar al jugador.
        if (tienePosicionJugador)
        {
            GameObject jugador =
                GameObject.FindGameObjectWithTag("Player");

            if (jugador != null)
            {
                RestaurarPosicion(
                    jugador.transform,
                    posicionJugador);

                Debug.Log("Posicion del heroe restaurada.");
            }

            tienePosicionJugador = false;
        }

        // Restaurar a Nun.
        if (tienePosicionNun)
        {
            if (nun != null)
            {
                RestaurarPosicion(nun, posicionNun);

                Debug.Log("Posicion de Nun restaurada.");
            }
            else
            {
                Debug.LogWarning(
                    "Asigna Nun en EncounterManager.");
            }

            tienePosicionNun = false;
        }
    }

    private void RestaurarPosicion(
        Transform personaje,
        Vector3 posicion)
    {
        // Actualizar el Transform.
        personaje.position = posicion;

        // Actualizar tambien el Rigidbody2D, si existe.
        Rigidbody2D rb =
            personaje.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = new Vector2(
                posicion.x,
                posicion.y);

            rb.linearVelocity = Vector2.zero;
        }
    }

    public void IniciarEncuentro(
        int idEnemigo,
        Vector3 posicionActualJugador)
    {
        if (cambiandoEscena)
            return;

        if (enemigosDerrotados.Contains(idEnemigo))
            return;

        cambiandoEscena = true;

        enemigoActual = idEnemigo;

        // Guardar posicion del heroe.
        posicionJugador = posicionActualJugador;
        tienePosicionJugador = true;

        // Guardar posicion de Nun.
        if (nun != null)
        {
            posicionNun = nun.position;
            tienePosicionNun = true;
        }
        else
        {
            tienePosicionNun = false;

            Debug.LogWarning(
                "Nun no esta asignada en EncounterManager.");
        }

        // Generar encuentro de 1 o 2 enemigos.
        cantidadEnemigos = Random.Range(1, 3);

        Debug.Log("ENCUENTRO INICIADO");
        Debug.Log("Cantidad de enemigos: " +
            cantidadEnemigos);

        SceneManager.LoadScene(escenaCombate);
    }

    public static void RegistrarVictoria()
    {
        if (enemigoActual >= 0)
        {
            enemigosDerrotados.Add(enemigoActual);
        }

        enemigoActual = -1;
    }

    public static bool EstaDerrotado(int idEnemigo)
    {
        return enemigosDerrotados.Contains(idEnemigo);
    }

    public static void ReiniciarEncuentros()
    {
        ReiniciarDatos();
    }
}
