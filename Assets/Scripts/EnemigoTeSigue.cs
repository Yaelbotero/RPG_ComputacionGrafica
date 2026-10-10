using UnityEngine;

public class EnemigoTeSigue : MonoBehaviour
{
    [Header("Salud")]
    public float salud;

    [Header("Rotación")]
    public bool mirandoEnemigo;

    [Header("Detección")]
    public float distanciaParaAtacarPlayer = 5f;
    public float velocidadMovimiento = 2f;

    [Header("Ataque")]
    public Transform controlGolpe;
    public Transform player;
    public float radioGolpe;
    public float damageGolpe = 10f;
    public float tiempoEntreAtaques = 1.5f;
    private float tiempoSiguienteAtaque;

    private GameObject target;
    private Vector3 escalaInicial;
    private Vector2 direccionMovimiento;
    private Rigidbody2D rb;
    private Animator anim;
    private bool ataqueEnCurso;
    private bool animacionAtaqueIniciada;
    public float horizontal;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player");
        if (player == null && target != null)
        {
            player = target.transform;
        }
        escalaInicial = transform.localScale;
        anim = GetComponent<Animator>();

        if (player == null)
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

        if (ataqueEnCurso && anim != null)
        {
            AnimatorStateInfo estadoActual = anim.GetCurrentAnimatorStateInfo(0);
            bool enAtaque = estadoActual.IsName("Enemigo_Attack") ||
                (anim.IsInTransition(0) && anim.GetNextAnimatorStateInfo(0).IsName("Enemigo_Attack"));

            if (enAtaque)
            {
                animacionAtaqueIniciada = true;
            }
            else if (animacionAtaqueIniciada)
            {
                ataqueEnCurso = false;
                animacionAtaqueIniciada = false;
            }
        }

        if (player == null)
        {
            horizontal = 0f;
            direccionMovimiento = Vector2.zero;
            if (anim != null)
            {
                anim.SetFloat("Caminar", 0f);
            }
            return;
        }

        float distancia = Vector2.Distance(transform.position, player.position);

        if (ataqueEnCurso)
        {
            horizontal = 0f;
        }
        else if (distancia < distanciaParaAtacarPlayer)
        {
            MirarJugador();
            float diferenciaX = player.position.x - transform.position.x;
            horizontal = Mathf.Abs(diferenciaX) > 0.05f ? Mathf.Sign(diferenciaX) : 0f;
        }
        else
        {
            horizontal = 0f;
        }

        direccionMovimiento = new Vector2(horizontal, 0f);
        if (anim != null)
        {
            anim.SetFloat("Caminar", Mathf.Abs(horizontal));
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + direccionMovimiento * velocidadMovimiento * Time.fixedDeltaTime);
        }
    }

    void MirarJugador()
    {
        float direccion = player.position.x - transform.position.x;

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
            ataqueEnCurso = true;
            animacionAtaqueIniciada = false;
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