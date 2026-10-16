
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public int strength;
    public int maxHealth;
    public int currentHealth;
    public List<string> attacks;

    public bool IsAlive
    {
        get { return currentHealth > 0; }
    }

    public CharacterData(
        string name,
        int strength,
        int health,
        params string[] attacks)
    {
        characterName = name;
        this.strength = strength;
        maxHealth = health;
        currentHealth = health;

        this.attacks = new List<string>(attacks);
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(
            0, currentHealth - Mathf.Max(0, damage));
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(
            maxHealth, currentHealth + Mathf.Max(0, amount));
    }
}
