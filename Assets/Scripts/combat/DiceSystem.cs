
using UnityEngine;

public static class DiceSystem
{
    // Lanza un dado de cualquier cantidad de caras.
    public static int Roll(int sides)
    {
        return Random.Range(1, sides + 1);
    }

    // Tirada de exito general (1D10).
    // 1 a 3: Fallo.
    // 4 a 10: Exito.
    public static bool RollSuccess()
    {
        int result = Roll(10);

        Debug.Log("Tirada de exito D10: " + result);

        if (result <= 3)
        {
            Debug.Log("FALLO: El ataque no acierta.");
            return false;
        }

        Debug.Log("EXITO: El ataque acierta.");
        return true;
    }

    // Lanza los dados de un ataque y suma resultados.
    // Ejemplo: 1D10+1D4 o 2D6.
    public static int RollAttack(string formula)
    {
        int totalDamage = 0;

        string[] parts = formula.ToUpper()
            .Replace(" ", "").Split('+');

        foreach (string part in parts)
        {
            string[] values = part.Split('D');

            int amount = int.Parse(values[0]);
            int sides = int.Parse(values[1]);

            for (int i = 0; i < amount; i++)
            {
                int result = Roll(sides);

                Debug.Log("D" + sides + ": " + result);

                totalDamage += result;
            }
        }

        Debug.Log("DANO TOTAL: " + totalDamage);

        return totalDamage;
    }
}
