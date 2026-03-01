using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("Fondos de Mapas")]
    [SerializeField] private MapBackground[] mapBackgrounds;

    [Header("Referencias")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [System.Serializable]
    public class MapBackground
    {
        public string mapId;
        public Sprite backgroundSprite;
    }

    private void Start()
    {
        ApplyBackgroundFromSelection();
    }

    public void ApplyBackgroundFromSelection()
    {
        var manager = MapManager.Instance;

        if (!MapManager.HasSelectedMap())
        {
            LogWarning("No se ha seleccionado ningún mapa.");
            return;
        }

        string selectedMap = MapManager.SelectedMap;
        Log($"Mapa obtenido de MapManager: '{selectedMap}'");

        if (string.IsNullOrEmpty(selectedMap))
        {
            LogWarning("El mapa seleccionado está vacío.");
            return;
        }

        ApplyBackground(selectedMap);
    }

    private void ApplyBackground(string mapId)
    {
        if (mapBackgrounds == null || mapBackgrounds.Length == 0)
        {
            LogError("⚠ Array 'mapBackgrounds' está vacío. Configúralo en el Inspector.");
            return;
        }

        Log($"Buscando fondo para mapId: '{mapId}'");
        Log($"Mapas configurados: [{string.Join(", ", System.Array.ConvertAll(mapBackgrounds, bg => $"'{bg.mapId}'"))}]");

        MapBackground mapBg = System.Array.Find(mapBackgrounds, bg => bg.mapId == mapId);

        if (mapBg != null && mapBg.backgroundSprite != null)
        {
            bool applied = false;

            if (backgroundImage != null)
            {
                backgroundImage.sprite = mapBg.backgroundSprite;
                backgroundImage.enabled = true;
                Log($"✓ Sprite '{mapBg.backgroundSprite.name}' aplicado a Image");
                applied = true;
            }

            if (backgroundSpriteRenderer != null)
            {
                backgroundSpriteRenderer.sprite = mapBg.backgroundSprite;
                backgroundSpriteRenderer.enabled = true;
                Log($"✓ Sprite '{mapBg.backgroundSprite.name}' aplicado a SpriteRenderer");
                applied = true;
            }

            if (!applied)
            {
                LogWarning("No hay componentes de fondo asignados (Image o SpriteRenderer).");
            }
            else
            {
                Log($"✓✓✓ FONDO APLICADO CORRECTAMENTE: '{mapId}' -> {mapBg.backgroundSprite.name}");
            }
        }
        else
        {
            if (mapBg == null)
            {
                LogError($"✗ NO SE ENCONTRÓ configuración para mapId: '{mapId}'");
            }
            else if (mapBg.backgroundSprite == null)
            {
                LogError($"✗ El mapId '{mapId}' existe pero NO TIENE SPRITE asignado");
            }
        }
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[BackgroundManager] {message}");
    }

    private void LogWarning(string message)
    {
        if (showDebugLogs)
            Debug.LogWarning($"[BackgroundManager] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[BackgroundManager] {message}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mapBackgrounds != null)
        {
            for (int i = 0; i < mapBackgrounds.Length; i++)
            {
                if (string.IsNullOrEmpty(mapBackgrounds[i].mapId))
                {
                    Debug.LogWarning($"[BackgroundManager] El mapa en índice {i} no tiene mapId asignado.", this);
                }
                if (mapBackgrounds[i].backgroundSprite == null)
                {
                    Debug.LogWarning($"[BackgroundManager] El mapa '{mapBackgrounds[i].mapId}' (índice {i}) no tiene sprite asignado.", this);
                }
            }
        }

        if (backgroundImage == null && backgroundSpriteRenderer == null)
        {
            Debug.LogWarning("[BackgroundManager] No hay componentes de fondo asignados (Image o SpriteRenderer).", this);
        }
    }
#endif
}