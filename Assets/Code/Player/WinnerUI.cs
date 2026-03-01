using UnityEngine;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class WinnerUI : MonoBehaviour
{
    [SerializeField] private GameObject winnerCanvas;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private int sortingOrder = 5000;

    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (winnerCanvas == null)
            Debug.LogWarning("[WinnerUI] winnerCanvas no asignado.");
        if (winnerText == null)
            Debug.LogWarning("[WinnerUI] winnerText no asignado.");

        rootCanvas = GetComponent<Canvas>();
        if (rootCanvas != null)
        {
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = sortingOrder;
            rootCanvas.enabled = true;
        }

        canvasGroup = winnerCanvas != null ? winnerCanvas.GetComponent<CanvasGroup>() : null;
        if (canvasGroup == null && winnerCanvas != null)
            canvasGroup = winnerCanvas.AddComponent<CanvasGroup>();

        if (winnerCanvas != null)
        {
            winnerCanvas.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void ShowWinner(string playerName)
    {
        if (winnerText != null)
            winnerText.text = $"Player {playerName} wins";
        else
            Debug.LogWarning("[WinnerUI] winnerText no asignado.");

        if (winnerCanvas != null)
        {
            Transform t = winnerCanvas.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }

            winnerCanvas.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            Debug.LogWarning("[WinnerUI] winnerCanvas no asignado.");
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (rootCanvas != null)
        {
            rootCanvas.enabled = true;
            rootCanvas.sortingOrder = sortingOrder;
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Debug.Log("[WinnerUI] Canvas de ganador mostrado.");
    }
}