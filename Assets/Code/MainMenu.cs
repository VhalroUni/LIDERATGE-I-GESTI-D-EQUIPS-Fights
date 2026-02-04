using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string sceneToLoad;

    [Header("Pantallas/UI")]
    [SerializeField] private GameObject Tscreen;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    // Claves de PlayerPrefs
    private const string PrefMaster = "pref_master_volume";
    private const string PrefMusic = "pref_music_volume";

    private const float MinDb = -80f;
    private const float MaxDb = 0f;

    private void Awake()
    {
        float master = PlayerPrefs.GetFloat(PrefMaster, 0.75f); // 0..1
        float music = PlayerPrefs.GetFloat(PrefMusic, 0.75f);   // 0..1

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

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }

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

    public void OnShowButton()
    {
        if (Tscreen == null)
        {
            Debug.LogError("[MainMenu] 'TScreen' no está asignado en el inspector.");
            return;
        }
        Tscreen.SetActive(true);
    }

    public void OnHideButton()
    {
        if (Tscreen == null)
        {
            Debug.LogError("[MainMenu] 'TScreen' no está asignado en el inspector.");
            return;
        }
        Tscreen.SetActive(false);
    }

    private void OnMasterVolumeChanged(float value01)
    {
        ApplyMasterVolume(value01);
        PlayerPrefs.SetFloat(PrefMaster, value01);
        PlayerPrefs.Save();
    }

    private void OnMusicVolumeChanged(float value01)
    {
        ApplyMusicVolume(value01);
        PlayerPrefs.SetFloat(PrefMusic, value01);
        PlayerPrefs.Save();
    }

    private void ApplyMasterVolume(float value01)
    {
        if (audioMixer == null) return;
        float db = Linear01ToDb(value01);
        audioMixer.SetFloat("MasterVolume", db);
    }

    private void ApplyMusicVolume(float value01)
    {
        if (audioMixer == null) return;
        float db = Linear01ToDb(value01);
        audioMixer.SetFloat("MusicVolume", db);
    }

    private float Linear01ToDb(float value01)
    {
        if (value01 <= 0.0001f) return MinDb;

        float db = Mathf.Log10(value01) * 20f;
        return Mathf.Clamp(db, MinDb, MaxDb);
    }
}