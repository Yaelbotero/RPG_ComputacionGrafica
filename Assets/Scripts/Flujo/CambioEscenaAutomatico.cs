using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Flujo
{
    /// <summary>
    /// Se coloca en el Canvas de transicion (CONTINUAR) de cada escena. Cuando el canvas se activa,
    /// carga la siguiente escena sola despues de unos segundos. El boton CONTINUAR sigue funcionando
    /// y la carga manual tiene prioridad (no se carga dos veces).
    /// </summary>
    public class CambioEscenaAutomatico : MonoBehaviour
    {
        [Tooltip("Nombre exacto de la escena siguiente (debe estar en Build Settings).")]
        public string escenaDestino;
        [Tooltip("Segundos que se muestra la pantalla de transicion antes de pasar sola.")]
        public float segundos = 3f;

        private static bool cargando;

        private void OnEnable()
        {
            cargando = false;
            StartCoroutine(EsperarYCargar());
        }

        private IEnumerator EsperarYCargar()
        {
            yield return new WaitForSecondsRealtime(segundos);
            Cargar();
        }

        public void Cargar()
        {
            if (cargando || string.IsNullOrEmpty(escenaDestino)) return;
            cargando = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaDestino);
        }
    }
}
