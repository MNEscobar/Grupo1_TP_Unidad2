# Patrones de diseño implementados

El servicio de pop-ups reutilizable combina tres patrones: **Observer** (cómo se solicita un pop-up), **Command** (qué hace cada botón) y **Object Pool** (cómo se reutiliza la ventana). Quien necesita un pop-up solo arma una solicitud y la publica; el resto ocurre sin que lo conozca.

```text
Emisor (GameManager / ExitConfirmationRequester)
  └─ PopupEventSO.Raise(PopupRequest)                                    ← Observer
       └─ PopupManager.HandleRequested → pool.Get() → PopupView.Show     ← Object Pool
            └─ clic en un botón → Close(): pool.Release() → NotifyClosed() → command.Execute()   ← Command
```

## Observer

`PopupEventSO` es un `ScriptableObject` que funciona como canal de eventos. Los componentes que necesitan un pop-up llaman a `Raise` con un `PopupRequest`; `PopupManager` se suscribe a `Requested` y atiende cada solicitud. De esta manera, quien pide el pop-up no conoce al administrador de la interfaz ni al prefab, y el acoplamiento entre ambos se reduce al asset compartido.

**Publicación del evento** — `Assets/_Project/Scripts/Events/PopupEventSO.cs`

```csharp
public event Action<PopupRequest> Requested;
public event Action<PopupRequest> Closed;

public void Raise(PopupRequest request)
{
    // ... validaciones (solicitud nula o nadie escuchando el canal)

    Requested.Invoke(request);
}
```

**Suscripción y atención de solicitudes** — `Assets/_Project/Scripts/UI/PopupManager.cs`

```csharp
private void OnEnable()
{
    // El flag evita suscribirse dos veces al mismo canal.
    if (pool == null || isSubscribed) return;

    popupEvent.Requested += HandleRequested;
    isSubscribed = true;
}
```

```csharp
private void HandleRequested(PopupRequest request)
{
    PopupView popup = pool.Get();

    popup.Show(
        request.Title,
        request.Message,
        () => Close(popup, request, request.AcceptCommand),
        () => Close(popup, request, request.CancelCommand),
        request.IsModal);
}
```

`OnDisable` realiza la desuscripción, de modo que el manager no recibe solicitudes cuando está desactivado.

**Notificación del cierre** — `Assets/_Project/Scripts/Gameplay/GameManager.cs`

El mismo canal publica `Closed`. Al cerrarse una ventana, `PopupManager` llama a `NotifyClosed`; `GameManager`, suscripto a `Requested` y `Closed`, pausa el juego al abrirse un modal y lo reanuda al cerrarse el último:

```csharp
private void HandlePopupClosed(PopupRequest request)
{
    if (request == null ||
        !request.IsModal ||
        activeModalPopups == 0)
    {
        return;
    }

    activeModalPopups--;

    // Reanuda cuando se cierra el último modal.
    if (activeModalPopups == 0)
        Time.timeScale = timeScaleBeforeModal;
}
```

## Command

El patrón Command encapsula las acciones asociadas a los botones de los pop-ups. La interfaz `ICommand` define una operación común, `Execute()`, que implementan comandos concretos como `ReloadSceneCommand`, `QuitApplicationCommand` y `GoToMainMenuCommand`. Cada `PopupRequest` trae un comando para Aceptar y otro para Cancelar (que puede ser `null`, en cuyo caso Cancelar solo cierra la ventana). Gracias a esto, la misma ventana sirve para reiniciar, volver al menú o salir sin contener lógica específica de cada caso, y los comandos pueden cambiar sin modificar `PopupView` ni `PopupManager`.

**Contrato común** — `Assets/_Project/Scripts/Commands/ICommand.cs`

```csharp
public interface ICommand
{
    void Execute();
}
```

**Comando concreto** — `Assets/_Project/Scripts/Commands/GoToMainMenuCommand.cs`

```csharp
public class GoToMainMenuCommand : ICommand
{
    private readonly UIManager uiManager;

    // ... constructor: recibe y guarda el UIManager

    public void Execute()
    {
        if (uiManager == null) return;

        uiManager.GoToMainMenu();
    }
}
```

**La solicitud transporta los comandos** — `Assets/_Project/Scripts/Events/PopupRequest.cs`

```csharp
public PopupRequest(string title, string message, ICommand acceptCommand, ICommand cancelCommand = null, bool isModal = true)
{
    Title = title;
    Message = message;
    AcceptCommand = acceptCommand;
    CancelCommand = cancelCommand;
    IsModal = isModal;
}
```

`PopupManager` ejecuta el comando elegido con `command?.Execute()` al cerrar la ventana (ver el método `Close` en la sección siguiente).

## Object Pool

`PopupManager` administra un `ObjectPool<PopupView>` de `UnityEngine.Pool`. Al recibir una solicitud obtiene una instancia con `pool.Get()`; al cerrarse, la devuelve con `pool.Release()` en lugar de destruirla. Así se evita instanciar y destruir la misma ventana en cada solicitud. Antes de reutilizarla, `Hide()` limpia los listeners de los botones, de modo que una instancia reciclada no ejecuta los comandos de la solicitud anterior.

**Creación del pool** — `Assets/_Project/Scripts/UI/PopupManager.cs`

```csharp
pool = new ObjectPool<PopupView>(
    createFunc: CreatePopup,
    actionOnGet: popup => popup.transform.SetAsLastSibling(),
    actionOnRelease: popup => popup.Hide(),
    actionOnDestroy: DestroyPopup,
    collectionCheck: true); // Detecta si se devuelve dos veces la misma instancia.
```

**Devolución de la instancia** — `Assets/_Project/Scripts/UI/PopupManager.cs`

```csharp
private void Close(
    PopupView popup,
    PopupRequest request,
    ICommand command)
{
    // Oculta y devuelve la instancia al pool.
    pool.Release(popup);

    // Notifica el cierre para restaurar el estado del juego.
    popupEvent.NotifyClosed(request);

    // Ejecuta el comando después de restaurar el estado.
    command?.Execute();
}
```

La instancia se obtiene en `HandleRequested` con `pool.Get()` (fragmento de la sección Observer). El pool se precarga con `prewarmCount` (1 por defecto) para no instanciar en el primer pop-up. En la práctica se abre una ventana a la vez, así que una instancia alcanza; si se pidiera otro pop-up mientras hay uno abierto, el pool crearía una adicional. El beneficio de rendimiento depende de la frecuencia de uso: con pocos pop-ups por partida es modesto, y el principal valor es mantener una única ventana configurable y reutilizable.

## Reutilización del servicio

El mismo servicio (mismo `PopupEvent.asset`, mismo prefab `Popup_Base`) atiende dos casos reales. Cada escena que lo necesita, `MainMenu` y `Gameplay`, tiene su propio `PopupManager`. Solo cambian el mensaje y los comandos de la solicitud.

**Derrota por caída** — `Assets/_Project/Scripts/Gameplay/GameManager.cs` (`HandlePlayerDeath`)

```csharp
popupEvent.Raise(new PopupRequest(
    "Fin de la partida",
    "¿Reintentar?",
    new ReloadSceneCommand(uiManager),
    new GoToMainMenuCommand(uiManager),
    isModal: true));
```

**Confirmación de salida** — `Assets/_Project/Scripts/UI/ExitConfirmationRequester.cs` (`RequestExit`)

```csharp
popupEvent.Raise(new PopupRequest(
    "Confirmar salida",
    "¿Seguro que quieres salir?",
    new QuitApplicationCommand(uiManager),
    cancelCommand: null,
    isModal: true));
```

## Validación funcional

Se verificó manualmente en el Editor de Unity (6000.3.17f1) el flujo de los dos usos del servicio: la confirmación de salida desde `MainMenu` y el pop-up de fin de partida por caída en `Gameplay`. Se comprobó que la ventana se reutiliza entre solicitudes, que el fondo modal bloquea los clics sobre la interfaz mientras permanece abierta (en `Gameplay`, además, `GameManager` pausa la partida hasta que se cierra) y que cada botón ejecuta la acción definida en su solicitud.

![Pop-up «Fin de la partida» abierto durante el gameplay](https://drive.google.com/uc?export=view&id=1V4SVvFYJqhjHlJFwbhHZvqck5mjzt6xo)