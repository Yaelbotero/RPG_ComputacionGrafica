using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlataformaMovimiento : MonoBehaviour
{
    public Transform[] puntosMovimiento;
    public float velocidad;

    private int indiceActual = 0;
    private Vector3 moverHacia;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moverHacia = puntosMovimiento[indiceActual].position;   
    }

    // FixedUpdate is called once per frame at a fixed interval
    void FixedUpdate()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, moverHacia, velocidad * Time.fixedDeltaTime);

        if (Vector3.Distance(this.transform.position, moverHacia) < 0.02f)
        {
            indiceActual++;
            if (indiceActual >= puntosMovimiento.Length)
            {
                indiceActual = 0;
            }

            moverHacia = puntosMovimiento[indiceActual].position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
        }
    }
}
