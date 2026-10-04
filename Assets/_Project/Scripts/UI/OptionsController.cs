using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla las opciones de la escena Options: volumen general y
/// pantalla completa. Los valores se guardan en PlayerPrefs para que
/// se mantengan entre sesiones.
/// Los controles llaman a SetVolume / SetFullscreen desde el Inspector
/// (Slider/Toggle > On Value Changed).
/// </summary>
public class OptionsController : MonoBehaviour
{
    // Claves de PlayerPrefs. Si se cambian, se pierden los valores guardados.
    private const string VolumeKey = "Options.Volume";
    private const string FullscreenKey = "Options.Fullscreen";

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    /// <summary>
    /// Aplica las opciones guardadas al iniciar el juego, sin esperar
    /// a que el jugador entre a la escena Options.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplySavedSettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
    }

    private void Start()
    {
        // Se usa SetValueWithoutNotify para reflejar el estado actual
        // en la UI sin volver a disparar los eventos OnValueChanged.
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
