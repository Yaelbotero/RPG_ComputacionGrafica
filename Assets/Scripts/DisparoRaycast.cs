using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisparoRaycast : MonoBehaviour
{
    public Transform firePoint;
    public LineRenderer line;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(Disparar());
        }
    }

    IEnumerator Disparar()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);

        if (hitInfo)
        {
            Debug.Log(hitInfo.transform.name);

            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hitInfo.point);
        }
        else
        {
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, firePoint.position + firePoint.right * 20f);
        }

        line.enabled = true;

        yield return new WaitForSeconds(0.02f);

        line.enabled = false;
    }
}
