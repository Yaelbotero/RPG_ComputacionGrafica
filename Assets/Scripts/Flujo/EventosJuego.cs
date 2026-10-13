using System;

namespace RPG.Flujo
{
    /// <summary>
    /// Contrato central de eventos del juego. Permite desacoplar los sistemas (UI, Audio, Combate, Flujo).
    /// POR QUÉ estáticos: Para que cualquier clase pueda emitir o escuchar eventos sin referencias directas (singleton o bus de eventos), facilitando la arquitectura.
    /// IMPORTANTE: Todo suscriptor DEBE desuscribirse en OnDisable() u OnDestroy() para evitar llamadas a referencias nulas (memory leaks), ya que los eventos estáticos sobreviven el ciclo de vida de los GameObjects.
    /// NOMBRES DE ENTIDAD VÁLIDOS (case-insensitive): 'Heroe' o 'Jugador', 'Lupus', 'Helena'. Cualquier otro nombre = enemigo.
    /// </summary>
    public static class EventosJuego
    {
        // ==========================================
        // EVENTOS DE COMBATE Y ESTADÍSTICAS
        // ==========================================

        /// <summary>Se dispara cuando la vida de una entidad cambia. Parámetros: nombre entidad, vida actual, vida máxima.</summary>
        public static event Action<string, int, int> OnVidaCambiada;
        public static void NotificarVidaCambiada(string quien, int actual, int max) => OnVidaCambiada?.Invoke(quien, actual, max);

        /// <summary>Se dispara al aplicar daño. Parámetros: objetivo que recibe daño, cantidad de daño.</summary>
        public static event Action<string, int> OnDanoRecibido;
        public static void NotificarDanoRecibido(string objetivo, int cantidad) => OnDanoRecibido?.Invoke(objetivo, cantidad);

        /// <summary>Se dispara cuando una entidad pifia un ataque. Parámetro: nombre de la entidad.</summary>
        public static event Action<string> OnPifia;
        public static void NotificarPifia(string quien) => OnPifia?.Invoke(quien);

        /// <summary>Se dispara al iniciar una secuencia de combate.</summary>
        public static event Action OnCombateIniciado;
        public static void NotificarCombateIniciado() => OnCombateIniciado?.Invoke();

        /// <summary>Se dispara al terminar un combate. Parámetro: true si es victoria, false si el héroe murió.</summary>
        public static event Action<bool> OnCombateTerminado;
        public static void NotificarCombateTerminado(bool victoria) => OnCombateTerminado?.Invoke(victoria);

        // ==========================================
        // EVENTOS DE INVENTARIO Y RECOLECCIÓN
        // ==========================================

        /// <summary>Se dispara al recoger un objeto del suelo o recompensa. Parámetros: nombre del objeto, cantidad.</summary>
        public static event Action<string, int> OnObjetoRecogido;
        public static void NotificarObjetoRecogido(string nombre, int cantidad) => OnObjetoRecogido?.Invoke(nombre, cantidad);

        /// <summary>Se dispara cuando el estado del inventario se actualiza. Parámetro: lista de items para la UI.</summary>
        public static event Action<System.Collections.Generic.List<RPG.Datos.ItemVista>> OnInventarioCambiado;
        public static void NotificarInventarioCambiado(System.Collections.Generic.List<RPG.Datos.ItemVista> items) => OnInventarioCambiado?.Invoke(items);

        /// <summary>Se dispara cuando la cantidad de oro cambia. Parámetro: nuevo total de oro.</summary>
        public static event Action<int> OnOroCambiado;
        public static void NotificarOroCambiado(int oroActual) => OnOroCambiado?.Invoke(oroActual);

        // ==========================================
        // EVENTOS DE FLUJO Y NARRATIVA
        // ==========================================

        /// <summary>Se dispara al iniciar un diálogo con un PNJ. Parámetro: nombre del PNJ.</summary>
        public static event Action<string> OnDialogoIniciado;
        public static void NotificarDialogoIniciado(string pnj) => OnDialogoIniciado?.Invoke(pnj);

        /// <summary>Se dispara al terminar un diálogo.</summary>
        public static event Action OnDialogoTerminado;
        public static void NotificarDialogoTerminado() => OnDialogoTerminado?.Invoke();

        /// <summary>Se dispara cuando una nueva escena termina de cargar. Parámetro: nombre de la escena.</summary>
        public static event Action<string> OnEscenaCargada;
        public static void NotificarEscenaCargada(string nombreEscena) => OnEscenaCargada?.Invoke(nombreEscena);

        /// <summary>Se dispara cuando cambia el turno en combate. Parámetro: nombre de quien tiene el turno.</summary>
        public static event Action<string> OnTurnoCambiado;
        public static void NotificarTurnoCambiado(string quien) => OnTurnoCambiado?.Invoke(quien);
    }
}
