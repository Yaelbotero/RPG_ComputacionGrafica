
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public EncounterCombat combatManager;

    [Range(0f, 100f)]
    public float probabilidadEncuentro = 60f;

    public void EntrarHierba(Transform jugador)
    {
        if (combatManager == null)
            return;

        if (combatManager.CombateActivo)
            return;

        if (Random.Range(0f, 100f) < probabilidadEncuentro)
        {
            int enemigo = Random.Range(0, 4);

            Debug.Log("Aparece Enemigo Bosque " + (enemigo + 1));

            combatManager.IniciarCombate(enemigo);
        }
    }

    public void SalirHierba(Transform jugador)
    {
        // No se necesita ninguna accion.
    }
}
