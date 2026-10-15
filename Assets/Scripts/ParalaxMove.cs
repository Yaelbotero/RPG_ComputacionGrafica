using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Paralaje solo horizontal: la capa (SpriteRenderer en modo Tiled) mantiene su altura y se mueve en X respecto a la camara.
[DefaultExecutionOrder(100)]
public class ParalaxMove : MonoBehaviour
{
    // Fraccion de la velocidad de la camara con la que se mueve el contenido:
    // 1 = fijo al mundo (sin paralaje), 0 = pegado a la camara (como el cielo).
    [SerializeField, Range(0f, 1f)]
    private float velocidadRelativa = 0.6f;

    // true: capa Tiled que se reubica en saltos de un tile. false: objeto unico (p. ej. un edificio) que solo se desplaza.
    [SerializeField] private bool repetir = true;

    private Transform camara;
    private float alturaFija;
    private float anchoMundo;
    private float xInicial;
    private float camXInicial;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        anchoMundo = sr.sprite.bounds.size.x * Mathf.Abs(transform.lossyScale.x);
        alturaFija = transform.position.y;
        xInicial = transform.position.x;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            camara = Camera.main.transform;
            camXInicial = camara.position.x;
        }
    }

    // La capa se desplaza con la camara a (1 - velocidadRelativa) y se reubica en saltos de un
    // ancho de tile (el patron es identico), asi nunca quedan huecos ni se solapan imagenes.
    private void LateUpdate()
    {
        if (camara == null || (repetir && anchoMundo <= 0f))
        {
            return;
        }

        float camX = camara.position.x;
        float x = xInicial + (camX - camXInicial) * (1f - velocidadRelativa);
        if (repetir)
        {
            x += anchoMundo * Mathf.Round((camX - x) / anchoMundo);
        }

        transform.position = new Vector3(x, alturaFija, transform.position.z);
    }
}
