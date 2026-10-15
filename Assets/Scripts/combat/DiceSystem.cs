
using UnityEngine;

public static class DiceSystem
{
    // Lanza un dado y devuelve el resultado.
    public static int Roll(int caras)
    {
        return Random.Range(1, caras + 1);
    }

    // Calcula el daño a partir de los dados del ataque.
    // Ejemplos: 1D6, 1D10+1D4, 1D10+2D4.
    public static int RollAttack(string formula)
    {
        int danoTotal = 0;

        string[] partes = formula.ToUpper()
            .Replace(" ", "")
            .Split('+');

        foreach (string parte in partes)
        {
            string[] valores = parte.Split('D');

            int cantidad = int.Parse(valores[0]);
            int caras = int.Parse(valores[1]);

            for (int i = 0; i < cantidad; i++)
            {
                int resultado = Roll(caras);

                Debug.Log("D" + caras + ": " + resultado);

                danoTotal += resultado;
            }
        }

        Debug.Log("DANO TOTAL: " + danoTotal);

        return danoTotal;
    }
}
