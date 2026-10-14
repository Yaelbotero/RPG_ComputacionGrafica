
using System.Collections.Generic;

public static class CharacterDatabase
{
    // Cuatro heroes del juego.
    public static List<CharacterData> CreateHeroes()
    {
        return new List<CharacterData>
        {
            new CharacterData(
                "Heroe 1", 19, 68,
                "1D10+1D4", "2D6"),

            new CharacterData(
                "Heroe 2", 23, 87,
                "1D10", "1D10+1D4", "1D10+2D4"),

            new CharacterData(
                "Heroe 3", 17, 50,
                "1D10+1D6"),

            new CharacterData(
                "Heroe 4", 16, 38,
                "1D6+1D8")
        };
    }

    // Los cuatro enemigos del bosque.
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

    // Jefes de la batalla final.
    public static List<CharacterData> CreateBosses()
    {
        return new List<CharacterData>
        {
            new CharacterData(
                "Lupus", 20, 82,
                "1D4+1D6", "2D6"),

            new CharacterData(
                "Helena", 18, 35,
                "1D8+1D6")
        };
    }
}
