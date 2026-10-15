using UnityEngine;

/// <summary>
/// Guarda las lineas de dialogo de un NPC para editarlas desde el inspector.
/// </summary>
[CreateAssetMenu(fileName = "NuevoDialogoNPC", menuName = "Dialogos/Dialogo NPC")]
public class DialogoNPC : ScriptableObject
{
    [TextArea(4, 6)]
    public string[] lineas;
}
