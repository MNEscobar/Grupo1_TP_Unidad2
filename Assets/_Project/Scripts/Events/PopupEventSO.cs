using System;
using UnityEngine;

/// <summary>
/// Canal de eventos (Observer) para solicitar pop-ups.
/// Los emisores llaman a <see cref="Raise"/> y el PopupManager escucha
/// <see cref="Requested"/>, así ninguno de los dos necesita referenciar al otro.
/// </summary>
[CreateAssetMenu(menuName = "Grupo1/Events/Popup Event", fileName = "PopupEvent")]
public class PopupEventSO : ScriptableObject
{
    public event Action<PopupRequest> Requested;

    public void Raise(PopupRequest request)
    {
        if (request == null)
        {
            Debug.LogError($"[PopupEventSO] Se intentó emitir una solicitud nula en \"{name}\".");
            return;
        }

        if (Requested == null)
        {
            // Normalmente indica que falta un PopupManager en la escena.
            Debug.LogWarning($"[PopupEventSO] Nadie escucha el canal \"{name}\"; el pop-up \"{request.Title}\" no se mostrará.");
            return;
        }

        Requested.Invoke(request);
    }
}
