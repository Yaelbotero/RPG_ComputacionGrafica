using UnityEngine;  

public class PlayerManager : MonoBehaviour
{
    public float salud;
    // Se activa cuando Nun termina de irse; mientras sea false las puertas no desactivan al jugador.
    public static bool puertaHabilitada = false;

    private void Awake()
    {
        puertaHabilitada = false;
    }

    public void getDamage(float dmg)
    {
        salud -= dmg;
        if (salud<=0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DesactivarSiEsPuerta(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DesactivarSiEsPuerta(collision.gameObject);
    }

    public GameObject canvasManana;
    public float segundosEspera = 3f;
    private bool enTransicion;

    private void DesactivarSiEsPuerta(GameObject objeto)
    {
        int capaPuerta = LayerMask.NameToLayer("Puerta");
        if (puertaHabilitada && !enTransicion && capaPuerta != -1 && objeto.layer == capaPuerta)
        {
            enTransicion = true;
            if (canvasManana == null)
            {
                Debug.LogWarning("PlayerManager: no se asignó canvasManana en el Inspector.");
            }
            // Un objeto independiente espera, porque este se desactiva y sus corrutinas se detendrían.
            GameObject temporizador = new GameObject("TemporizadorCanvasManana");
            temporizador.AddComponent<ActivadorRetardado>().Iniciar(canvasManana, segundosEspera);
            gameObject.SetActive(false);
        }
    }
}

public class ActivadorRetardado : MonoBehaviour
{
    public void Iniciar(GameObject objetivo, float segundos)
    {
        StartCoroutine(Esperar(objetivo, segundos));
    }

    private System.Collections.IEnumerator Esperar(GameObject objetivo, float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        if (objetivo != null)
        {
            objetivo.SetActive(true);
        }
        Destroy(gameObject);
    }
}
