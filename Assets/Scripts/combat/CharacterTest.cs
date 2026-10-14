
using System.Collections.Generic;
using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== PRUEBA DE PERSONAJES ===");

        List<CharacterData> heroes =
            CharacterDatabase.CreateHeroes();

        List<CharacterData> bosses =
            CharacterDatabase.CreateBosses();

        Debug.Log("--- HEROES ---");

        foreach (CharacterData hero in heroes)
        {
            ShowCharacter(hero);
        }

        Debug.Log("--- ENEMIGOS DEL BOSQUE ---");

        for (int i = 0; i < 4; i++)
        {
            CharacterData enemy =
                CharacterDatabase.CreateForestEnemy(i);

            ShowCharacter(enemy);
        }

        Debug.Log("--- JEFES ---");

        foreach (CharacterData boss in bosses)
        {
            ShowCharacter(boss);
        }

        Debug.Log("--- PRUEBA DE VIDA ---");

        CharacterData hero1 = heroes[0];

        Debug.Log("Vida inicial: " + hero1.currentHealth);

        hero1.TakeDamage(10);

        Debug.Log("Despues de recibir 10 de dano: " +
            hero1.currentHealth);

        Debug.Log("Sigue vivo: " + hero1.IsAlive);

        Debug.Log("--- PRUEBA DE ATAQUE ---");

        string attack = hero1.GetRandomAttack();

        Debug.Log("Ataque seleccionado: " + attack);

        if (DiceSystem.RollSuccess())
        {
            int damage = DiceSystem.RollAttack(attack);
            Debug.Log("Dano causado: " + damage);
        }

        Debug.Log("=== FIN DE PRUEBA ===");
    }

    void ShowCharacter(CharacterData character)
    {
        Debug.Log(
            character.characterName +
            " | Fuerza: " + character.strength +
            " | Vida: " + character.currentHealth +
            " | Ataques: " + string.Join(", ", character.attacks)
        );
    }
}
