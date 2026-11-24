using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    const string FullscreenKey = "Fullscreen";

    void Awake()
    {
        // load saved Fullscreen options
        if (PlayerPrefs.HasKey(FullscreenKey))
            ApplyFullscreen(PlayerPrefs.GetInt(FullscreenKey) == 1);
    }

    public void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' not found ");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    //Fullscreen
    public void SetFullscreen(bool value)
    {
        ApplyFullscreen(value);
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleFullscreen()
    {
        bool current = PlayerPrefs.HasKey(FullscreenKey)
            ? PlayerPrefs.GetInt(FullscreenKey) == 1
            : Screen.fullScreen;

        SetFullscreen(!current);
    }

    static void ApplyFullscreen(bool value)
    {
        Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    //Exit Game
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
