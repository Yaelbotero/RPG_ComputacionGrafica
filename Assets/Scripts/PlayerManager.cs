using UnityEngine;  

public class PlayerManager : MonoBehaviour
{
    public float salud;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void getDamage(float dmg)
    {
        salud -= dmg;
        if (salud<=0)
        {
            Destroy(gameObject);
        }
    }
}
