using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Escucha el canal <see cref="PopupEventSO"/> (Observer) y muestra el
/// prefab Popup_Base con los datos de cada <see cref="PopupRequest"/>.
/// Los botones ejecutan los <see cref="ICommand"/> que trae la solicitud,
/// así el manager no sabe si se está reiniciando, saliendo o confirmando
/// otra cosa (Command).
/// Las ventanas se reutilizan con un <see cref="ObjectPool{T}"/>: al
/// cerrarse vuelven al pool en vez de destruirse (Object Pool).
/// </summary>
public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupEventSO popupEvent;
    [SerializeField] private PopupView popupPrefab;

    // Donde se instancian los pop-ups. Normalmente el Canvas de la escena,
    // para que usen su CanvasScaler y queden por encima del resto de la UI.
    [SerializeField] private Transform popupParent;

    [Tooltip("Instancias que se crean al iniciar la escena para no instanciar en el primer pop-up.")]
    [SerializeField] private int prewarmCount = 1;

    private ObjectPool<PopupView> pool;
    private bool isSubscribed;
    private int testRequestCount;

    private void Awake()
    {
        if (popupEvent == null || popupPrefab == null || popupParent == null)
        {
            Debug.LogError($"[PopupManager] Faltan referencias en \"{name}\" (PopupEvent, Popup Prefab o Popup Parent).");
            enabled = false;
            return;
        }

        pool = new ObjectPool<PopupView>(
            createFunc: CreatePopup,
            actionOnGet: popup => popup.transform.SetAsLastSibling(),
            actionOnRelease: popup => popup.Hide(),
            actionOnDestroy: DestroyPopup,
            collectionCheck: true); // Detecta si se devuelve dos veces la misma instancia.

        Prewarm();
    }

    private void OnEnable()
    {
        // El flag evita suscribirse dos veces al mismo canal.
        if (pool == null || isSubscribed) return;

        popupEvent.Requested += HandleRequested;
        isSubscribed = true;
    }

    private void OnDisable()
    {
        if (!isSubscribed) return;

        popupEvent.Requested -= HandleRequested;
        isSubscribed = false;
    }

    private void OnDestroy()
    {
        pool?.Dispose();
    }

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

    /// <summary>
    /// Devuelve la ventana al pool, notifica su cierre para restaurar el estado del juego 
    /// y finalmente ejecuta el comando asociado. 
    /// Ejecutar el comando al final asegura que el pop-up ya esté liberado en caso de un cambio de escena. 
    /// Un comando null (por ejemplo, Cancelar sin acción) simplemente cerrará el pop-up.
    /// </summary>
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

    private PopupView CreatePopup()
    {
        PopupView popup = Instantiate(popupPrefab, popupParent, false);
        popup.gameObject.SetActive(false);
        return popup;
    }

    private void DestroyPopup(PopupView popup)
    {
        // Al cambiar de escena la instancia puede haberse destruido junto con el Canvas.
        if (popup != null)
            Destroy(popup.gameObject);
    }

    private void Prewarm()
    {
        var created = new PopupView[Mathf.Max(0, prewarmCount)];
        for (int i = 0; i < created.Length; i++)
            created[i] = pool.Get();

        foreach (PopupView popup in created)
            pool.Release(popup);
    }

    // ------------------------------------------------------------------
    // Prueba manual (click derecho sobre el componente en Play Mode)
    // ------------------------------------------------------------------

    /// <summary>
    /// Emite una solicitud de prueba por el canal, igual que lo haría
    /// cualquier emisor. Abriéndola varias veces se puede ver en la
    /// jerarquía que se reutiliza la misma instancia.
    /// </summary>
    [ContextMenu("Probar pop-up")]
    private void RaiseTestRequest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[PopupManager] La prueba solo funciona en Play Mode.");
            return;
        }

        testRequestCount++;
        popupEvent.Raise(new PopupRequest(
            $"Prueba nº {testRequestCount}",
            "Aceptar y Cancelar escriben en la consola y cierran el pop-up.",
            new LogCommand($"Aceptar en la prueba nº {testRequestCount}"),
            new LogCommand($"Cancelar en la prueba nº {testRequestCount}")));
    }

    /// <summary>Comando de prueba: solo deja un mensaje en la consola.</summary>
    private class LogCommand : ICommand
    {
        private readonly string message;

        public LogCommand(string message) => this.message = message;

        public void Execute() => Debug.Log($"[PopupManager] {message}");
    }
}
