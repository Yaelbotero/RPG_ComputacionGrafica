using UnityEngine;

public class Pared_Derecha : MonoBehaviour
{
    [Header("Canvas a activar al tocar la pared")]
    public GameObject canvasCambiarEscena;

    private void ActivarCanvas(GameObject playerObj)
    {
        if (canvasCambiarEscena != null && !canvasCambiarEscena.activeSelf)
        {
            canvasCambiarEscena.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var mov = playerObj.GetComponent<Movimiento>();
        if (mov != null)
        {
            mov.bloqueado = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ActivarCanvas(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ActivarCanvas(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ActivarCanvas(collision.gameObject);
        }
    }
}
