using System.Collections;
using UnityEngine;

/// <summary>
/// Orquesta la cinemática de la escena Pelea Final una vez finalizado el diálogo entre Nun y Personaje.
/// Secuencia:
/// 1. Nun camina con animación hacia (x=42.3, y=-3.14).
/// 2. Nun se desactiva.
/// 3. Helena Malvada se activa.
/// 4. Enemy minotaur y Helena Malvada se mueven con animación hacia sus destinos:
///    - Enemy minotaur hacia (x=36.87, y=-0.24)
///    - Helena Malvada hacia (x=33.83, y=-3.01)
/// 5. Ambos miran siempre al Personaje.
/// 6. Helena Malvada y Enemy minotaur tienen sus colisiones ignoradas entre sí para evitar colisiones.
/// </summary>
public class CinematicaPeleaFinal : MonoBehaviour
{
    [Header("Referencias de Personajes")]
    [SerializeField] private GameObject nun;
    [SerializeField] private TextManagerNun textManagerNun;
    [SerializeField] private NunController nunController;
    [SerializeField] private GameObject helenaMalvada;
    [SerializeField] private GameObject enemyMinotaur;
    [SerializeField] private Transform personaje;

    [Header("Destinos")]
    [SerializeField] private Vector2 destinoNun = new Vector2(42.3f, -3.14f);
    [SerializeField] private Vector2 destinoHelena = new Vector2(33.83f, -3.01f);
    [SerializeField] private Vector2 destinoMinotaur = new Vector2(36.87f, -0.24f);

    [Header("Velocidades")]
    [SerializeField] private float velocidadNun = 2.5f;
    [SerializeField] private float velocidadHelena = 2.0f;
    [SerializeField] private float velocidadMinotaur = 1.9f;

    [Header("Animaciones")]
    [SerializeField] private string animCaminarHelena = "HelenaMala_Caminando";
    [SerializeField] private string animIdleHelena = "HelenaMala_Idle";
    [SerializeField] private string animCaminarMinotaur = "minotaur_Caminando";
    [SerializeField] private string animIdleMinotaur = "minotaur_idle";

    private SpriteRenderer srNun;
    private SpriteRenderer srHelena;
    private SpriteRenderer srMinotaur;

    private Animator animHelena;
    private Animator animMinotaur;

    private Rigidbody2D rbHelena;
    private Rigidbody2D rbMinotaur;

    private bool cinematicaIniciada = false;

    private void Awake()
    {
        ResolverReferencias();
    }

    private void Start()
    {
        ConfigurarColisiones();
        PrepararEstadoInicial();

        if (textManagerNun != null)
        {
            textManagerNun.DialogoTerminado -= OnDialogoTerminado;
            textManagerNun.DialogoTerminado += OnDialogoTerminado;
        }
    }

    private void OnDestroy()
    {
        if (textManagerNun != null)
        {
            textManagerNun.DialogoTerminado -= OnDialogoTerminado;
        }
    }

    private void LateUpdate()
    {
        ActualizarMiradas();
    }

    /// <summary>
    /// Encuentra y enlaza todas las referencias necesarias en la escena.
    /// </summary>
    public void ResolverReferencias()
    {
        if (nun == null) nun = BuscarEnEscena("Nun");
        if (textManagerNun == null && nun != null) textManagerNun = nun.GetComponent<TextManagerNun>();
        if (nunController == null && nun != null) nunController = nun.GetComponent<NunController>();
        if (srNun == null && nun != null) srNun = nun.GetComponent<SpriteRenderer>();

        if (helenaMalvada == null) helenaMalvada = BuscarEnEscena("Helena Malvada");
        if (helenaMalvada != null)
        {
            srHelena = helenaMalvada.GetComponent<SpriteRenderer>();
            animHelena = helenaMalvada.GetComponent<Animator>();
            rbHelena = helenaMalvada.GetComponent<Rigidbody2D>();
        }

        if (enemyMinotaur == null) enemyMinotaur = BuscarEnEscena("Enemy minotaur");
        if (enemyMinotaur != null)
        {
            srMinotaur = enemyMinotaur.GetComponent<SpriteRenderer>();
            animMinotaur = enemyMinotaur.GetComponent<Animator>();
            rbMinotaur = enemyMinotaur.GetComponent<Rigidbody2D>();
        }

        if (personaje == null)
        {
            var pObj = BuscarEnEscena("Personaje");
            if (pObj != null) personaje = pObj.transform;
        }
    }

    /// <summary>
    /// Desactiva Helena Malvada inicialmente y asegura configuraciones iniciales.
    /// </summary>
    private void PrepararEstadoInicial()
    {
        if (helenaMalvada != null)
        {
            helenaMalvada.SetActive(false);
        }

        if (textManagerNun != null)
        {
            // Evitar que el diálogo se repita tras terminar
            var so = new UnityEditor.SerializedObject(textManagerNun);
#if UNITY_EDITOR
            var soloUnaVezProp = so.FindProperty("soloUnaVez");
            if (soloUnaVezProp != null)
            {
                soloUnaVezProp.boolValue = true;
                so.ApplyModifiedProperties();
            }
#endif
        }

        if (enemyMinotaur != null && animMinotaur != null)
        {
            animMinotaur.Play(animIdleMinotaur);
        }

        ActualizarMiradas();
    }

    /// <summary>
    /// Configura Physics2D.IgnoreCollision entre todos los colliders de Helena Malvada y Enemy minotaur
    /// y de Nun para asegurar que no choquen entre ellos.
    /// </summary>
    public void ConfigurarColisiones()
    {
        IgnorarColisionesEntre(helenaMalvada, enemyMinotaur);
        IgnorarColisionesEntre(nun, helenaMalvada);
        IgnorarColisionesEntre(nun, enemyMinotaur);
    }

    private void IgnorarColisionesEntre(GameObject a, GameObject b)
    {
        if (a == null || b == null) return;

        var colsA = a.GetComponentsInChildren<Collider2D>(true);
        var colsB = b.GetComponentsInChildren<Collider2D>(true);

        foreach (var ca in colsA)
        {
            foreach (var cb in colsB)
            {
                Physics2D.IgnoreCollision(ca, cb, true);
            }
        }
    }

    private void OnDialogoTerminado()
    {
        if (cinematicaIniciada) return;
        IniciarCinematica();
    }

    /// <summary>
    /// Inicia la cinemática de transición y caminata.
    /// </summary>
    [ContextMenu("Forzar Inicio Cinematica")]
    public void IniciarCinematica()
    {
        if (cinematicaIniciada) return;
        cinematicaIniciada = true;
        StartCoroutine(SecuenciaCinematicaCorrutina());
    }

    private IEnumerator SecuenciaCinematicaCorrutina()
    {
        // -------------------------------------------------------------
        // PASO 1: Nun camina con animación hacia (x=42.3, y=-3.14)
        // -------------------------------------------------------------
        if (nun != null && nun.activeInHierarchy)
        {
            // Nun debe mirar hacia la derecha (hacia el destino 42.3)
            if (srNun != null)
            {
                srNun.flipX = destinoNun.x < nun.transform.position.x;
            }

            if (nunController != null)
            {
                nunController.IniciarCaminata(destinoNun);

                float timeoutNun = 12f;
                while (!nunController.CaminataTerminada && timeoutNun > 0f)
                {
                    timeoutNun -= Time.deltaTime;
                    if (srNun != null)
                    {
                        srNun.flipX = destinoNun.x < nun.transform.position.x;
                    }
                    yield return null;
                }
            }
            else
            {
                // Respaldo de movimiento si nunController no estuviera disponible
                while (Vector2.Distance(nun.transform.position, destinoNun) > 0.05f)
                {
                    nun.transform.position = Vector3.MoveTowards(nun.transform.position, new Vector3(destinoNun.x, destinoNun.y, nun.transform.position.z), velocidadNun * Time.deltaTime);
                    yield return null;
                }
            }

            nun.transform.position = new Vector3(destinoNun.x, destinoNun.y, nun.transform.position.z);
            yield return new WaitForSeconds(0.2f);

            // -------------------------------------------------------------
            // PASO 2: Nun se desactiva
            // -------------------------------------------------------------
            nun.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        // -------------------------------------------------------------
        // PASO 3: Helena Malvada se activa
        // -------------------------------------------------------------
        if (helenaMalvada != null)
        {
            helenaMalvada.SetActive(true);
            ConfigurarColisiones();
            ActualizarMiradas();
            yield return new WaitForSeconds(0.15f);
        }

        // -------------------------------------------------------------
        // PASO 4: Enemy minotaur y Helena Malvada se mueven con animación
        //         Helena hacia (33.83, -3.01) y Minotaur hacia (36.87, -0.24)
        // -------------------------------------------------------------
        if (animHelena != null) animHelena.Play(animCaminarHelena);
        if (animMinotaur != null) animMinotaur.Play(animCaminarMinotaur);

        bool helenaLlego = (helenaMalvada == null);
        bool minotaurLlego = (enemyMinotaur == null);
        float timeoutMov = 15f;

        while ((!helenaLlego || !minotaurLlego) && timeoutMov > 0f)
        {
            timeoutMov -= Time.deltaTime;

            // Movimiento Helena
            if (!helenaLlego && helenaMalvada != null)
            {
                Vector3 posH = helenaMalvada.transform.position;
                Vector3 metaH = new Vector3(destinoHelena.x, destinoHelena.y, posH.z);
                helenaMalvada.transform.position = Vector3.MoveTowards(posH, metaH, velocidadHelena * Time.deltaTime);

                DetenerVelocidadFisica(rbHelena);

                if (Vector2.Distance(new Vector2(helenaMalvada.transform.position.x, helenaMalvada.transform.position.y), destinoHelena) <= 0.03f)
                {
                    helenaMalvada.transform.position = metaH;
                    helenaLlego = true;
                    if (animHelena != null) animHelena.Play(animIdleHelena);
                }
            }

            // Movimiento Minotauro
            if (!minotaurLlego && enemyMinotaur != null)
            {
                Vector3 posM = enemyMinotaur.transform.position;
                Vector3 metaM = new Vector3(destinoMinotaur.x, destinoMinotaur.y, posM.z);
                enemyMinotaur.transform.position = Vector3.MoveTowards(posM, metaM, velocidadMinotaur * Time.deltaTime);

                DetenerVelocidadFisica(rbMinotaur);

                if (Vector2.Distance(new Vector2(enemyMinotaur.transform.position.x, enemyMinotaur.transform.position.y), destinoMinotaur) <= 0.03f)
                {
                    enemyMinotaur.transform.position = metaM;
                    minotaurLlego = true;
                    if (animMinotaur != null) animMinotaur.Play(animIdleMinotaur);
                }
            }

            yield return null;
        }

        // Asegurar posiciones exactas y estado Idle final
        if (helenaMalvada != null)
        {
            helenaMalvada.transform.position = new Vector3(destinoHelena.x, destinoHelena.y, helenaMalvada.transform.position.z);
            if (animHelena != null) animHelena.Play(animIdleHelena);
        }

        if (enemyMinotaur != null)
        {
            enemyMinotaur.transform.position = new Vector3(destinoMinotaur.x, destinoMinotaur.y, enemyMinotaur.transform.position.z);
            if (animMinotaur != null) animMinotaur.Play(animIdleMinotaur);
        }
    }

    private void DetenerVelocidadFisica(Rigidbody2D rb)
    {
        if (rb == null) return;
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
    }

    /// <summary>
    /// Hace que Helena Malvada y Enemy minotaur miren constantemente al Personaje.
    /// </summary>
    private void ActualizarMiradas()
    {
        if (personaje == null) return;

        // Helena por defecto mira hacia la derecha en su sprite.
        // Si Personaje está a la izquierda (personaje.x < transform.x), flipX = true.
        if (helenaMalvada != null && helenaMalvada.activeInHierarchy && srHelena != null)
        {
            srHelena.flipX = personaje.position.x < helenaMalvada.transform.position.x;
        }

        // Minotauro por defecto mira hacia la izquierda en su sprite (arma detrás a la derecha, cuernos/cara al frente hacia la izquierda).
        // Si Personaje está a la derecha (personaje.x > transform.x), flipX = true para voltearlo hacia la derecha.
        // Si Personaje está a la izquierda (personaje.x <= transform.x), flipX = false para mantenerlo mirando a la izquierda hacia Personaje.
        if (enemyMinotaur != null && enemyMinotaur.activeInHierarchy && srMinotaur != null)
        {
            srMinotaur.flipX = personaje.position.x > enemyMinotaur.transform.position.x;
        }
    }

    private GameObject BuscarEnEscena(string nombre)
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.name == nombre) return root;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.gameObject.name == nombre) return t.gameObject;
            }
        }
        return null;
    }
}
