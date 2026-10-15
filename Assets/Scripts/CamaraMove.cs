using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamaraMove : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.20f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    // LateUpdate is called once per frame after all Update calls
    [SerializeField] private Transform target;

    // Borde visible del mapa; la camara se detiene cuando su vista llega a estos limites.
    [SerializeField] private bool limitarEnX = true;
    [SerializeField] private float limiteIzquierdo = -17.4f;
    [SerializeField] private float limiteDerecho = 36.1f;

    void LateUpdate()
    {
        
        Vector3 targetPos = target.position + offset;
        if (limitarEnX)
        {
            Camera cam = Camera.main;
            float mediaVista = cam.orthographicSize * cam.aspect;
            float minX = limiteIzquierdo + mediaVista;
            float maxX = limiteDerecho - mediaVista;
            targetPos.x = minX > maxX ? (limiteIzquierdo + limiteDerecho) * 0.5f : Mathf.Clamp(targetPos.x, minX, maxX);
        }
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
