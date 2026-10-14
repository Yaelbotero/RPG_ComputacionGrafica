
using UnityEngine;

public class DiceTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== INICIO PRUEBA DE DADOS ===");

        // Primero se comprueba si el ataque acierta.
        Debug.Log("HEROE 1 - ATAQUE 1");

        bool success = DiceSystem.RollSuccess();

        if (success)
        {
            DiceSystem.RollAttack("1D10+1D4");
        }
        else
        {
            Debug.Log("El heroe no hace dano.");
        }

        // Prueba independiente de dados.
        Debug.Log("PRUEBA ATAQUE 2D6");
        DiceSystem.RollAttack("2D6");

        // Prueba independiente con tres dados.
        Debug.Log("PRUEBA ATAQUE 1D10+2D4");
        DiceSystem.RollAttack("1D10+2D4");

        Debug.Log("=== FIN DE PRUEBA ===");
    }
}
