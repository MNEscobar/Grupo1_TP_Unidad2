using UnityEngine;

/// <summary>
/// Cierra la aplicación a través de <see cref="UIManager"/>
/// (en el Editor detiene el Play Mode).
/// Pensado para la acción "Aceptar" del pop-up de confirmación de salida.
/// </summary>
public class QuitApplicationCommand : ICommand
{
    private readonly UIManager uiManager;

    public QuitApplicationCommand(UIManager uiManager)
    {
        if (uiManager == null)
        {
            Debug.LogError("[QuitApplicationCommand] Se recibió un UIManager nulo.");
        }

        this.uiManager = uiManager;
    }

    public void Execute()
    {
        if (uiManager == null) return;

        uiManager.QuitApplication();
    }
}
