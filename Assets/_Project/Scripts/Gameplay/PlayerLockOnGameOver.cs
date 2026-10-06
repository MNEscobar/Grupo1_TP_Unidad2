using UnityEngine;

/// <summary>
/// Bloquea al jugador cuando se juntan todos los recolectables.
/// Escucha el evento AllCollected del ScoreManager, igual que el HUD,
/// para que PlayerController no necesite conocer el puntaje ni la UI.
/// Al reiniciar la escena el jugador se vuelve a crear, así que no
/// hace falta desbloquearlo.
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerLockOnGameOver : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;

    private PlayerController controller;
    private Rigidbody body;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (scoreManager == null)
        {
            Debug.LogError("[PlayerLockOnGameOver] Falta asignar el ScoreManager en el Inspector.");
            return;
        }

        scoreManager.AllCollected += LockPlayer;
    }

    private void OnDisable()
    {
        if (scoreManager == null) return;

        scoreManager.AllCollected -= LockPlayer;
    }

    private void LockPlayer()
    {
        // Sin PlayerController activo no se lee input ni se mueve el Rigidbody.
        controller.enabled = false;

        // Se anula la velocidad horizontal para que no siga deslizándose.
        // Se conserva la vertical para no interferir con la gravedad.
        Vector3 velocity = body.linearVelocity;
        body.linearVelocity = new Vector3(0f, velocity.y, 0f);
    }
}