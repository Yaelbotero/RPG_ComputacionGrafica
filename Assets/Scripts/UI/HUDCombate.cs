using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPG.Flujo;
using System.Collections.Generic;

namespace RPG.UI
{
    /// <summary>
    /// Gestiona la visualización de la interfaz de combate (HP de combatientes, botones de ataque y registro).
    /// POR QUÉ: Se engancha a EventosJuego para pintar la información sin manejar la lógica de la pelea.
    /// Los botones exponen UnityEvents para que el equipo de Combate arrastre su lógica en el Inspector.
    /// </summary>
    public class HUDCombate : MonoBehaviour
    {
        [Header("Tipografía")]
        public TMP_FontAsset fuenteTitulos;
        public TMP_FontAsset fuenteCuerpo;

        [Header("Registro de Combate")]
        public int lineasMaximasLog = 5;
        private Queue<string> registroCombate = new Queue<string>();

        [Header("Eventos de Combate (Para el equipo de lógica)")]
        public UnityEngine.Events.UnityEvent AlAtaque1;
        public UnityEngine.Events.UnityEvent AlAtaque2;
        public UnityEngine.Events.UnityEvent AlAtaque3;

        private TextMeshProUGUI txtHeroeHP;
        private TextMeshProUGUI txtLupusHP;
        private TextMeshProUGUI txtHelenaHP;
        private TextMeshProUGUI txtEnemigoBasicoHP;
        private TextMeshProUGUI txtRegistro;
        private TextMeshProUGUI txtTurno;

        private void Awake()
        {
            // POR QUÉ usar transform.Find: Permite encontrar automáticamente a los hijos directos sin depender de que alguien olvide arrastrarlos en el Inspector.
            txtHeroeHP = transform.Find("TextoHeroe")?.GetComponent<TextMeshProUGUI>();
            txtLupusHP = transform.Find("TextoLupus")?.GetComponent<TextMeshProUGUI>();
            txtHelenaHP = transform.Find("TextoHelena")?.GetComponent<TextMeshProUGUI>();
            txtEnemigoBasicoHP = transform.Find("TextoEnemigo")?.GetComponent<TextMeshProUGUI>();
            txtRegistro = transform.Find("TextoRegistro")?.GetComponent<TextMeshProUGUI>();
            txtTurno = transform.Find("TextoTurno")?.GetComponent<TextMeshProUGUI>();

            Transform panelBotones = transform.Find("PanelBotones");
            if (panelBotones != null)
            {
                Button btn1 = panelBotones.Find("BtnAtaque1")?.GetComponent<Button>();
                Button btn2 = panelBotones.Find("BtnAtaque2")?.GetComponent<Button>();
                Button btn3 = panelBotones.Find("BtnAtaque3")?.GetComponent<Button>();

                // POR QUÉ lambdas: Como los botones viven en este mismo objeto, su ciclo de vida coincide, por lo que las lambdas son seguras sin causar leaks.
                if (btn1 != null) btn1.onClick.AddListener(() => AlAtaque1?.Invoke());
                if (btn2 != null) btn2.onClick.AddListener(() => AlAtaque2?.Invoke());
                if (btn3 != null) btn3.onClick.AddListener(() => AlAtaque3?.Invoke());
            }

            AplicarFuentes();
        }

        private void AplicarFuentes()
        {
            if (fuenteTitulos != null)
            {
                if (txtHeroeHP != null) txtHeroeHP.font = fuenteTitulos;
                if (txtLupusHP != null) txtLupusHP.font = fuenteTitulos;
                if (txtHelenaHP != null) txtHelenaHP.font = fuenteTitulos;
                if (txtEnemigoBasicoHP != null) txtEnemigoBasicoHP.font = fuenteTitulos;
                if (txtTurno != null) txtTurno.font = fuenteTitulos;
            }
            if (fuenteCuerpo != null && txtRegistro != null)
            {
                txtRegistro.font = fuenteCuerpo;
            }
        }

        private void OnEnable()
        {
            EventosJuego.OnVidaCambiada += ActualizarVida;
            EventosJuego.OnDanoRecibido += AgregarDanoALog;
            EventosJuego.OnPifia += AgregarPifiaALog;
            EventosJuego.OnTurnoCambiado += MostrarTurno;
        }

        private void OnDisable()
        {
            EventosJuego.OnVidaCambiada -= ActualizarVida;
            EventosJuego.OnDanoRecibido -= AgregarDanoALog;
            EventosJuego.OnPifia -= AgregarPifiaALog;
            EventosJuego.OnTurnoCambiado -= MostrarTurno;
        }

        private void MostrarTurno(string quien)
        {
            if (txtTurno != null)
            {
                txtTurno.text = $"Turno de: {quien}";
            }
        }

        private void ActualizarVida(string quien, int actual, int max)
        {
            if (string.IsNullOrEmpty(quien)) return;

            string texto = $"{quien}\nHP: {actual}/{max}";
            switch (quien.ToLower())
            {
                case "heroe":
                case "jugador":
                    if (txtHeroeHP != null) txtHeroeHP.text = texto;
                    break;
                case "lupus":
                    if (txtLupusHP != null) txtLupusHP.text = texto;
                    break;
                case "helena":
                    if (txtHelenaHP != null) txtHelenaHP.text = texto;
                    break;
                default:
                    if (txtEnemigoBasicoHP != null) txtEnemigoBasicoHP.text = texto;
                    break;
            }
        }

        private void AgregarDanoALog(string objetivo, int cantidad)
        {
            AgregarLineaRegistro($"{objetivo} recibe {cantidad} daño.");
        }

        private void AgregarPifiaALog(string quien)
        {
            AgregarLineaRegistro($"¡{quien} ha pifiado!");
        }

        public void AgregarLineaRegistro(string mensaje)
        {
            registroCombate.Enqueue(mensaje);
            if (registroCombate.Count > lineasMaximasLog)
            {
                registroCombate.Dequeue();
            }

            if (txtRegistro != null)
            {
                txtRegistro.text = string.Join("\n", registroCombate);
            }
        }
    }
}
