/// <summary>
/// Datos de una solicitud de pop-up: qué mostrar y qué ejecutar con cada botón.
/// El emisor la arma y la envía por <see cref="PopupEventSO"/> sin conocer
/// la implementación visual de la ventana.
/// </summary>
public class PopupRequest
{
    public string Title { get; }
    public string Message { get; }

    /// <summary>Acción del botón Aceptar.</summary>
    public ICommand AcceptCommand { get; }

    /// <summary>
    /// Acción del botón Cancelar. Si es null, Cancelar solo cierra el pop-up.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Si es true, la ventana bloquea la interacción con el resto de la UI.
    /// </summary>
    public bool IsModal { get; }

    public PopupRequest(string title, string message, ICommand acceptCommand, ICommand cancelCommand = null, bool isModal = true)
    {
        Title = title;
        Message = message;
        AcceptCommand = acceptCommand;
        CancelCommand = cancelCommand;
        IsModal = isModal;
    }
}
