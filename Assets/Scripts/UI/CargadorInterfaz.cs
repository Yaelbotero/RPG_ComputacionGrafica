using UnityEngine;
using UnityEngine.SceneManagement;
using RPG.Flujo;

namespace RPG.UI
{
    /// <summary>
    /// Carga la escena "Interfaz" (HUD, dialogos, inventario, pausa, combate) encima de cada escena de juego.
    /// No se carga en el Menu principal ni si ya esta cargada. Quita la camara, el AudioListener y el EventSystem
    /// repetidos de la interfaz para que no choquen con los de la escena.
    /// Tambien reinicia la partida al volver al Menu.
    /// </summary>
    public static class CargadorInterfaz
    {
        public const string EscenaInterfaz = "Interfaz";
        public const string EscenaMenu = "Menu";

        private static bool cargando;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Iniciar()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            SceneManager.sceneLoaded += AlCargarEscena;
            Aplicar(SceneManager.GetActiveScene());
        }

        private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (modo != LoadSceneMode.Single) return;
            Aplicar(escena);
        }

        private static void Aplicar(Scene escena)
        {
            if (escena.name == EscenaMenu)
            {
                if (PartyManager.Instance != null) PartyManager.Instance.ReiniciarPartida();
                EncounterManager.ReiniciarEncuentros();
                return;
            }

            if (escena.name == EscenaInterfaz) return;
            if (cargando || SceneManager.GetSceneByName(EscenaInterfaz).isLoaded) return;
            if (!Application.CanStreamedLevelBeLoaded(EscenaInterfaz)) return;

            cargando = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(EscenaInterfaz, LoadSceneMode.Additive);
            if (op != null) op.completed += _ => { cargando = false; Limpiar(); };
            else cargando = false;
        }

        private static void Limpiar()
        {
            Scene interfaz = SceneManager.GetSceneByName(EscenaInterfaz);
            if (!interfaz.isLoaded) return;

            foreach (GameObject raiz in interfaz.GetRootGameObjects())
            {
                foreach (Camera c in raiz.GetComponentsInChildren<Camera>(true)) Object.Destroy(c);
                foreach (AudioListener a in raiz.GetComponentsInChildren<AudioListener>(true)) Object.Destroy(a);
                // Si la escena de juego ya trae un EventSystem, se elimina el de la interfaz.
                if (Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length > 1)
                {
                    foreach (UnityEngine.EventSystems.EventSystem e in raiz.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
                    {
                        Object.Destroy(e.gameObject);
                        break;
                    }
                }
            }

            if (PartyManager.Instance != null) PartyManager.Instance.PublicarEstado();
        }
    }
}
