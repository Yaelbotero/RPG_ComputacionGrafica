using UnityEngine;

public class MovimientoNubes : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 2f;
    public float limiteIzquierdo = -25f; // Posición X donde la nube desaparece (fuera de cámara a la izquierda)
    public float puntoReinicio = 25f;    // Posición X donde la nube reaparece (fuera de cámara a la derecha)

    void Update()
    {
        // Mueve la nube constantemente hacia la izquierda usando el tiempo real
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);

        // Verifica si la nube ya cruzó el límite izquierdo
        if (transform.position.x <= limiteIzquierdo)
        {
            // Teletransporta la nube a la derecha, manteniendo su altura (Y) original
            transform.position = new Vector3(puntoReinicio, transform.position.y, transform.position.z);
        }
    }
}