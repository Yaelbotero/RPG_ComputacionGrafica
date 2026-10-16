
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrullaje")]
    public float distancia = 3f;
    public float velocidad = 2f;

    [Header("Encuentro")]
    public EncounterManager encounterManager;

    [Min(1)]
    public int idEnemigo = 1;

    private Vector3 puntoA;
    private Vector3 puntoB;

    private bool haciaB = true;
    private bool encontroJugador = false;

    void Awake()
    {
        // Ocultar enemigos derrotados en esta partida.
        if (EncounterManager.EstaDerrotado(idEnemigo))
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        puntoA = transform.position;
        puntoB = puntoA + Vector3.right * distancia;
    }

    void Update()
    {
        if (encontroJugador)
            return;

        Vector3 destino = haciaB ? puntoB : puntoA;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, destino) < 0.01f)
        {
            haciaB = !haciaB;

            Vector3 escala = transform.localScale;
            escala.x *= -1;
            transform.localScale = escala;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (encontroJugador)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (EncounterManager.EstaDerrotado(idEnemigo))
            return;

        if (encounterManager == null)
        {
            Debug.LogWarning(
                "Falta EncounterManager.");
            return;
        }

        encontroJugador = true;

        Debug.Log("Contacto con patrullero " + idEnemigo);

        // Enviar identificador y posicion del heroe.
        encounterManager.IniciarEncuentro(
            idEnemigo,
            other.transform.position);
    }
}
