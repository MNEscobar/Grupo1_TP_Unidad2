using UnityEngine;

/// <summary>
/// Vuelve al menú principal a través de <see cref="UIManager"/>.
/// Pensado para la acción "Volver al menú" del pop-up de fin de partida.
/// No restablece Time.timeScale: eso queda a cargo de quien pausa el juego.
/// </summary>
public class GoToMainMenuCommand : ICommand
{
    private readonly UIManager uiManager;

    public GoToMainMenuCommand(UIManager uiManager)
    {
        if (uiManager == null)
        {
            Debug.LogError("[GoToMainMenuCommand] Se recibió un UIManager nulo.");
        }

        this.uiManager = uiManager;
    }

    public void Execute()
    {
        if (uiManager == null) return;

        uiManager.GoToMainMenu();
    }
}
