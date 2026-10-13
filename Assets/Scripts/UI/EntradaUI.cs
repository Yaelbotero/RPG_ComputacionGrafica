using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RPG.UI
{
    /// <summary>
    /// Entrada de teclado unificada para toda la interfaz (dialogos, combate, pausa, inventario).
    /// POR QUE: el proyecto usa "Active Input Handling = Both" (hay scripts con Input clasico y otros con Input System);
    /// centralizar aqui evita repetir #if en cada script y permite cambiar las teclas en un solo lugar.
    /// </summary>
    public static class EntradaUI
    {
        private static int frameCancelarConsumido = -1;

        /// <summary>Quien use Escape/X para cerrar algo lo marca, asi la pausa no se abre en el mismo frame.</summary>
        public static void ConsumirCancelar() { frameCancelarConsumido = Time.frameCount; }
        public static bool CancelarConsumidoEsteFrame { get { return frameCancelarConsumido == Time.frameCount; } }

        public static bool Confirmar() { return Tecla(KeyCode.Space) || Tecla(KeyCode.Return) || Tecla(KeyCode.E) || Tecla(KeyCode.Z); }
        public static bool Cancelar() { return Tecla(KeyCode.Escape) || Tecla(KeyCode.X) || Tecla(KeyCode.Backspace); }
        public static bool Retroceder() { return Tecla(KeyCode.Q); }
        public static bool Inventario() { return Tecla(KeyCode.I) || Tecla(KeyCode.Tab); }

        public static bool Tecla(KeyCode k)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(k);
#elif ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb == null) return false;
            switch (k)
            {
                case KeyCode.Space: return kb.spaceKey.wasPressedThisFrame;
                case KeyCode.Return: return kb.enterKey.wasPressedThisFrame;
                case KeyCode.E: return kb.eKey.wasPressedThisFrame;
                case KeyCode.Z: return kb.zKey.wasPressedThisFrame;
                case KeyCode.X: return kb.xKey.wasPressedThisFrame;
                case KeyCode.C: return kb.cKey.wasPressedThisFrame;
                case KeyCode.K: return kb.kKey.wasPressedThisFrame;
                case KeyCode.N: return kb.nKey.wasPressedThisFrame;
                case KeyCode.Q: return kb.qKey.wasPressedThisFrame;
                case KeyCode.I: return kb.iKey.wasPressedThisFrame;
                case KeyCode.Tab: return kb.tabKey.wasPressedThisFrame;
                case KeyCode.Escape: return kb.escapeKey.wasPressedThisFrame;
                case KeyCode.Backspace: return kb.backspaceKey.wasPressedThisFrame;
                case KeyCode.Alpha1: return kb.digit1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return kb.digit2Key.wasPressedThisFrame;
                case KeyCode.F1: return kb.f1Key.wasPressedThisFrame;
                case KeyCode.F2: return kb.f2Key.wasPressedThisFrame;
                case KeyCode.F3: return kb.f3Key.wasPressedThisFrame;
                case KeyCode.F4: return kb.f4Key.wasPressedThisFrame;
            }
            return false;
#else
            return false;
#endif
        }
    }
}
