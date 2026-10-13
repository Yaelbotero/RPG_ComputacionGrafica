using UnityEngine;

namespace RPG.Flujo
{
    /// <summary>
    /// Gestor global del estado del juego. Sobrevive a la carga de escenas.
    /// Almacena banderas narrativas, posición en el mapa y progreso.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instancia { get; private set; }

        [Header("Estado Narrativo")]
        public bool introTabernaVista = false;
        public bool viajeAceptado = false;
        public bool bossVencido = false;
        public bool heroeMurio = false;
        // POR QUE HashSet: Permite buscar de forma rapida (O(1)) si un dialogo ya fue visto, evitando duplicados.
        // POR QUE [System.NonSerialized]: Unity no serializa HashSet; elimina el warning UAC1009 manteniendo el campo público para acceso en runtime.
        [System.NonSerialized]
        public System.Collections.Generic.HashSet<string> dialogosVistos = new System.Collections.Generic.HashSet<string>();


        [Header("Posición del Jugador en Exploración")]
        public Vector2 posicionHeroeBosque;
        public bool retornarDeCombate = false;

        [Header("Estadísticas / Bonificaciones")]
        public int monstruosDerrotados = 0;
        public int oroRecolectado = 0;

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            // POR QUÉ: Para conservar las banderas narrativas y la posición del héroe al cambiar entre el bosque y las escenas de combate/boss.
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventosJuego.OnOroCambiado += SincronizarOro;
            EventosJuego.OnCombateTerminado += RegistrarFinCombate;
        }

        private void OnDisable()
        {
            EventosJuego.OnOroCambiado -= SincronizarOro;
            EventosJuego.OnCombateTerminado -= RegistrarFinCombate;
        }

        private void SincronizarOro(int totalOro)
        {
            oroRecolectado = totalOro;
        }

        private void RegistrarFinCombate(bool victoria)
        {
            if (victoria)
            {
                monstruosDerrotados++;
            }
            else
            {
                heroeMurio = true;
            }
        }

        /// <summary>
        /// Llamado por el equipo de combate al derrotar a Lupus/Helena.
        /// </summary>
        public void MarcarBossVencido()
        {
            bossVencido = true;
        }

        /// <summary>
        /// Registra la posición actual antes de cargar una nueva escena (ej. Combate).
        /// </summary>
        public void GuardarPosicionBosque(Vector2 pos)
        {
            posicionHeroeBosque = pos;
            retornarDeCombate = true;
        }

        /// <summary>
        /// Limpia el estado de retorno una vez aplicado.
        /// </summary>
        public void LimpiarRetornoDeCombate()
        {
            retornarDeCombate = false;
        }

        public void MarcarDialogoVisto(string id)
        {
            if (!string.IsNullOrEmpty(id)) dialogosVistos.Add(id);
        }

        public bool DialogoVisto(string id)
        {
            return dialogosVistos.Contains(id);
        }

        // QUE HACE: Limpia las banderas para un nuevo juego.
        // POR QUE: SecuenciaFin necesita resetear el progreso sin destruir el Singleton GameManager al volver al Menu.
        public void ReiniciarPartida()
        {
            introTabernaVista = false;
            viajeAceptado = false;
            bossVencido = false;
            heroeMurio = false;
            dialogosVistos.Clear();
            monstruosDerrotados = 0;
            oroRecolectado = 0;
            retornarDeCombate = false;
        }
    }
}