
using System.Collections.Generic;

public static class CharacterDatabase
{
    public static CharacterData CreateHero()
    {
        return new CharacterData(
            "Heroe 2", 23, 87,
            "1D10", "1D10+1D4", "1D10+2D4"
        );
    }

    public static CharacterData CreateForestEnemy(int index)
    {
        switch (index)
        {
            case 0:
                return new CharacterData(
                    "Enemigo Bosque 1", 15, 13, "1D6");

            case 1:
                return new CharacterData(
                    "Enemigo Bosque 2", 18, 21, "1D10");

            case 2:
                return new CharacterData(
                    "Enemigo Bosque 3", 12, 23, "1D4");

            case 3:
                return new CharacterData(
                    "Enemigo Bosque 4", 19, 35, "1D10");

            default:
                return null;
        }
    }

    public static List<CharacterData> CreateBosses()
    {
        return new List<CharacterData>
        {
            new CharacterData(
                "Lupus", 20, 82, "1D4+1D6", "2D6"),

            new CharacterData(
                "Helena", 18, 35, "1D8+1D6")
        };
    }
}
