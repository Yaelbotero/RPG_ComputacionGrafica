namespace RPG.Flujo
{
    /// <summary>
    /// Constantes con los nombres exactos de las escenas en Unity para evitar errores de tipeo.
    /// </summary>
    public static class NombresEscenas
    {
        public const string MENU = "Menu";
        public const string PUEBLO = "Pueblo";
        public const string BOSQUE = "Bosque";
        public const string COMBATE = "Combate";
        public const string BOSS = "Boss";
        // NOTA: No existe una escena "Fin". La SecuenciaFin se muestra mediante un objeto 'Fin' presente en las escenas Boss y Combate.
    }
}
