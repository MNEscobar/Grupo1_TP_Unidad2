using UnityEngine;

/// <summary>
/// Solicita la confirmación de salida del juego a través del servicio de
/// pop-ups (Observer): arma la <see cref="PopupRequest"/> y la emite por
/// <see cref="PopupEventSO"/> sin conocer la ventana que la mostrará.
/// Aceptar ejecuta <see cref="QuitApplicationCommand"/> (Command);
/// Cancelar no tiene comando, así que solo cierra el pop-up.
/// La navegación sigue en <see cref="UIManager"/>; este componente
/// solo decide qué diálogo pedir.
/// </summary>
public class ExitConfirmationRequester : MonoBehaviour
{
    [SerializeField] private PopupEventSO popupEvent;
    [SerializeField] private UIManager uiManager;

    /// <summary>
    /// Pensado para el OnClick del botón Salir.
    /// </summary>
    public void RequestExit()
    {
        // Se valida acá y no en Awake: un UnityEvent llama a los métodos
        // públicos aunque el componente esté deshabilitado.
        if (popupEvent == null || uiManager == null)
        {
            Debug.LogError("[ExitConfirmationRequester] Faltan referencias en el Inspector.", this);
            return;
        }

        popupEvent.Raise(new PopupRequest(
            "Confirmar salida",
            "¿Seguro que quieres salir?",
            new QuitApplicationCommand(uiManager),
            cancelCommand: null,
            isModal: true));
    }
}