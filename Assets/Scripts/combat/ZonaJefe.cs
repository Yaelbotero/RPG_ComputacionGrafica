
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZonaJefe : MonoBehaviour
{
    [Header("Configuracion")]
    public string escenaJefe = "CombateJefes";

    private bool activado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        Debug.Log("El heroe ha llegado al jefe.");

        SceneManager.LoadScene(escenaJefe);
    }
}
