using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la Splashscreen (escena Bootstrap): pasa sola al menú
/// principal después de unos segundos, o antes si el jugador toca
/// cualquier tecla o hace clic.
/// La navegación se delega en el UIManager de la escena para no
/// duplicar la lógica de carga de escenas.
/// </summary>
public class SplashController : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    [Tooltip("Segundos que se muestra el splash antes de ir al menú.")]
    [SerializeField] private float duration = 3f;

    private float elapsed;
    private bool hasLeft;

    private void Update()
    {
        if (hasLeft) return;

        // Se usa tiempo sin escala para que el splash avance aunque
        // Time.timeScale haya quedado en 0.
        elapsed += Time.unscaledDeltaTime;

        if (elapsed >= duration || SkipPressed())
            LeaveSplash();
    }

    private static bool SkipPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return key || click;
    }

    private void LeaveSplash()
    {
        // Evita cargar el menú dos veces (por ejemplo, si el jugador
        // hace clic justo cuando termina el temporizador).
        hasLeft = true;

        if (uiManager == null)
        {
            Debug.LogError("[SplashController] Falta asignar el UIManager en el Inspector.");
            return;
        }

        uiManager.GoToMainMenu();
    }
}
