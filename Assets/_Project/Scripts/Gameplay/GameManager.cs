using UnityEngine;

/// <summary>
/// Gestiona el estado básico de Gameplay, detecta la caída del jugador
/// y controla la pausa provocada por los pop-ups modales.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PopupEventSO popupEvent;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Derrota")]
    [SerializeField] private float fallLimitY = -5f;

    private bool gameEnded;

    private int activeModalPopups;
    private float timeScaleBeforeModal = 1f;

    private void Awake()
    {
        if (popupEvent == null ||
            uiManager == null ||
            playerController == null ||
            playerBody == null ||
            scoreManager == null)
        {
            Debug.LogError(
                "[GameManager] Faltan referencias en el Inspector.",
                this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!enabled || popupEvent == null || scoreManager == null)
            return;

        popupEvent.Requested += HandlePopupRequested;
        popupEvent.Closed += HandlePopupClosed;
        scoreManager.AllCollected += HandleVictory;
    }

    private void OnDisable()
    {
        if (popupEvent != null)
        {
            popupEvent.Requested -= HandlePopupRequested;
            popupEvent.Closed -= HandlePopupClosed;
        }

        if (scoreManager != null)
            scoreManager.AllCollected -= HandleVictory;

        // Evita dejar el juego pausado si el objeto se desactiva.
        if (activeModalPopups > 0)
        {
            Time.timeScale = timeScaleBeforeModal;
            activeModalPopups = 0;
        }
    }

    private void Update()
    {
        if (gameEnded || playerBody == null)
            return;

        if (playerBody.position.y < fallLimitY)
            HandlePlayerDeath();
    }

    private void HandlePlayerDeath()
    {
        // Evita emitir varias solicitudes mientras el jugador cae.
        gameEnded = true;

        // Bloquea el control del jugador.
        playerController.enabled = false;

        // Congela el cuerpo físico para evitar que siga cayendo
        // mientras se muestra el pop-up.
        playerBody.linearVelocity = Vector3.zero;
        playerBody.isKinematic = true;

        popupEvent.Raise(new PopupRequest(
            "Fin de la partida",
            "¿Reintentar?",
            new ReloadSceneCommand(uiManager),
            new GoToMainMenuCommand(uiManager),
            isModal: true));
    }

    private void HandleVictory()
    {
        // La victoria también finaliza la partida.
        // Evita que una caída posterior abra un pop-up de derrota.
        gameEnded = true;
    }

    private void HandlePopupRequested(PopupRequest request)
    {
        if (request == null || !request.IsModal)
            return;

        // Conserva el estado previo solo al abrir el primer modal.
        if (activeModalPopups == 0)
            timeScaleBeforeModal = Time.timeScale;

        activeModalPopups++;
        Time.timeScale = 0f;
    }

    private void HandlePopupClosed(PopupRequest request)
    {
        if (request == null ||
            !request.IsModal ||
            activeModalPopups == 0)
        {
            return;
        }

        activeModalPopups--;

        // Reanuda cuando se cierra el último modal.
        if (activeModalPopups == 0)
            Time.timeScale = timeScaleBeforeModal;
    }
}