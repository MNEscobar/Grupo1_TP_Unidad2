using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centraliza la navegación entre las escenas del proyecto.
/// Los botones de la UI llaman a estos métodos públicos desde el Inspector
/// (Button > OnClick) en vez de tener lógica de carga de escena repetida
/// en cada pantalla.
/// </summary>
public class UIManager : MonoBehaviour
{
    // Nombres de las escenas, tal como están guardadas en
    // Assets/_Project/Scenes/ y listadas en Build Settings.
    // Si se renombra una escena, actualizar acá también.
    private const string SceneMainMenu = "MainMenu";
    private const string SceneGameplay = "Gameplay";
    private const string SceneOptions = "Options";
    private const string SceneCredits = "Credits";

    public void GoToMainMenu() => LoadScene(SceneMainMenu);

    public void GoToGameplay() => LoadScene(SceneGameplay);

    public void GoToOptions() => LoadScene(SceneOptions);

    public void GoToCredits() => LoadScene(SceneCredits);

    /// <summary>
    /// Pensado para el botón de salir dentro de una escena secundaria
    /// (por ejemplo Gameplay): vuelve al menú principal.
    /// </summary>
    public void QuitToMainMenu() => LoadScene(SceneMainMenu);

    /// <summary>
    /// Cierra la aplicación. En el Editor detiene el Play Mode
    /// en vez de cerrar Unity.
    /// </summary>
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Carga una escena por nombre, validando antes que exista en
    /// Build Settings para no romper la navegación por un typo.
    /// </summary>
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[UIManager] Nombre de escena vacío o nulo.");
            return;
        }

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[UIManager] La escena \"{sceneName}\" no coincide con el nombre de ninguna escena en Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}