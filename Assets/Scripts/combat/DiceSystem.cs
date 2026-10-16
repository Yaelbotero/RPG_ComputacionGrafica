
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

    // ------------------------------------------------------------------
    // TIRADA DETALLADA (para la interfaz de dados)
    // Regla: el PRIMER dado de la formula es el dado de ATAQUE.
    //  - Si sale en la zona de pifia (1 .. caras/5, minimo 1) el ataque falla y no hace dano.
    //  - Si no, es EXITO: se tiran los dados restantes y el dano es la suma de todos.
    // ------------------------------------------------------------------
    public static int UmbralPifia(int caras)
    {
        return Mathf.Max(1, caras / 5);
    }

    public static ResultadoDado TirarAtaque(string formula)
    {
        ResultadoDado r = new ResultadoDado();
        r.formula = formula;

        System.Collections.Generic.List<int> carasExtras = new System.Collections.Generic.List<int>();
        bool primero = true;

        string[] partes = formula.ToUpper().Replace(" ", "").Split('+');

        foreach (string parte in partes)
        {
            string[] valores = parte.Split('D');
            int cantidad = int.Parse(valores[0]);
            int caras = int.Parse(valores[1]);

            for (int i = 0; i < cantidad; i++)
            {
                if (primero)
                {
                    r.carasPrincipal = caras;
                    primero = false;
                }
                else
                {
                    carasExtras.Add(caras);
                }
            }
        }

        r.tiradaPrincipal = Roll(r.carasPrincipal);
        r.pifia = r.tiradaPrincipal <= UmbralPifia(r.carasPrincipal);

        r.carasExtras = carasExtras.ToArray();
        r.extras = new int[r.carasExtras.Length];

        if (r.pifia)
        {
            r.danio = 0;
            return r;
        }

        r.danio = r.tiradaPrincipal;
        for (int i = 0; i < r.carasExtras.Length; i++)
        {
            r.extras[i] = Roll(r.carasExtras[i]);
            r.danio += r.extras[i];
        }

        return r;
    }
}

[System.Serializable]
public class ResultadoDado
{
    public string formula;
    public int carasPrincipal;
    public int tiradaPrincipal;
    public bool pifia;
    public int[] carasExtras = new int[0];
    public int[] extras = new int[0];
    public int danio;
}
