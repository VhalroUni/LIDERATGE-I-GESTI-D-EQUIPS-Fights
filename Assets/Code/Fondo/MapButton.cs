using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración del Mapa")]
    [SerializeField] private string mapId;

    [Header("Referencias Visuales (Opcional)")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text mapNameText;

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

        SetMapNameVisible(false);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetMapNameVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetMapNameVisible(false);
    }

    private void SetMapNameVisible(bool visible)
    {
        if (mapNameText != null)
        {
            mapNameText.gameObject.SetActive(visible);
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