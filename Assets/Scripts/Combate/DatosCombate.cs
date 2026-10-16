using System;
using UnityEngine;

namespace RPG.Combate
{
    /// <summary>Un ataque definido por dados. Ej: Ataque 3 = 1D10 + 2D4.</summary>
    [Serializable]
    public class AtaqueDados
    {
        public string nombre = "Ataque 1";
        [Min(2)] public int carasDadoAtaque = 10;   // D10: dado principal
        [Min(0)] public int dadosExtra = 0;         // cuántos dados extra de daño
        [Min(2)] public int carasDadoExtra = 4;     // D4 por defecto
    }

    /// <summary>Datos de un combatiente (héroe o enemigo).</summary>
    [Serializable]
    public class Combatiente
    {
        public string nombre = "Héroe 2";
        public int fuerza = 23;        // bono de daño = fuerza / fuerzaPorPunto
        public int resistencia = 87;   // vida máxima
        public AtaqueDados[] ataques = new AtaqueDados[]
        {
            new AtaqueDados { nombre = "Ataque 1", dadosExtra = 0 },
            new AtaqueDados { nombre = "Ataque 2", dadosExtra = 1 },
            new AtaqueDados { nombre = "Ataque 3", dadosExtra = 2 },
        };

        [NonSerialized] public int vidaActual;

        public void Restaurar() { vidaActual = resistencia; }
        public bool EstaVivo { get { return vidaActual > 0; } }
    }

    /// <summary>Resultado de tirar un ataque. La interfaz lo usa para animar los dados.</summary>
    public class ResultadoAtaque
    {
        public AtaqueDados ataque;
        public int tirada;          // cara que cae en el dado de ataque
        public bool pifia;          // globo normal
        public bool exito;          // globo dorado
        public int[] extras;        // caras de los dados de daño extra
        public int bonoFuerza;
        public int danio;           // daño final (0 si hubo pifia)
    }
}
