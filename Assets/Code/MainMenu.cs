using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [SerializeField] private string sceneToLoad = "Game";

    [Header("Canvas/Pantallas")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject mapSelectorCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameObject controlsCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private GameObject tscreen;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    private const string PrefMaster = "MasterVolume";
    private const string PrefMusic = "MusicVolume";

    private const float MinDb = -80f;
    private const float MaxDb = 0f;

    private void Awake()
    {
        InitializeAudioSettings();
        InitializeUI();
    }

    private void Start()
    {
        _ = MapManager.Instance;
    }

    private void OnDestroy()
    {
        UnregisterAudioListeners();
    }

    #region Inicialización

    private void InitializeAudioSettings()
    {
        float master = PlayerPrefs.GetFloat(PrefMaster, 0.75f);
        float music = PlayerPrefs.GetFloat(PrefMusic, 0.75f);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = master;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = music;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        ApplyMasterVolume(master);
        ApplyMusicVolume(music);
    }

    private void InitializeUI()
    {
        if (mapSelectorCanvas != null)
        {
            mapSelectorCanvas.SetActive(false);
        }

        if (optionsCanvas != null)
        {
            optionsCanvas.SetActive(false);
        }

        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(false);
        }

        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(false);
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }

    private void UnregisterAudioListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
    }

    #endregion

    #region Navegación del Menú

    public void OnPlayButton()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("[MainMenu] 'sceneToLoad' no está configurado.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"[MainMenu] La escena '{sceneToLoad}' no está en Build Settings.");
            Debug.LogError("Añádela en: File > Build Settings > Scenes In Build");
            return;
        }

        ShowMapSelector();
    }

    private void ShowMapSelector()
    {
        if (mapSelectorCanvas == null)
        {
            Debug.LogError("[MainMenu] 'mapSelectorCanvas' no está asignado.");
            return;
        }

        Debug.Log("[MainMenu] Mostrando selector de mapas...");

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        mapSelectorCanvas.SetActive(true);
    }

    public void OnBackToMainMenu()
    {
        if (mapSelectorCanvas != null)
        {
            mapSelectorCanvas.SetActive(false);
        }

        if (optionsCanvas != null)
        {
            optionsCanvas.SetActive(false);
        }

        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(false);
        }

        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(false);
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }

    public void OnOptionsButton()
    {
        if (optionsCanvas == null)
        {
            Debug.LogError("[MainMenu] 'optionsCanvas' no está asignado.");
            return;
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        optionsCanvas.SetActive(true);
    }

    public void OnControlsButton()
    {
        if (controlsCanvas == null)
        {
            Debug.LogError("[MainMenu] 'controlsCanvas' no está asignado.");
            return;
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        controlsCanvas.SetActive(true);
    }

    public void OnCreditsButton()
    {
        if (creditsCanvas == null)
        {
            Debug.LogError("[MainMenu] 'creditsCanvas' no está asignado.");
            return;
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        creditsCanvas.SetActive(true);
    }

    #endregion

    #region Selección de Mapa

    public void OnMapSelected(string mapId)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("[MainMenu] mapId vacío recibido.");
            return;
        }

        Debug.Log($"[MainMenu] ✓ Mapa seleccionado: '{mapId}'");

        MapManager.SelectedMap = mapId;

        StartCoroutine(LoadGameSceneCoroutine());
    }

    private IEnumerator LoadGameSceneCoroutine()
    {
        yield return null;

        Debug.Log($"[MainMenu] Cargando escena: '{sceneToLoad}'...");

        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    #endregion

    #region Gestión de Audio

    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(PrefMaster, value);
        PlayerPrefs.Save();
        ApplyMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(PrefMusic, value);
        PlayerPrefs.Save();
        ApplyMusicVolume(value);
    }

    private void ApplyMasterVolume(float normalizedValue)
    {
        if (audioMixer == null) return;

        float db = Mathf.Lerp(MinDb, MaxDb, Mathf.Clamp01(normalizedValue));
        audioMixer.SetFloat("MasterVolume", db);
    }

    private void ApplyMusicVolume(float normalizedValue)
    {
        if (audioMixer == null) return;

        float db = Mathf.Lerp(MinDb, MaxDb, Mathf.Clamp01(normalizedValue));
        audioMixer.SetFloat("MusicVolume", db);
    }

    #endregion

    #region Otros Botones

    public void OnQuitButton()
    {
        Debug.Log("[MainMenu] Saliendo de la aplicación...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}