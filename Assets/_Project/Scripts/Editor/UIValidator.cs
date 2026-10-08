using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Revisa que la UI de todas las escenas del Build sea responsiva.
/// Se ejecuta desde Tools > Grupo1 > Validar UI y reporta en la consola:
///  - Canvas sin CanvasScaler en "Scale With Screen Size" 1920x1080
///    con Match 0.5 (convención del proyecto: así la UI no se achica
///    demasiado ni en horizontal ni en vertical).
///  - Elementos que se salen de su contenedor en 16:9 (1920x1080) o
///    en 9:16 (1080x1920), calculando su rect a partir de anchors,
///    pivot y offsets, igual que lo hace Unity.
/// Los hijos de Layout Groups y del contenido de un ScrollRect se
/// saltean: su posición la maneja el layout o el scroll.
/// </summary>
public static class UIValidator
{
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
    private const float MatchWidthOrHeight = 0.5f;

    private static readonly Vector2[] TestResolutions =
    {
        new Vector2(1920f, 1080f), // 16:9
        new Vector2(1080f, 1920f), // 9:16
    };

    // Margen en unidades de canvas para no reportar diferencias de redondeo.
    private const float Tolerance = 1f;

    [MenuItem("Tools/Grupo1/Validar UI")]
    public static void ValidateFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string previousScene = EditorSceneManager.GetActiveScene().path;
        int issues = ValidateAllScenes();

        if (!string.IsNullOrEmpty(previousScene))
            EditorSceneManager.OpenScene(previousScene);

        if (issues == 0)
            Debug.Log("[UIValidator] OK: la UI de todas las escenas entra en 16:9 y 9:16.");
        else
            Debug.LogWarning($"[UIValidator] Se encontraron {issues} problema(s). Ver los mensajes anteriores.");
    }

    /// <summary>Para correrlo por línea de comandos (-executeMethod).</summary>
    public static void ValidateBatch()
    {
        int issues = ValidateAllScenes();
        Debug.Log($"[UIValidator] Problemas encontrados: {issues}");
    }

    private static int ValidateAllScenes()
    {
        int issues = 0;

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;

            var scene = EditorSceneManager.OpenScene(buildScene.path);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (!canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) continue;
                    issues += ValidateCanvas(scene.name, canvas);
                }
            }
        }

        return issues;
    }

    private static int ValidateCanvas(string sceneName, Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null
            || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
            || scaler.referenceResolution != ReferenceResolution
            || !Mathf.Approximately(scaler.matchWidthOrHeight, MatchWidthOrHeight))
        {
            Debug.LogWarning($"[UIValidator] {sceneName}/{canvas.name}: el CanvasScaler tiene que estar en Scale With Screen Size con 1920x1080 y Match 0.5.", canvas);
            return 1;
        }

        int issues = 0;
        var reported = new HashSet<RectTransform>();

        foreach (Vector2 screen in TestResolutions)
        {
            Vector2 canvasSize = GetCanvasSize(screen, scaler);
            foreach (RectTransform child in canvas.transform)
                issues += ValidateRect(sceneName, child, canvasSize, screen, reported);
        }

        return issues;
    }

    /// <summary>
    /// Tamaño lógico del canvas para una resolución de pantalla, usando la
    /// misma fórmula que CanvasScaler en modo Match Width Or Height.
    /// </summary>
    private static Vector2 GetCanvasSize(Vector2 screen, CanvasScaler scaler)
    {
        float logWidth = Mathf.Log(screen.x / scaler.referenceResolution.x, 2f);
        float logHeight = Mathf.Log(screen.y / scaler.referenceResolution.y, 2f);
        float scale = Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight));
        return screen / scale;
    }

    private static int ValidateRect(string sceneName, RectTransform rect, Vector2 parentSize,
        Vector2 screen, HashSet<RectTransform> reported)
    {
        if (!rect.gameObject.activeSelf) return 0;

        // Mismo cálculo que hace Unity: esquinas = anchor * tamaño del padre + offset.
        Vector2 min = Vector2.Scale(rect.anchorMin, parentSize) + rect.offsetMin;
        Vector2 max = Vector2.Scale(rect.anchorMax, parentSize) + rect.offsetMax;

        int issues = 0;
        bool outside = min.x < -Tolerance || min.y < -Tolerance
            || max.x > parentSize.x + Tolerance || max.y > parentSize.y + Tolerance;

        if (outside && reported.Add(rect))
        {
            Debug.LogWarning($"[UIValidator] {sceneName}: \"{GetPath(rect)}\" se sale de su contenedor en {screen.x}x{screen.y} " +
                             $"(rect {min} → {max}, contenedor {parentSize}).", rect);
            issues++;
        }

        // Los hijos de un Layout Group o del contenido de un ScrollRect los
        // posiciona el layout/scroll, así que no se validan uno por uno.
        if (rect.GetComponent<LayoutGroup>() != null || IsScrollContent(rect))
            return issues;

        Vector2 size = max - min;
        foreach (Transform child in rect)
        {
            if (child is RectTransform childRect)
                issues += ValidateRect(sceneName, childRect, size, screen, reported);
        }

        return issues;
    }

    private static bool IsScrollContent(RectTransform rect)
    {
        ScrollRect scroll = rect.GetComponentInParent<ScrollRect>(true);
        return scroll != null && scroll.content == rect;
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null && t.parent.GetComponent<Canvas>() == null)
        {
            t = t.parent;
            path = $"{t.name}/{path}";
        }
        return path;
    }
}
