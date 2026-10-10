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
    void LateUpdate()
    {
        
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
