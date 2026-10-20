using UnityEngine;

/// <summary>
/// Vuelve a cargar la escena actual a través de <see cref="UIManager"/>.
/// Pensado para la acción "Reintentar" del pop-up de fin de partida.
/// No restablece Time.timeScale: eso queda a cargo de quien pausa el juego.
/// </summary>
public class ReloadSceneCommand : ICommand
{
    private readonly UIManager uiManager;

    public ReloadSceneCommand(UIManager uiManager)
    {
        if (uiManager == null)
        {
            Debug.LogError("[ReloadSceneCommand] Se recibió un UIManager nulo.");
        }

        this.uiManager = uiManager;
    }

    public void Execute()
    {
        if (uiManager == null) return;

        uiManager.ReloadCurrentScene();
    }
}
