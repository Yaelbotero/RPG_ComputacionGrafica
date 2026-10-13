using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Salud")]
    public float salud;

    [Header("Rotación")]
    public bool mirandoEnemigo;

    [Header("Detección")]
    public float distanciaParaAtacarPlayer = 5f;

    private GameObject target;
    private Vector3 escalaInicial;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        escalaInicial = transform.localScale;

        if (target == null)
        {
            Debug.LogWarning("No se encontró el jugador con tag 'Player'.");
        }
    }

    void Update()
    {
        if (target == null)
            return;

        float distancia = Vector2.Distance(transform.position, target.transform.position);

        if (distancia < distanciaParaAtacarPlayer)
        {
            MirarJugador();
        }
    }

    void MirarJugador()
    {
        float direccion = target.transform.position.x - transform.position.x;

        if (direccion > 0f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
            mirandoEnemigo = true;
        }
        else if (direccion < 0f)
        {
            transform.localScale = new Vector3(Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
            mirandoEnemigo = false;
        }
    }

    public void getDamage(float dmg)
    {
        salud -= dmg;

        if (salud <= 0)
        {
            Destroy(gameObject);
        }
    }
}
