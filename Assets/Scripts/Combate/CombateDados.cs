using System;
using UnityEngine;

namespace RPG.Combate
{
    /// <summary>
    /// Lógica de combate por turnos con dados. No dibuja nada: la interfaz escucha los eventos
    /// y anima el dado hasta la cara que ya se decidió en Tirar().
    /// Flujo de un turno:  Tirar(indice) -> la UI anima los dados -> Aplicar(resultado).
    /// </summary>
    public class CombateDados : MonoBehaviour
    {
        [Header("Reglas")]
        [Tooltip("Una tirada igual o menor a este valor es PIFIA (globo normal).")]
        public int umbralPifia = 2;
        [Tooltip("Cada cuántos puntos de Fuerza se suma +1 al daño.")]
        public int fuerzaPorPunto = 10;

        public Combatiente heroe = new Combatiente();
        public Combatiente enemigo = new Combatiente { nombre = "Enemigo", fuerza = 10, resistencia = 40 };

        public bool TurnoJugador { get; private set; }
        public bool EnCurso { get; private set; }

        public event Action<Combatiente, Combatiente> CombateIniciado;
        public event Action<Combatiente, bool> TurnoCambiado;               // quién, es jugador
        public event Action<Combatiente, ResultadoAtaque> AtaqueTirado;     // la UI anima aquí
        public event Action<Combatiente, Combatiente, ResultadoAtaque> AtaqueAplicado;
        public event Action<bool> CombateTerminado;                         // true = ganó el héroe

        public void Iniciar(Combatiente h, Combatiente e)
        {
            heroe = h; enemigo = e;
            heroe.Restaurar(); enemigo.Restaurar();
            EnCurso = true;
            TurnoJugador = true;
            if (CombateIniciado != null) CombateIniciado(heroe, enemigo);
            if (TurnoCambiado != null) TurnoCambiado(heroe, true);
        }

        /// <summary>Decide el resultado de los dados (sin aplicar daño todavía).</summary>
        public ResultadoAtaque Tirar(Combatiente atacante, int indiceAtaque)
        {
            AtaqueDados a = atacante.ataques[Mathf.Clamp(indiceAtaque, 0, atacante.ataques.Length - 1)];
            ResultadoAtaque r = new ResultadoAtaque();
            r.ataque = a;
            r.tirada = UnityEngine.Random.Range(1, a.carasDadoAtaque + 1);
            r.pifia = r.tirada <= umbralPifia;
            r.exito = !r.pifia;
            r.extras = new int[r.exito ? a.dadosExtra : 0];
            int suma = 0;
            for (int i = 0; i < r.extras.Length; i++)
            {
                r.extras[i] = UnityEngine.Random.Range(1, a.carasDadoExtra + 1);
                suma += r.extras[i];
            }
            r.bonoFuerza = r.exito ? atacante.fuerza / Mathf.Max(1, fuerzaPorPunto) : 0;
            r.danio = r.exito ? r.tirada + suma + r.bonoFuerza : 0;
            if (AtaqueTirado != null) AtaqueTirado(atacante, r);
            return r;
        }

        /// <summary>Resta la vida y pasa el turno. La UI lo llama cuando termina de animar.</summary>
        public void Aplicar(Combatiente atacante, ResultadoAtaque r)
        {
            if (!EnCurso) return;
            Combatiente defensor = atacante == heroe ? enemigo : heroe;
            defensor.vidaActual = Mathf.Max(0, defensor.vidaActual - r.danio);
            if (AtaqueAplicado != null) AtaqueAplicado(atacante, defensor, r);

            if (!defensor.EstaVivo)
            {
                EnCurso = false;
                if (CombateTerminado != null) CombateTerminado(atacante == heroe);
                return;
            }
            TurnoJugador = !TurnoJugador;
            if (TurnoCambiado != null) TurnoCambiado(TurnoJugador ? heroe : enemigo, TurnoJugador);
        }

        /// <summary>Atajo para el enemigo: elige un ataque al azar.</summary>
        public ResultadoAtaque TirarEnemigo()
        {
            return Tirar(enemigo, UnityEngine.Random.Range(0, enemigo.ataques.Length));
        }
    }
}
