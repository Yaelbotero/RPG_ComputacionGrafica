using UnityEngine;

public class FijarRotacionUI : MonoBehaviour
{
    private Quaternion rotacionFija;

    [Header("Objetivo a seguir")]
    public Transform objetivo; // Aquí pondrás a tu jugador
    private Vector3 offset;

    [Header("Tiempo visible")]
    public float tiempoVisible = 2f; // Tiempo que el texto será visible después de aparecer
    private float temporizador = 0f;
    private int monedasAnteriores = 0;
    private Canvas miCanvas;

    void Start()
    {
        miCanvas = GetComponent<Canvas>();
        // Memoriza la rotación original del Canvas al iniciar el juego (mirando a la cámara)
        rotacionFija = transform.rotation;
        // Calcula automáticamente la distancia exacta a la que acomodaste el texto en la escena
        if (objetivo != null)
        {
            offset = transform.position - objetivo.position;
        }
        // Sincroniza la cantidad inicial de monedas leyendo tu GameManager
        if (GameManager.Instance != null)
        {
            monedasAnteriores = GameManager.Instance.cantMonedas;
        }

        if (miCanvas != null)
        {
            miCanvas.enabled = false; // Inicialmente desactiva el Canvas
        }
    }

    void LateUpdate()
    {
        // Tras anular cualquier giro que el jugador intente forzarle, vuelve a su rotación original
        transform.rotation = rotacionFija;
        if (objetivo != null)
        {
            transform.position = objetivo.position + offset;
        }

        if (GameManager.Instance != null)
        {
            // Verifica si la cantidad de monedas ha cambiado
            if (GameManager.Instance.cantMonedas > monedasAnteriores && GameManager.Instance != null)
            {
                // Actualiza el temporizador y la cantidad de monedas anteriores
                monedasAnteriores = GameManager.Instance.cantMonedas;
                temporizador = tiempoVisible;

                // Activa el Canvas para mostrar el texto
                if (miCanvas != null)
                {
                    miCanvas.enabled = true;
                }
            }

            if (temporizador > 0)
            {
                temporizador -= Time.deltaTime;
                if (temporizador <= 0)
                {
                    // Desactiva el Canvas cuando el tiempo se agote
                    if (temporizador <= 0 && miCanvas != null)
                    {
                        miCanvas.enabled = false;
                    }
                }
            }
        }
    }
}