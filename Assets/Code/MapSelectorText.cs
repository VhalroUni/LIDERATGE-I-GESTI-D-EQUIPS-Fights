using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using TMPro;

public class MapSelectorText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text canvasText;
    [TextArea] public string stageName;
    public string stageText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        canvasText.text = stageName;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canvasText.text = stageText;
    }
}
