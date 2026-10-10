using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisparoPrefab : MonoBehaviour
{
    public Transform firePoint;
    public GameObject balaPrefab;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Disparar();
        }
    }

    public void Disparar()
    {
        Instantiate(balaPrefab, firePoint.position, firePoint.rotation);
    }
}
