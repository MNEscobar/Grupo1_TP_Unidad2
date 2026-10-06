using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Arma automáticamente el contenido del gameplay básico:
/// materiales, prefabs (Player, Collectible + variante Gold, Button_Quit)
/// y los objetos de las escenas Gameplay, Bootstrap y MainMenu.
///
/// Se ejecuta desde Tools > Grupo1 > Setup Gameplay. Se puede correr más
/// de una vez: los assets que ya existen se reutilizan (para no cambiar
/// sus GUID y romper referencias) y los objetos de escena que crea este
/// script se borran y se vuelven a generar.
/// </summary>
public static class GameplaySetup
{
    private const string MaterialsFolder = "Assets/_Project/Materials";
    private const string GameplayPrefabsFolder = "Assets/_Project/Prefabs/Gameplay";
    private const string GameplayVariantsFolder = "Assets/_Project/Prefabs/Gameplay/Variants";
    private const string UIVariantsFolder = "Assets/_Project/Prefabs/UI/Variants";

    private const string PlayerPrefabPath = GameplayPrefabsFolder + "/Player.prefab";
    private const string CollectiblePrefabPath = GameplayPrefabsFolder + "/Collectible.prefab";
    private const string CollectibleGoldPrefabPath = GameplayVariantsFolder + "/Collectible_Gold.prefab";
    private const string ButtonBasePrefabPath = "Assets/_Project/Prefabs/UI/Button_Base.prefab";
    private const string ButtonBackPrefabPath = UIVariantsFolder + "/Button_Back.prefab";
    private const string ButtonQuitPrefabPath = UIVariantsFolder + "/Button_Quit.prefab";

    private const string GameplayScenePath = "Assets/_Project/Scenes/Gameplay.unity";
    private const string BootstrapScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
    private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";

    private const string UrpLitShader = "Universal Render Pipeline/Lit";
    private const string PlayerTag = "Player";

    // Posiciones de los recolectables sobre el suelo (Plane 30x30).
    // Se evitan el Landmark del centro y los pilares en las esquinas.
    private static readonly Vector3[] CoinPositions =
    {
        new Vector3(4f, 0.8f, 3f),
        new Vector3(-4f, 0.8f, 3f),
        new Vector3(4f, 0.8f, -3f),
        new Vector3(-4f, 0.8f, -3f),
        new Vector3(0f, 0.8f, 7f),
        new Vector3(0f, 0.8f, -9f),
    };

    private static readonly Vector3[] GoldCoinPositions =
    {
        new Vector3(-11f, 0.8f, 5f),
        new Vector3(11f, 0.8f, -5f),
    };

    private static readonly Vector3 PlayerSpawn = new Vector3(0f, 1f, -5f);

    [MenuItem("Tools/Grupo1/Setup Gameplay")]
    public static void Run()
    {
        // Si hay cambios sin guardar en la escena abierta, se le pregunta
        // al usuario antes de cambiar de escena.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        CreateFolders();

        Material playerMat = GetOrCreateMaterial("M_Player", new Color(0.2f, 0.45f, 0.95f), 0f);
        Material coinMat = GetOrCreateMaterial("M_Collectible", new Color(0.95f, 0.85f, 0.2f), 0.2f);
        Material goldMat = GetOrCreateMaterial("M_Collectible_Gold", new Color(1f, 0.6f, 0.05f), 0.9f);

        GameObject playerPrefab = GetOrCreatePlayerPrefab(playerMat);
        GameObject coinPrefab = GetOrCreateCollectiblePrefab(coinMat);
        GameObject goldPrefab = GetOrCreateGoldVariant(coinPrefab, goldMat);
        GameObject quitPrefab = GetOrCreateQuitButtonVariant();

        SetupGameplayScene(playerPrefab, coinPrefab, goldPrefab);
        SetupBootstrapScene();
        SetupMainMenuScene(quitPrefab);

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(GameplayScenePath);
        Debug.Log("[GameplaySetup] Listo: prefabs creados y escenas Gameplay, Bootstrap y MainMenu actualizadas.");
    }

    // ------------------------------------------------------------------
    // Assets
    // ------------------------------------------------------------------

    private static void CreateFolders()
    {
        EnsureFolder(GameplayPrefabsFolder);
        EnsureFolder(GameplayVariantsFolder);
        EnsureFolder(UIVariantsFolder);
        EnsureFolder(MaterialsFolder);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }

    private static Material GetOrCreateMaterial(string name, Color color, float metallic)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Material material = new Material(Shader.Find(UrpLitShader));
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject GetOrCreatePlayerPrefab(Material material)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (existing != null) return existing;

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = PlayerTag;
        player.GetComponent<MeshRenderer>().sharedMaterial = material;
        player.AddComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        player.AddComponent<PlayerController>();

        return SaveAndDestroy(player, PlayerPrefabPath);
    }

    private static GameObject GetOrCreateCollectiblePrefab(Material material)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(CollectiblePrefabPath);
        if (existing != null) return existing;

        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Collectible";
        coin.transform.localScale = Vector3.one * 0.6f;
        coin.GetComponent<MeshRenderer>().sharedMaterial = material;
        coin.GetComponent<Collider>().isTrigger = true;
        coin.AddComponent<Collectible>();

        return SaveAndDestroy(coin, CollectiblePrefabPath);
    }

    /// <summary>
    /// La variante Gold hereda todo del Collectible base y solo
    /// sobreescribe puntos, escala y material.
    /// </summary>
    private static GameObject GetOrCreateGoldVariant(GameObject basePrefab, Material material)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(CollectibleGoldPrefabPath);
        if (existing != null) return existing;

        GameObject gold = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        gold.name = "Collectible_Gold";
        gold.transform.localScale = Vector3.one * 0.9f;
        gold.GetComponent<MeshRenderer>().sharedMaterial = material;
        SetInt(gold.GetComponent<Collectible>(), "points", 5);
        SetFloat(gold.GetComponent<Collectible>(), "rotationSpeed", 180f);

        // Guardar una instancia de prefab como asset nuevo genera una Prefab Variant.
        return SaveAndDestroy(gold, CollectibleGoldPrefabPath);
    }

    private static GameObject GetOrCreateQuitButtonVariant()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonQuitPrefabPath);
        if (existing != null) return existing;

        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonBasePrefabPath);
        GameObject quit = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        quit.name = "Button_Quit";
        SetButtonText(quit, "Salir");

        return SaveAndDestroy(quit, ButtonQuitPrefabPath);
    }

    private static GameObject SaveAndDestroy(GameObject instance, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    // ------------------------------------------------------------------
    // Escenas
    // ------------------------------------------------------------------

    private static void SetupGameplayScene(GameObject playerPrefab, GameObject coinPrefab, GameObject goldPrefab)
    {
        var scene = EditorSceneManager.OpenScene(GameplayScenePath);

        DestroyIfExists("Player");
        DestroyIfExists("Collectibles");
        DestroyIfExists("ScoreManager");
        DestroyIfExists("Text_Score");
        DestroyIfExists("Panel_Win");

        // Jugador
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = PlayerSpawn;

        // Recolectables agrupados bajo un mismo padre para mantener la jerarquía prolija.
        Transform collectibles = new GameObject("Collectibles").transform;
        Transform environment = FindInScene("Environment");
        if (environment != null) collectibles.SetParent(environment, false);

        for (int i = 0; i < CoinPositions.Length; i++)
            InstantiateAt(coinPrefab, CoinPositions[i], collectibles, $"Collectible_{i + 1}");

        for (int i = 0; i < GoldCoinPositions.Length; i++)
            InstantiateAt(goldPrefab, GoldCoinPositions[i], collectibles, $"Collectible_Gold_{i + 1}");

        // ScoreManager dentro de [Managers], junto al UIManager.
        GameObject scoreGO = new GameObject("ScoreManager");
        Transform managers = FindInScene("[Managers]");
        if (managers != null) scoreGO.transform.SetParent(managers, false);
        ScoreManager scoreManager = scoreGO.AddComponent<ScoreManager>();

        // HUD
        Transform canvas = FindInScene("Canvas");
        TMP_Text title = FindInScene("Text_Title").GetComponent<TMP_Text>();
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();

        TMP_Text scoreText = CreateScoreText(title, canvas);
        GameObject winPanel = CreateWinPanel(title, canvas, uiManager);

        GameplayHUD hud = canvas.GetComponent<GameplayHUD>();
        if (hud == null) hud = canvas.gameObject.AddComponent<GameplayHUD>();
        SetRef(hud, "scoreManager", scoreManager);
        SetRef(hud, "scoreText", scoreText);
        SetRef(hud, "winPanel", winPanel);

        // Cámara más alta e inclinada para ver toda la arena.
        Transform cam = FindInScene("Main Camera");
        if (cam != null)
        {
            cam.position = new Vector3(0f, 16f, -17f);
            cam.rotation = Quaternion.Euler(45f, 0f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupBootstrapScene()
    {
        var scene = EditorSceneManager.OpenScene(BootstrapScenePath);

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[GameplaySetup] No se encontró el UIManager en Bootstrap.");
            return;
        }

        SplashController splash = uiManager.GetComponent<SplashController>();
        if (splash == null) splash = uiManager.gameObject.AddComponent<SplashController>();
        SetRef(splash, "uiManager", uiManager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>
    /// Reemplaza el botón Salir (instancia directa de Button_Base) por una
    /// instancia de la variante Button_Quit, manteniendo su lugar en el
    /// Layout Group y su OnClick a QuitApplication.
    /// </summary>
    private static void SetupMainMenuScene(GameObject quitPrefab)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScenePath);

        Transform oldQuit = FindInScene("Button_Quit");
        if (oldQuit == null)
        {
            Debug.LogError("[GameplaySetup] No se encontró Button_Quit en MainMenu.");
            return;
        }

        if (PrefabUtility.GetCorrespondingObjectFromSource(oldQuit.gameObject) == quitPrefab)
            return; // Ya usa la variante.

        GameObject newQuit = (GameObject)PrefabUtility.InstantiatePrefab(quitPrefab, oldQuit.parent);
        newQuit.name = "Button_Quit";
        newQuit.transform.SetSiblingIndex(oldQuit.GetSiblingIndex());
        CopyRect((RectTransform)oldQuit, (RectTransform)newQuit.transform);

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        AddClick(newQuit, uiManager.QuitApplication);

        Object.DestroyImmediate(oldQuit.gameObject);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ------------------------------------------------------------------
    // UI del Gameplay
    // ------------------------------------------------------------------

    /// <summary>
    /// Duplica Text_Title para que el contador tenga la misma fuente y
    /// estilo. Queda anclado arriba a la izquierda, debajo del título.
    /// </summary>
    private static TMP_Text CreateScoreText(TMP_Text title, Transform canvas)
    {
        TMP_Text score = Object.Instantiate(title, canvas);
        score.name = "Text_Score";
        score.text = "Puntos: 0 / 0";
        score.fontSize = title.fontSize * 0.7f;

        RectTransform rect = score.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(40f, -130f);
        rect.sizeDelta = new Vector2(600f, 60f);
        return score;
    }

    /// <summary>
    /// Panel de victoria: fondo semitransparente estirado a toda la
    /// pantalla (anchors 0..1) y un contenedor centrado con un
    /// VerticalLayoutGroup para el mensaje y los botones.
    /// </summary>
    private static GameObject CreateWinPanel(TMP_Text title, Transform canvas, UIManager uiManager)
    {
        GameObject panel = new GameObject("Panel_Win", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas, false);
        Stretch((RectTransform)panel.transform);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        GameObject container = new GameObject("Container_Win", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(panel.transform, false);
        RectTransform containerRect = (RectTransform)container.transform;
        containerRect.anchorMin = containerRect.anchorMax = containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(700f, 400f);

        VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TMP_Text winText = Object.Instantiate(title, container.transform);
        winText.name = "Text_Win";
        winText.text = "¡Ganaste!";
        winText.alignment = TextAlignmentOptions.Center;
        winText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        winText.rectTransform.sizeDelta = new Vector2(700f, 100f);

        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonBasePrefabPath);
        GameObject restart = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab, container.transform);
        restart.name = "Button_Restart";
        SetButtonText(restart, "Reiniciar");
        AddClick(restart, uiManager.ReloadCurrentScene);

        GameObject backPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonBackPrefabPath);
        GameObject back = (GameObject)PrefabUtility.InstantiatePrefab(backPrefab, container.transform);
        back.name = "Button_MainMenu";
        AddClick(back, uiManager.GoToMainMenu);

        // El HUD lo oculta en Start; en el editor queda visible para poder editarlo.
        panel.transform.SetAsLastSibling();
        return panel;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void InstantiateAt(GameObject prefab, Vector3 position, Transform parent, string name)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.position = position;
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

    private static void AddClick(GameObject buttonGO, UnityAction action)
    {
        Button button = buttonGO.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);
        PrefabUtility.RecordPrefabInstancePropertyModifications(button);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CopyRect(RectTransform from, RectTransform to)
    {
        to.anchorMin = from.anchorMin;
        to.anchorMax = from.anchorMax;
        to.pivot = from.pivot;
        to.anchoredPosition = from.anchoredPosition;
        to.sizeDelta = from.sizeDelta;
    }

    // Los campos de los componentes son privados con [SerializeField],
    // así que se asignan por SerializedObject (igual que en el Inspector).
    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(Object target, string field, int value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string field, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
