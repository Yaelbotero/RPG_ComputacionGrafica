using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParalaxMove : MonoBehaviour
{
    [SerializeField]
    private Vector2 velMovi;

    private Vector2 offset;
    private Material mat;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        mat = GetComponent<SpriteRenderer>().material;
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        offset = (rb.linearVelocity.x * 0.1f) * velMovi * Time.deltaTime;
        mat.mainTextureOffset += offset;
    }
}
