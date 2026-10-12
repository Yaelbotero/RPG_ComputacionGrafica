using UnityEngine;

// Coloca el panel de dialogo justo encima del NPC y lo mantiene siguiendolo.
public class DialogoSigueNPC : MonoBehaviour
{
    [SerializeField] private SpriteRenderer npc;
    [SerializeField] private RectTransform panel;
    [SerializeField] private float margenSuperior = 0.15f;

    private void LateUpdate()
    {
        if (npc == null || panel == null || !panel.gameObject.activeInHierarchy)
        {
            return;
        }

        Bounds b = npc.bounds;
        Vector3 p = panel.position;
        p.x = b.center.x;
        p.y = b.max.y + margenSuperior;
        panel.position = p;
    }
}
