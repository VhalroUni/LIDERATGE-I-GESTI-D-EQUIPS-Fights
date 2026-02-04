using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string sceneToLoad;

    [Header("Pantallas/UI")]
    [SerializeField] private GameObject controlsScreen; 

    public void OnPlayButton()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("[MainMenu] 'sceneToLoad' no está configurado en el inspector.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"[MainMenu] La escena '{sceneToLoad}' no está incluida en Build Settings (File > Build Settings > Scenes In Build).");
            return;
        }

        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    public void OnExitButton()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
    public void OnShowControlsButton()
    {
        if (controlsScreen == null)
        {
            Debug.LogError("[MainMenu] 'controlsScreen' no está asignado en el inspector.");
            return;
        }

        controlsScreen.SetActive(true);
    }
        public void OnHideControlsButton()
    {
        if (controlsScreen == null)
        {
            Debug.LogError("[MainMenu] 'controlsScreen' no está asignado en el inspector.");
            return;
        }
        controlsScreen.SetActive(false);
    }
}