using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Salud")]
    public float salud;

    [Header("Rotación")]
    public bool mirandoEnemigo;

    [Header("Detección")]
    public float distanciaParaAtacarPlayer = 5f;

    [Header("Ataque")]
    public Transform controlGolpe;
    public float radioGolpe;
    public float damageGolpe = 10f;
    public float tiempoEntreAtaques = 1.5f;
    private float tiempoSiguienteAtaque;

    private GameObject target;
    private Vector3 escalaInicial;
    private Animator anim;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        escalaInicial = transform.localScale;
        anim = GetComponent<Animator>();

        if (target == null)
        {
            Debug.LogWarning("No se encontró el jugador con tag 'Player'.");
        }
    }

    void Update()
    {
        // Temporizador para el tiempo de enfriamiento del ataque
        if (tiempoSiguienteAtaque > 0)
        {
            tiempoSiguienteAtaque -= Time.deltaTime;
        }

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

        // INVERSIÓN DE SIGNOS: Arregla el problema de que mire al lado opuesto
        if (direccion > 0f) // El jugador está a la derecha
        {
            // Ponemos el signo negativo aquí para voltear el sprite hacia la derecha
            transform.localScale = new Vector3(-Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
            mirandoEnemigo = true;
        }
        else if (direccion < 0f) // El jugador está a la izquierda
        {
            // Lo dejamos positivo para que mantenga su vista original hacia la izquierda
            transform.localScale = new Vector3(Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
            mirandoEnemigo = false;
        }
    }

    // Se activa cuando el jugador entra en el colisionador del enemigo
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IntentarAtacar();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IntentarAtacar();
        }
    }

    private void IntentarAtacar()
    {
        if (tiempoSiguienteAtaque > 0)
            return;

        if (anim != null)
        {
            anim.SetTrigger("Enemigo_Attack");
        }

        tiempoSiguienteAtaque = tiempoEntreAtaques;
    }

    // Este es el método que llamarás desde el Evento de Animación
    public void Atacar()
    {
        Collider2D[] jugadores = Physics2D.OverlapCircleAll(controlGolpe.position, radioGolpe);

        foreach (Collider2D colision in jugadores)
        {
            if (colision.CompareTag("Player"))
            {
                // Busca el PlayerManager y aplica el daño
                colision.GetComponent<PlayerManager>().getDamage(damageGolpe);
            }
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

    // Dibuja el círculo rojo en la escena para configurar el ataque
    private void OnDrawGizmos()
    {
        if (controlGolpe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controlGolpe.position, radioGolpe);
        }
    }
}