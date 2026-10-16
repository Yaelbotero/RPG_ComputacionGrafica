using UnityEngine;

// Solo para Bosque: la monja mira siempre al jugador y lo sigue por detras en X.
public class NunSigueJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform enemigoGrock;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Seguimiento")]
    [SerializeField] private float distanciaDetras = 2f;
    [SerializeField] private float velocidad = 6.5f;
    [SerializeField] private float margen = 0.1f;
    [SerializeField] private string parametroCaminar = "Caminar";

    // Lado de la monja respecto al jugador: +1 derecha, -1 izquierda.
    private int lado = -1;
    private bool tieneCaminar;

    private void Awake()
    {
        // Solo en la escena "Pelea Final", Nun permanece fija y no sigue al personaje.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Pelea Final")
        {
            enabled = false;
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        IgnorarColisionConJugador();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == parametroCaminar && p.type == AnimatorControllerParameterType.Float)
                {
                    tieneCaminar = true;
                }
            }
        }
    }

    // Solo los colliders solidos: los triggers (interaccion con E) siguen activos.
    private void IgnorarColisionConJugador()
    {
        IgnorarColisionCon(jugador);
        IgnorarColisionCon(enemigoGrock);
    }

    private void IgnorarColisionCon(Transform otro)
    {
        if (otro == null)
        {
            return;
        }

        foreach (Collider2D mio in GetComponents<Collider2D>())
        {
            if (mio.isTrigger) continue;
            foreach (Collider2D suyo in otro.GetComponentsInChildren<Collider2D>())
            {
                if (suyo.isTrigger) continue;
                Physics2D.IgnoreCollision(mio, suyo, true);
            }
        }
    }

    private void Update()
    {
        if (jugador == null)
        {
            return;
        }

        float dx = transform.position.x - jugador.position.x;
        if (Mathf.Abs(dx) > 0.05f)
        {
            lado = dx > 0f ? 1 : -1;
        }

        // Mira siempre al jugador.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = jugador.position.x < transform.position.x;
        }

        float objetivoX = jugador.position.x + lado * distanciaDetras;
        bool debeCaminar = Mathf.Abs(dx) > distanciaDetras + margen;

        if (debeCaminar)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, objetivoX, velocidad * Time.deltaTime);
            transform.position = pos;
        }

        if (tieneCaminar)
        {
            animator.SetFloat(parametroCaminar, debeCaminar ? 1f : 0f);
        }
    }
}
