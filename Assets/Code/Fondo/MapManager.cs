using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static MapManager instance;
    private string selectedMapId;

    private const string PrefSelectedMap = "pref_selected_map";

    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("[MapManager]");
                    instance = go.AddComponent<MapManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPlayerPrefs();

        Debug.Log($"[MapManager] ✓ Inicializado. Mapa actual: '{selectedMapId}'");
    }

    private void LoadFromPlayerPrefs()
    {
        selectedMapId = PlayerPrefs.GetString(PrefSelectedMap, "");
        if (!string.IsNullOrEmpty(selectedMapId))
        {
            Debug.Log($"[MapManager] ✓ Mapa cargado desde PlayerPrefs: '{selectedMapId}'");
        }
    }

    public static string SelectedMap
    {
        get
        {
            if (string.IsNullOrEmpty(Instance.selectedMapId))
            {
                Instance.LoadFromPlayerPrefs();
            }
            return Instance.selectedMapId;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning("[MapManager] ⚠ Intentando guardar mapId vacío.");
                return;
            }

            Instance.selectedMapId = value;
            PlayerPrefs.SetString(PrefSelectedMap, value);
            PlayerPrefs.Save();
            Debug.Log($"[MapManager] ✓ Mapa seleccionado guardado: '{value}'");
        }
    }

    public static bool HasSelectedMap()
    {
        string map = SelectedMap;
        return !string.IsNullOrEmpty(map);
    }

    public static void ClearSelection()
    {
        Instance.selectedMapId = "";
        PlayerPrefs.DeleteKey(PrefSelectedMap);
        PlayerPrefs.Save();
        Debug.Log("[MapManager] Selección limpiada.");
    }
}