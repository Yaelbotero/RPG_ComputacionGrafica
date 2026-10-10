using UnityEngine;

/// <summary>
/// Controla la caminata de la monja durante la cinemática de introducción.
/// </summary>
public class NunController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float distanciaLlegada = 0.01f;

    [Header("Animator")]
    [SerializeField] private string parametroCaminar = "Caminar";
    [SerializeField] private string parametroIdle = "Idle";

    private Vector2 destino;
    private Vector2 posicionActual;
    private Vector2 nuevaPosicion;
    private bool caminando;

    /// <summary>
    /// Indica si la monja terminó la caminata actual.
    /// </summary>
    public bool CaminataTerminada { get; private set; } = true;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (!caminando)
        {
            return;
        }
        int nun = 0;
        posicionActual = transform.position;
        nuevaPosicion = Vector2.MoveTowards(posicionActual, destino, velocidad * Time.deltaTime);
        if (nun == 0)
        {
        transform.position = new Vector3(nuevaPosicion.x, nuevaPosicion.y, transform.position.z);
        nun = 1;
        }
        
        if (TieneParametro(parametroCaminar, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parametroCaminar, 1f);
        }

        if (Vector2.Distance(nuevaPosicion, destino) <= distanciaLlegada)
        {
            transform.position = new Vector3(destino.x, destino.y, transform.position.z);
            caminando = false;
            CaminataTerminada = true;
            CambiarAIdle();
        }

    }
    /// <summary>
    /// Inicia la caminata de la monja hacia el destino indicado.
    /// </summary>
    public void IniciarCaminata(Vector2 nuevoDestino)
    {
        destino = nuevoDestino;
        caminando = true;
        CaminataTerminada = false;
        CambiarAEstadoCaminando();
    }

private void CambiarAEstadoCaminando()
    {
        if (animator == null)
        {
            return;
        }

        if (TieneParametro(parametroIdle, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parametroIdle, false);
        }

        if (TieneParametro(parametroCaminar, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parametroCaminar, 1f);
        }
    }

private void CambiarAIdle()
    {
        if (animator == null)
        {
            return;
        }

        // Las dos condiciones deben actualizarse para que la transición Walk -> Idle ocurra.
        if (TieneParametro(parametroCaminar, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parametroCaminar, 0f);
        }

        if (TieneParametro(parametroIdle, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parametroIdle, true);
        }

        if (animator.HasState(0, Animator.StringToHash("Base Layer.Idle")))
        {
            animator.Play("Base Layer.Idle", 0, 0f);
        }

        else if (TieneParametro(parametroIdle, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(parametroIdle);
        }
    }

    private bool TieneParametro(string nombre, AnimatorControllerParameterType tipo)
    {
        if (string.IsNullOrEmpty(nombre) || animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.name == nombre && parametro.type == tipo)
            {
                return true;
            }
        }

        return false;
    }
}
