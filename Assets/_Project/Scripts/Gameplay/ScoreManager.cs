using System;
using UnityEngine;

/// <summary>
/// Lleva el puntaje de la partida. Al iniciar cuenta los recolectables
/// que hay en la escena para saber el total posible, y avisa por
/// eventos cuando cambia el puntaje y cuando se juntaron todos.
/// La UI (GameplayHUD) escucha estos eventos en vez de consultar
/// el puntaje en cada frame.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    /// <summary>Puntaje actual y puntaje total posible.</summary>
    public event Action<int, int> ScoreChanged;

    public event Action AllCollected;

    public int Score { get; private set; }
    public int TotalPoints { get; private set; }

    private int remaining;

    private void Awake()
    {
        // Se calcula en Awake para que el HUD ya tenga el total en su Start.
        Collectible[] collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        remaining = collectibles.Length;

        foreach (Collectible collectible in collectibles)
            TotalPoints += collectible.Points;
    }

    private void OnEnable()
    {
        Collectible.Collected += HandleCollected;
    }

    private void OnDisable()
    {
        // El evento es estático: si no se desuscribe, al recargar la
        // escena quedaría apuntando a un ScoreManager destruido.
        Collectible.Collected -= HandleCollected;
    }

    private void HandleCollected(Collectible collectible)
    {
        Score += collectible.Points;
        remaining--;

        ScoreChanged?.Invoke(Score, TotalPoints);

        if (remaining <= 0)
            AllCollected?.Invoke();
    }
}
