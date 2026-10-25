using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Parte visual del prefab Popup_Base: título, mensaje, botones
/// Aceptar/Cancelar y un fondo que bloquea la interacción con el resto
/// de la UI cuando el pop-up es modal.
/// No sabe qué hace cada botón: quien lo muestra (el PopupManager) le
/// pasa las acciones en <see cref="Show"/>. Así el mismo prefab sirve
/// para reiniciar, salir o cualquier otra confirmación.
/// </summary>
public class PopupView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    // Image estirada a toda la pantalla. Con raycastTarget activo, los
    // clicks no llegan a los elementos que quedan detrás del pop-up.
    [SerializeField] private Image blocker;
    [SerializeField] private Color modalBlockerColor = new Color(0f, 0f, 0f, 0.6f);

    /// <summary>
    /// Configura y muestra el pop-up. <paramref name="onAccept"/> y
    /// <paramref name="onCancel"/> se ejecutan una sola vez: después del
    /// primer click los botones quedan deshabilitados hasta el próximo Show.
    /// </summary>
    public void Show(string title, string message, Action onAccept, Action onCancel, bool isModal)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        SetupButton(acceptButton, onAccept);
        SetupButton(cancelButton, onCancel);
        SetModal(isModal);

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta el pop-up y suelta las acciones, para que una instancia
    /// reutilizada no ejecute los comandos de la solicitud anterior.
    /// </summary>
    public void Hide()
    {
        ClearButton(acceptButton);
        ClearButton(cancelButton);
        gameObject.SetActive(false);
    }

    private void SetupButton(Button button, Action action)
    {
        if (button == null)
        {
            Debug.LogError($"[PopupView] Falta asignar un botón en \"{name}\".");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // Evita que un doble click ejecute la acción dos veces.
            SetButtonsInteractable(false);
            action?.Invoke();
        });
        button.interactable = true;
    }

    private void ClearButton(Button button)
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (acceptButton != null) acceptButton.interactable = interactable;
        if (cancelButton != null) cancelButton.interactable = interactable;
    }

    private void SetModal(bool isModal)
    {
        if (blocker == null) return;

        blocker.color = isModal ? modalBlockerColor : Color.clear;
        blocker.raycastTarget = isModal;
    }
}
