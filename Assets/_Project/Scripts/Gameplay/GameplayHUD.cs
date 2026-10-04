using TMPro;
using UnityEngine;

/// <summary>
/// HUD de la escena Gameplay: muestra el contador de puntos y el panel
/// de victoria cuando se juntan todos los recolectables.
/// Los botones del panel (Reiniciar / Volver) llaman directamente al
/// UIManager desde el Inspector, igual que el resto de la UI.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject winPanel;

    private void OnEnable()
    {
        if (scoreManager == null)
        {
            Debug.LogError("[GameplayHUD] Falta asignar el ScoreManager en el Inspector.");
            return;
        }

        scoreManager.ScoreChanged += UpdateScore;
        scoreManager.AllCollected += ShowWinPanel;
    }

    private void OnDisable()
    {
        if (scoreManager == null) return;

        scoreManager.ScoreChanged -= UpdateScore;
        scoreManager.AllCollected -= ShowWinPanel;
    }

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (scoreManager != null)
            UpdateScore(scoreManager.Score, scoreManager.TotalPoints);
    }

    private void UpdateScore(int score, int total)
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {score} / {total}";
    }

    private void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }
}
