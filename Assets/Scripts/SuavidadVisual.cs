using UnityEngine;

/// <summary>
/// Sincroniza el renderizado con la pantalla mientras la escena que lo contiene esta activa
/// y restaura los ajustes originales al salir. Solo se agrega en la escena Bosque.
/// </summary>
public class SuavidadVisual : MonoBehaviour
{
    [SerializeField] private Rigidbody2D jugador;

    private int vSyncOriginal;

    private void Awake()
    {
        vSyncOriginal = QualitySettings.vSyncCount;
        QualitySettings.vSyncCount = 1;

        if (jugador != null)
        {
            jugador.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void OnDestroy()
    {
        QualitySettings.vSyncCount = vSyncOriginal;
    }
}
