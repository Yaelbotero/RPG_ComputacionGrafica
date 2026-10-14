
using System.Collections.Generic;
using UnityEngine;

public class BossAI
{
    private float healChance;
    private int healDiceSides;

    public BossAI(float chance, int diceSides)
    {
        healChance = Mathf.Clamp01(chance);
        healDiceSides = Mathf.Max(1, diceSides);
    }

    public void LupusTurn(
        CharacterData lupus,
        CharacterData helena,
        List<CharacterData> heroes)
    {
        if (!lupus.IsAlive) return;

        Debug.Log("--- TURNO DE LUPUS ---");

        // Decision de regeneracion.
        if (!helena.IsAlive && 
            lupus.currentHealth <= lupus.maxHealth / 2)
        {
            if (Random.value < healChance)
            {
                int previousHP = lupus.currentHealth;
                int healing = DiceSystem.Roll(healDiceSides);

                lupus.Heal(healing);

                Debug.Log("Lupus utiliza magia regenerativa.");
                Debug.Log("Curacion obtenida: " + healing);
                Debug.Log("Vida: " + previousHP +
                    " -> " + lupus.currentHealth);

                return;
            }
        }

        // Tirada especial de la diapositiva 15.
        int roll = DiceSystem.Roll(10);

        Debug.Log("Tirada especial de Lupus 1D10: " + roll);

        if (roll >= 8 && roll <= 9)
        {
            Debug.Log("PIFIA: no se realiza el ataque.");
            return;
        }

        if (roll == 7 || roll == 10)
        {
            Debug.Log("Resultado no definido: sin ataque.");
            return;
        }

        // Resultados exitosos: 1 a 6.
        if (roll >= 1 && roll <= 6)
        {
            CharacterData target = ChooseTarget(heroes);

            if (target == null) return;

            if (helena != null && helena.IsAlive)
            {
                Debug.Log("Lupus solicita ayuda de Helena.");
                ApplyDamage(helena, target);
            }
            else
            {
                Debug.Log("Helena ha muerto. Lupus ataca solo.");
                ApplyDamage(lupus, target);
            }
        }
    }

    public void HelenaTurn(
        CharacterData helena,
        List<CharacterData> heroes)
    {
        if (!helena.IsAlive) return;

        Debug.Log("--- TURNO DE HELENA ---");

        CharacterData target = ChooseTarget(heroes);

        if (target == null) return;

        if (DiceSystem.RollSuccess())
        {
            ApplyDamage(helena, target);
        }
        else
        {
            Debug.Log("Helena falla su ataque.");
        }
    }

    private void ApplyDamage(
        CharacterData attacker,
        CharacterData target)
    {
        string attack = attacker.GetRandomAttack();

        Debug.Log(attacker.characterName +
            " ataca a " + target.characterName);

        Debug.Log("Dados: " + attack);

        int damage = DiceSystem.RollAttack(attack);

        target.TakeDamage(damage);

        Debug.Log("Dano realizado: " + damage);

        Debug.Log(target.characterName +
            " HP: " + target.currentHealth +
            "/" + target.maxHealth);

        if (!target.IsAlive)
        {
            Debug.Log(target.characterName +
                " HA SIDO DERROTADO.");
        }
    }

    private CharacterData ChooseTarget(
        List<CharacterData> heroes)
    {
        List<CharacterData> alive = heroes.FindAll(
            hero => hero.IsAlive);

        if (alive.Count == 0)
            return null;

        return alive[Random.Range(0, alive.Count)];
    }
}
