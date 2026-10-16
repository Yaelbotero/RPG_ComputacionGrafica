using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Datos;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Puente entre la logica de combate (EncounterCombat / BossCombat) y la interfaz.
    /// El combate le pide cosas ("preparate", "que elija el jugador", "muestra esta tirada") y esta clase
    /// se encarga de abrir los menus, esperar la eleccion y reproducir la animacion de dados.
    /// Si la escena no tiene interfaz cargada, Activo es false y el combate usa el teclado (1, 2, 3).
    /// </summary>
    public static class CombateUI
    {
        public class Accion
        {
            public int tipo = -1;      // 0 = ataque, 1 = objeto
            public int indice = -1;    // ataque elegido
            public int objetivo = -1;  // enemigo elegido
        }

        public static MenuCombatePokemon Menu { get; private set; }
        public static bool Activo { get { return Menu != null; } }

        private static PanelDados panelDados;

        /// <summary>Espera a que la interfaz este cargada y la deja lista para el combate.</summary>
        public static IEnumerator Preparar(CharacterData heroe, string[] nombres, int[] hps, int[] maxs, string mensajeInicio)
        {
            Menu = null;

            float espera = 0f;
            while ((GestorInterfaz.Instancia == null || GestorInterfaz.Instancia.MenuActivo == null) && espera < 3f)
            {
                espera += Time.unscaledDeltaTime;
                yield return null;
            }

            GestorInterfaz gestor = GestorInterfaz.Instancia;
            if (gestor == null || gestor.MenuActivo == null)
            {
                Debug.LogWarning("CombateUI: no hay interfaz de combate en esta escena. Se usa el teclado.");
                yield break;
            }

            EventosJuego.NotificarCombateIniciado();
            gestor.MostrarCombate();
            yield return null; // el menu se activa y se suscribe a los eventos

            MenuCombatePokemon menu = gestor.MenuActivo;

            List<DatosAtaque> ataques = new List<DatosAtaque>();
            for (int i = 0; i < heroe.attacks.Count; i++)
                ataques.Add(new DatosAtaque { nombre = "Ataque " + (i + 1), dados = heroe.attacks[i] });

            List<ItemVista> items = PartyManager.Instance != null ? PartyManager.Instance.ListaItems() : new List<ItemVista>();

            menu.nombreHeroe = "Héroe";
            menu.PrepararCombate(nombres, hps, maxs, heroe.currentHealth, heroe.maxHealth, ataques, items);

            if (panelDados == null)
                panelDados = PanelDados.Crear(menu.txtMensaje != null ? menu.txtMensaje.font : null);

            Menu = menu;
            if (!string.IsNullOrEmpty(mensajeInicio)) Menu.MostrarMensaje(mensajeInicio);
            yield return Menu.EsperarMensajes();
        }

        /// <summary>
        /// Coloca a los personajes como en Pokemon: el heroe abajo a la izquierda y los enemigos arriba a la derecha.
        /// Las posiciones estan en unidades del mundo (la camara de combate es fija: 17.8 x 10 unidades).
        /// </summary>
public static void ColocarEnSuelo(Transform heroe, Transform[] enemigos, float[] escalas, float pisoY)
        {
            CombateEscena.ColocarEnSuelo(heroe, enemigos, escalas, pisoY);
        }

        public static void ColocarPokemon(Transform heroe, Transform[] enemigos, float[] escalas)
        {
            CombateEscena.Colocar(heroe, enemigos, escalas);
        }

                        /// <summary>Dibuja un ovalo bajo los pies del personaje, como las plataformas de Pokemon.</summary>


        public static void Mensaje(string texto)
        {
            if (Menu != null) Menu.MostrarMensaje(texto);
        }

        public static IEnumerator EsperarMensajes()
        {
            if (Menu != null) yield return Menu.EsperarMensajes();
        }

        /// <summary>Actualiza las barras de vida del heroe y de los enemigos.</summary>
        public static void ActualizarVidas(CharacterData heroe, List<CharacterData> enemigos)
        {
            if (Menu == null) return;
            Menu.SetVidaHeroe(heroe.currentHealth, heroe.maxHealth);
            if (enemigos != null)
                for (int i = 0; i < enemigos.Count; i++) Menu.SetVidaEnemigo(i, enemigos[i].currentHealth);
            if (PartyManager.Instance != null) PartyManager.Instance.PublicarEstado();
        }

        /// <summary>
        /// Turno del jugador: menu ATACAR / ITEMS. Si usa un objeto lo aplica (curacion) y devuelve tipo 1.
        /// Si ataca, devuelve el ataque y el objetivo (pregunta a quien si hay mas de un enemigo vivo).
        /// </summary>
        public static IEnumerator ElegirAccion(Accion resultado, CharacterData heroe, string[] nombres, int[] hps, int[] maxs, bool[] vivos, bool preguntarObjetivo)
        {
            resultado.tipo = -1;

            while (true)
            {
                if (PartyManager.Instance != null) Menu.ConfigurarItems(PartyManager.Instance.ListaItems());

                yield return Menu.EsperarMensajes();
                Menu.AbrirMenuPrincipal();

                int tipo = -1, indice = -1;
                Action<int> alAtaque = i => { tipo = 0; indice = i; };
                Action<int> alItem = i => { tipo = 1; indice = i; };
                Menu.AtaqueElegido += alAtaque;
                Menu.ItemElegido += alItem;
                while (tipo < 0) yield return null;
                Menu.AtaqueElegido -= alAtaque;
                Menu.ItemElegido -= alItem;

                if (tipo == 1)
                {
                    string nombre = Menu.items[indice].nombre;
                    if (heroe.currentHealth >= heroe.maxHealth)
                    {
                        Menu.MostrarMensaje("Tu vida ya está completa.");
                        continue;
                    }

                    int curado = PartyManager.Instance.UsarItem(nombre);
                    Menu.SetVidaHeroe(heroe.currentHealth, heroe.maxHealth);
                    Menu.MostrarMensaje("Héroe usa " + nombre + " y recupera " + curado + " HP.");
                    Menu.ConfigurarItems(PartyManager.Instance.ListaItems());
                    resultado.tipo = 1;
                    yield return Menu.EsperarMensajes();
                    yield break;
                }

                // Ataque: elegir objetivo.
                int vivosCuenta = 0, unico = -1;
                for (int i = 0; i < vivos.Length; i++)
                    if (vivos[i]) { vivosCuenta++; unico = i; }

                int objetivo = unico;
                if (preguntarObjetivo && vivosCuenta > 1)
                {
                    objetivo = -2;
                    Menu.PedirObjetivo(nombres, hps, maxs, vivos, i => objetivo = i);
                    while (objetivo == -2) yield return null;
                    if (objetivo == -1) continue; // cancelo: vuelve al menu principal
                }

                resultado.tipo = 0;
                resultado.indice = indice;
                resultado.objetivo = objetivo;
                yield break;
            }
        }

        /// <summary>Reproduce la animacion de dados de una tirada.</summary>
        public static IEnumerator Tirada(string atacante, string nombreAtaque, ResultadoDado r, float velocidad)
        {
            if (panelDados == null) yield break;
            panelDados.velocidad = velocidad;
            yield return panelDados.Mostrar(atacante + " usa " + nombreAtaque + "  (" + r.formula + ")", r);
        }

        public static void Terminar(bool victoria)
        {
            EventosJuego.NotificarCombateTerminado(victoria);
            if (Menu != null) Menu.OcultarTodo();
            Menu = null;
        }
    }
}
