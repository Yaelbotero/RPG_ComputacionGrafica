using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using RPG.Datos;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>Un bloque de interfaz perteneciente a una parte del juego (escena 1, 2, 3 o Boss/Final).</summary>
    [Serializable]
    public class GrupoInterfaz
    {
        public string etiqueta;
        public ParteJuego parte;
        [Tooltip("Nombres de escena de Unity que activan este grupo.")] public string[] escenasUnity;
        public GameObject raiz;
        [Tooltip("Interfaz del escenario normal (HUD de vida y oro).")] public GameObject escenarioNormal;
        [Tooltip("Interfaz de cuando se habla con un personaje.")] public GameObject conversaciones;
        [Tooltip("Interfaz de combate tipo Pokemon (opcional).")] public GameObject combate;
        public ReproductorConversacion reproductor;
        public MenuCombatePokemon menuCombate;
    }

    /// <summary>
    /// Decide que interfaz se ve segun la escena de Unity que esta activa.
    /// MENU y OBJETOS (inventario, notificaciones) no pertenecen a ningun grupo: estan siempre disponibles.
    ///
    /// En la propia escena "Interfaz" (vista previa): F1..F4 cambian de grupo, N = escenario normal,
    /// C = siguiente conversacion, K = combate, I = inventario, Esc = pausa.
    /// </summary>
    public class GestorInterfaz : MonoBehaviour
    {
        public static GestorInterfaz Instancia { get; private set; }

        public string escenaVistaPrevia = "Interfaz";
        public List<GrupoInterfaz> grupos = new List<GrupoInterfaz>();
        public GameObject ayudaVistaPrevia;

        private GrupoInterfaz grupoActual;

        /// <summary>Menu de combate del grupo que esta activo en esta escena (null si la escena no tiene combate).</summary>
        public MenuCombatePokemon MenuActivo { get { return grupoActual != null ? grupoActual.menuCombate : null; } }
        private bool vistaPrevia;

        private void Awake()
        {
            Instancia = this;
            vistaPrevia = SceneManager.GetActiveScene().name == escenaVistaPrevia;
            if (ayudaVistaPrevia != null) ayudaVistaPrevia.SetActive(vistaPrevia);
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += AlCambiarEscena;
            EventosJuego.OnCombateIniciado += MostrarCombate;
            EventosJuego.OnCombateTerminado += AlTerminarCombate;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= AlCambiarEscena;
            EventosJuego.OnCombateIniciado -= MostrarCombate;
            EventosJuego.OnCombateTerminado -= AlTerminarCombate;
        }

        private void Start()
        {
            AplicarEscena(SceneManager.GetActiveScene().name);
            if (vistaPrevia) CargarDatosDeEjemplo();
        }

        private void AlCambiarEscena(Scene anterior, Scene nueva) { AplicarEscena(nueva.name); }
        private void AlTerminarCombate(bool victoria) { MostrarEscenarioNormal(); }

        private void AplicarEscena(string nombreEscena)
        {
            GrupoInterfaz elegido = null;
            foreach (GrupoInterfaz g in grupos)
                if (g.escenasUnity != null && Array.IndexOf(g.escenasUnity, nombreEscena) >= 0) { elegido = g; break; }

            if (elegido == null && vistaPrevia && grupos.Count > 0) elegido = grupos[0];
            Activar(elegido);
        }

        private void Activar(GrupoInterfaz elegido)
        {
            grupoActual = elegido;
            foreach (GrupoInterfaz g in grupos)
                if (g.raiz != null) g.raiz.SetActive(g == elegido);
            MostrarEscenarioNormal();
        }

        public void MostrarEscenarioNormal()
        {
            if (grupoActual == null) return;
            if (grupoActual.escenarioNormal != null) grupoActual.escenarioNormal.SetActive(true);
            if (grupoActual.combate != null) grupoActual.combate.SetActive(false);
        }

        public void MostrarCombate()
        {
            if (grupoActual == null || grupoActual.combate == null) return;
            if (grupoActual.escenarioNormal != null) grupoActual.escenarioNormal.SetActive(false);
            grupoActual.combate.SetActive(true);
        }

        public bool ReproducirConversacion(int numero)
        {
            foreach (GrupoInterfaz g in grupos)
                if (g.reproductor != null && g.raiz != null && g.raiz.activeInHierarchy && g.reproductor.Reproducir(numero)) return true;
            return false;
        }

        private void Update()
        {
            if (!vistaPrevia || ReproductorConversacion.Activa || MenuPausa.Pausado) return;

            if (EntradaUI.Tecla(KeyCode.F1)) CambiarGrupo(0);
            else if (EntradaUI.Tecla(KeyCode.F2)) CambiarGrupo(1);
            else if (EntradaUI.Tecla(KeyCode.F3)) CambiarGrupo(2);
            else if (EntradaUI.Tecla(KeyCode.F4)) CambiarGrupo(3);
            else if (EntradaUI.Tecla(KeyCode.N)) MostrarEscenarioNormal();
            else if (EntradaUI.Tecla(KeyCode.K)) MostrarCombate();
            else if (EntradaUI.Tecla(KeyCode.C) && grupoActual != null && grupoActual.reproductor != null) grupoActual.reproductor.ReproducirSiguiente();
        }

        private void CambiarGrupo(int indice)
        {
            if (indice < grupos.Count) Activar(grupos[indice]);
        }

        private void CargarDatosDeEjemplo()
        {
            EventosJuego.NotificarVidaCambiada("Heroe", 87, 87);
            EventosJuego.NotificarOroCambiado(0);
            List<ItemVista> demo = new List<ItemVista>
            {
                new ItemVista { nombre = "Poción", cantidad = 3 },
                new ItemVista { nombre = "Cofre de tesoro", cantidad = 1 }
            };
            EventosJuego.NotificarInventarioCambiado(demo);
        }
    }
}
