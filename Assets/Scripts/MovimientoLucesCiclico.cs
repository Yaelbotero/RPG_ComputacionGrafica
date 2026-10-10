using UnityEngine;

public class MovimientoLucesCiclico : MonoBehaviour
{
    [Header("Movimiento cíclico")]
    [SerializeField, Min(0f)] private float amplitudHorizontal = 0.15f;
    [SerializeField, Min(0f)] private float amplitudVertical = 0.08f;
    [SerializeField, Min(0f)] private float velocidad = 0.8f;
    [SerializeField] private float desfase = 0f;

    [Header("Transparencia")]
    [SerializeField, Range(0f, 1f)] private float transparenciaMinima = 0.8f;
    [SerializeField, Range(0f, 1f)] private float transparenciaMaxima = 1f;
    [SerializeField, Min(0f)] private float velocidadTransparencia = 0.7f;


    

    private SpriteRenderer[] renderizadores;
    private Color[] coloresIniciales;
    private Vector3 posicionInicial;

    private void Awake()
    {
        posicionInicial = transform.localPosition;
        renderizadores = GetComponentsInChildren<SpriteRenderer>(true);
        coloresIniciales = new Color[renderizadores.Length];

        for (int i = 0; i < renderizadores.Length; i++)
        {
            coloresIniciales[i] = renderizadores[i].color;
        }
    }

    private void Update()
    {
        float movimiento = (Time.time + desfase) * velocidad;
        float desplazamientoX = Mathf.Sin(movimiento) * amplitudHorizontal;
        float desplazamientoY = Mathf.Sin(movimiento * 1.37f) * amplitudVertical;

        transform.localPosition = posicionInicial + new Vector3(desplazamientoX, desplazamientoY, 0f);

        float transparenciaBaja = Mathf.Min(transparenciaMinima, transparenciaMaxima);
        float transparenciaAlta = Mathf.Max(transparenciaMinima, transparenciaMaxima);
        float cicloTransparencia = (Mathf.Sin((Time.time + desfase) * velocidadTransparencia) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(transparenciaBaja, transparenciaAlta, cicloTransparencia);

        for (int i = 0; i < renderizadores.Length; i++)
        {
            Color color = coloresIniciales[i];
            color.a *= alpha;
            renderizadores[i].color = color;
        }
    }
}