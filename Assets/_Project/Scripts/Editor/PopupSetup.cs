using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Genera el prefab reutilizable Popup_Base: fondo bloqueante a pantalla
/// completa y una ventana centrada con título, mensaje y botones
/// Aceptar/Cancelar. No tiene lógica de reinicio ni de salida: los textos
/// y las acciones los configura el PopupManager por código a través de
/// <see cref="PopupView"/>.
///
/// Se ejecuta desde Tools > Grupo1 > Crear Popup_Base. Se puede correr más
/// de una vez: el prefab se vuelve a generar sobre el mismo path, así que
/// conserva su GUID y no se rompen las referencias.
///
/// Tools > Grupo1 > Setup PopupManager agrega el PopupManager a las escenas
/// que muestran pop-ups (MainMenu y Gameplay), dentro de [Managers].
/// </summary>
public static class PopupSetup
{
    private const string UIPrefabsFolder = "Assets/_Project/Prefabs/UI";
    private const string PopupPrefabPath = UIPrefabsFolder + "/Popup_Base.prefab";
    private const string ButtonBasePrefabPath = UIPrefabsFolder + "/Button_Base.prefab";
    private const string ButtonCancelPrefabPath = UIPrefabsFolder + "/Variants/Button_Cancel.prefab";
    private const string PopupEventPath = "Assets/_Project/ScriptableObjects/PopupEvent.asset";

    private static readonly string[] PopupScenePaths =
    {
        "Assets/_Project/Scenes/MainMenu.unity",
        "Assets/_Project/Scenes/Gameplay.unity",
    };

    private const string PopupManagerName = "PopupManager";

    // 900 de ancho entra tanto en 16:9 (canvas ~1920x1080) como en 9:16
    // (canvas ~1080x1920) con el CanvasScaler del proyecto (Match 0.5).
    private const float WindowWidth = 900f;
    private const float ButtonHeight = 64f;

    private static readonly Color BlockerColor = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color WindowColor = new Color(0.12f, 0.12f, 0.16f, 0.95f);

    [MenuItem("Tools/Grupo1/Crear Popup_Base")]
    public static void CreatePopupBase()
    {
        GameObject buttonBase = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonBasePrefabPath);
        GameObject buttonCancel = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonCancelPrefabPath);
        if (buttonBase == null || buttonCancel == null)
        {
            Debug.LogError("[PopupSetup] No se encontraron Button_Base o Button_Cancel en Prefabs/UI.");
            return;
        }

        // Raíz: fondo estirado a toda la pantalla. Es el "blocker" que
        // impide hacer click en lo que queda detrás de la ventana modal.
        GameObject root = new GameObject("Popup_Base", typeof(RectTransform), typeof(Image), typeof(PopupView));
        Stretch((RectTransform)root.transform);
        Image blocker = root.GetComponent<Image>();
        blocker.color = BlockerColor;
        blocker.raycastTarget = true;

        GameObject window = CreateWindow(root.transform);
        TMP_Text title = CreateText("Text_Title", window.transform, "Título", 48f, FontStyles.Bold);
        TMP_Text message = CreateText("Text_Message", window.transform, "Mensaje", 32f, FontStyles.Normal);
        Transform buttons = CreateButtonsContainer(window.transform);

        GameObject accept = (GameObject)PrefabUtility.InstantiatePrefab(buttonBase, buttons);
        accept.name = "Button_Accept";
        SetButtonText(accept, "Aceptar");

        GameObject cancel = (GameObject)PrefabUtility.InstantiatePrefab(buttonCancel, buttons);
        cancel.name = "Button_Cancel";

        PopupView view = root.GetComponent<PopupView>();
        SetRef(view, "titleText", title);
        SetRef(view, "messageText", message);
        SetRef(view, "acceptButton", accept.GetComponent<Button>());
        SetRef(view, "cancelButton", cancel.GetComponent<Button>());
        SetRef(view, "blocker", blocker);

        // El PopupManager lo activa al mostrarlo.
        root.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(root, PopupPrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PopupPrefabPath);
        Debug.Log($"[PopupSetup] Listo: prefab generado en {PopupPrefabPath}.");
    }

    /// <summary>
    /// Crea (o vuelve a crear) el PopupManager de cada escena y le asigna
    /// el canal de eventos, el prefab y el Canvas donde instanciar.
    /// </summary>
    [MenuItem("Tools/Grupo1/Setup PopupManager")]
    public static void SetupPopupManager()
    {
        // Si hay cambios sin guardar en la escena abierta, se le pregunta
        // al usuario antes de cambiar de escena.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        foreach (string scenePath in PopupScenePaths)
            SetupScene(scenePath);

        Debug.Log("[PopupSetup] Listo: PopupManager agregado en MainMenu y Gameplay.");
    }

    private static void SetupScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Los assets se cargan después de abrir la escena: OpenScene descarga
        // los assets que no usa nadie y una referencia cargada antes queda
        // inválida (se guardaría como None en el Inspector).
        PopupEventSO popupEvent = AssetDatabase.LoadAssetAtPath<PopupEventSO>(PopupEventPath);
        GameObject popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PopupPrefabPath);
        if (popupEvent == null || popupPrefab == null)
        {
            Debug.LogError("[PopupSetup] Falta PopupEvent.asset o Popup_Base.prefab (correr antes Crear Popup_Base).");
            return;
        }

        Transform canvas = FindInScene("Canvas");
        if (canvas == null)
        {
            Debug.LogError($"[PopupSetup] No se encontró el Canvas en {scenePath}.");
            return;
        }

        DestroyIfExists(PopupManagerName);

        GameObject managerGO = new GameObject(PopupManagerName);
        Transform managers = FindInScene("[Managers]");
        if (managers != null) managerGO.transform.SetParent(managers, false);

        PopupManager manager = managerGO.AddComponent<PopupManager>();
        SetRef(manager, "popupEvent", popupEvent);
        SetRef(manager, "popupPrefab", popupPrefab.GetComponent<PopupView>());
        SetRef(manager, "popupParent", canvas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ------------------------------------------------------------------
    // Jerarquía
    // ------------------------------------------------------------------

    /// <summary>
    /// Ventana centrada de ancho fijo. El alto lo calcula el
    /// ContentSizeFitter según el largo del mensaje.
    /// </summary>
    private static GameObject CreateWindow(Transform parent)
    {
        GameObject window = new GameObject("Panel_Window", typeof(RectTransform), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        window.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)window.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(WindowWidth, 0f);

        window.GetComponent<Image>().color = WindowColor;

        VerticalLayoutGroup layout = window.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 40, 40);
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = window.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return window;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>
    /// Fila de botones. Los botones mantienen su tamaño de Button_Base
    /// (320x64); el layout solo los centra y los separa.
    /// </summary>
    private static Transform CreateButtonsContainer(Transform parent)
    {
        GameObject container = new GameObject("Container_Buttons", typeof(RectTransform),
            typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        container.transform.SetParent(parent, false);

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 40f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        container.GetComponent<LayoutElement>().preferredHeight = ButtonHeight;
        return container.transform;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Busca un objeto por nombre en la escena activa, incluyendo los
    /// inactivos (GameObject.Find solo encuentra los activos).
    /// </summary>
    private static Transform FindInScene(string name)
    {
        var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t;
            }
        }
        return null;
    }

    private static void DestroyIfExists(string name)
    {
        Transform t;
        while ((t = FindInScene(name)) != null)
            Object.DestroyImmediate(t.gameObject);
    }

    private static void SetButtonText(GameObject button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        var so = new SerializedObject(label);
        so.FindProperty("m_text").stringValue = text;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Los campos de PopupView son privados con [SerializeField], así que
    // se asignan por SerializedObject (igual que en el Inspector).
    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
