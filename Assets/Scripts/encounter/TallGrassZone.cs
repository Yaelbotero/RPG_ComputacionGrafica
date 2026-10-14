
using UnityEngine;

public class TallGrassZone : MonoBehaviour
{
    public EncounterManager encounterManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (encounterManager != null)
        {
            encounterManager.EntrarHierba(
                other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (encounterManager != null)
        {
            encounterManager.SalirHierba(
                other.transform);
        }
    }
}
