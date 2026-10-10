using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player_Attack : MonoBehaviour
{
    [SerializeField] private Transform controlGolpe;
    [SerializeField] private float radioGolpe;
    [SerializeField] private float damageGolpe;
    [SerializeField] private float tiemEntreAtaque;
    [SerializeField] private float tiemSigAtaque;

    private Animator anim;
    private Movimiento mov;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        mov = GetComponent<Movimiento>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tiemSigAtaque > 0f)
        {
            tiemSigAtaque = Mathf.Max(0f, tiemSigAtaque - Time.deltaTime);
        }

        if (Input.GetButtonDown("Fire1") && tiemSigAtaque <= 0f)
        {
            anim.SetTrigger("Attack");
            tiemSigAtaque = tiemEntreAtaque;
        }
    }

    public void Atacar()
    {
        Collider2D[] enemigos = Physics2D. OverlapCircleAll(controlGolpe.position, radioGolpe);

        foreach (Collider2D colision in enemigos)
        {
            if (colision.CompareTag("Enemigo"))
            {
                colision.transform.GetComponent<Enemigo>().getDamage(damageGolpe);
            }
        }
    }

    public void StopMove()
    {
        mov.attack = true;
        mov.horizontal = 0;
        mov.anim.SetFloat("Caminar", 0);
    }

    public void RestarMove()
    {
        mov.attack = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controlGolpe.position, radioGolpe);
    }
}
