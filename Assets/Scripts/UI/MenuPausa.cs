using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG.UI
{
    /// <summary>
    /// Menu de pausa disponible en cualquier momento de la partida (tecla Esc).
    /// Reutiliza el mezclador de audio del menu principal (MusicVolume / SFXVolume) y vuelve a la escena "Menu".
    /// No se abre durante una conversacion ni mientras el combate tiene un submenu abierto.
    /// </summary>
    public class MenuPausa : MonoBehaviour
    {
        public static bool Pausado { get; private set; }

        [Header("Vista")]
        public GameObject panel;
        public GameObject panelAjustes;
        public Button botonContinuar;
        public Button botonAjustes;
        public Button botonMenuPrincipal;
        public Button botonSalir;
        public Button botonVolverAjustes;
        public Slider sliderMusica;
        public Slider sliderEfectos;

        [Header("Audio y escenas")]
        public AudioMixer mixer;
        public string escenaMenu = "Menu";

        private void Awake()
        {
            Pausado = false;
            if (panel != null) panel.SetActive(false);
            if (panelAjustes != null) panelAjustes.SetActive(false);

            if (botonContinuar != null) botonContinuar.onClick.AddListener(Continuar);
            if (botonAjustes != null) botonAjustes.onClick.AddListener(AbrirAjustes);
            if (botonVolverAjustes != null) botonVolverAjustes.onClick.AddListener(CerrarAjustes);
            if (botonMenuPrincipal != null) botonMenuPrincipal.onClick.AddListener(VolverAlMenu);
            if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
            if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(v => CambiarVolumen("MusicVolume", v));
            if (sliderEfectos != null) sliderEfectos.onValueChanged.AddListener(v => CambiarVolumen("SFXVolume", v));
        }

        private void Start()
        {
            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 0f));
            if (sliderEfectos != null) sliderEfectos.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 0f));
        }

        private void OnDisable()
        {
            if (Pausado) { Pausado = false; Time.timeScale = 1f; }
        }

        private void LateUpdate()
        {
            if (!EntradaUI.Tecla(KeyCode.Escape)) return;
            if (EntradaUI.CancelarConsumidoEsteFrame) return;
            if (ReproductorConversacion.Activa) return;
            if (SceneManager.GetActiveScene().name == escenaMenu) return;

            if (Pausado)
            {
                if (panelAjustes != null && panelAjustes.activeSelf) CerrarAjustes();
                else Continuar();
            }
            else Pausar();
        }

        public void Pausar()
        {
            Pausado = true;
            Time.timeScale = 0f;
            if (panel != null) panel.SetActive(true);
            Seleccionar(botonContinuar);
        }

        public void Continuar()
        {
            Pausado = false;
            Time.timeScale = 1f;
            if (panelAjustes != null) panelAjustes.SetActive(false);
            if (panel != null) panel.SetActive(false);
        }

        public void AbrirAjustes()
        {
            if (panelAjustes != null) panelAjustes.SetActive(true);
            Seleccionar(botonVolverAjustes);
        }

        public void CerrarAjustes()
        {
            if (panelAjustes != null) panelAjustes.SetActive(false);
            Seleccionar(botonContinuar);
        }

        public void VolverAlMenu()
        {
            Continuar();
            SceneManager.LoadScene(escenaMenu);
        }

        public void Salir()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }

        private void CambiarVolumen(string parametro, float valor)
        {
            if (mixer != null) mixer.SetFloat(parametro, valor);
            PlayerPrefs.SetFloat(parametro, valor);
        }

        private static void Seleccionar(Button b)
        {
            if (b != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(b.gameObject);
        }
    }
}
