
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TestPlayerMovement : MonoBehaviour
{
    public float velocidad = 4f;
    public EncounterCombat combatManager;

    private Rigidbody2D rb;
    private Vector2 direccion;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        direccion = Vector2.zero;

        // El jugador no se mueve durante el combate.
        if (combatManager != null &&
            combatManager.CombateActivo)
        {
            return;
        }

        Keyboard teclado = Keyboard.current;

        if (teclado == null)
            return;

        if (teclado.aKey.isPressed)
            direccion.x = -1f;

        if (teclado.dKey.isPressed)
            direccion.x = 1f;
    }

    void FixedUpdate()
    {
        if (combatManager != null &&
            combatManager.CombateActivo)
        {
            return;
        }

        Vector2 nuevaPosicion =
            rb.position + direccion *
            velocidad * Time.fixedDeltaTime;

        rb.MovePosition(nuevaPosicion);
    }
}
