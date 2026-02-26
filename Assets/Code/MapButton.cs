using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    [Header("Configuración del Mapa")]
    [SerializeField] private string mapId;

    [Header("Referencias Visuales (Opcional)")]
    [SerializeField] private Image previewImage;
    [SerializeField] private Text mapNameText;

    private Button button;
    private MainMenu mainMenu;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogError("[MapButton] No se encontró componente Button.");
        }
    }

    private void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();

        if (mainMenu == null)
        {
            Debug.LogError("[MapButton] No se encontró MainMenu en la escena.");
        }

        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError($"[MapButton] El botón '{gameObject.name}' no tiene mapId configurado.");
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("[MapButton] No se puede seleccionar un mapa sin mapId.");
            return;
        }

        if (mainMenu != null)
        {
            Debug.Log($"[MapButton] Botón presionado: {mapId}");
            mainMenu.OnMapSelected(mapId);
        }
        else
        {
            Debug.LogError("[MapButton] MainMenu no está disponible.");
        }
    }

    public void SetMapId(string newMapId)
    {
        mapId = newMapId;
    }

    public string GetMapId()
    {
        return mapId;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogWarning($"[MapButton] El botón '{gameObject.name}' necesita un mapId asignado.");
        }
    }
#endif
}