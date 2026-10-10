using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{
    public float speed = 20f;
    public Rigidbody2D rb;
    public float damage;

    private void Start()
    {
        rb.linearVelocity = transform.right * speed;
       Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerManager>().getDamage(damage);
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Enemigo"))
        {
            collision.gameObject.GetComponent<Enemigo>().getDamage(damage);
            Destroy(gameObject);
        }
        
    }
}
