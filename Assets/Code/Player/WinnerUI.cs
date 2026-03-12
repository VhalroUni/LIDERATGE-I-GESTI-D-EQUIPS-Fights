using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class WinnerUI : MonoBehaviour
{
    [SerializeField] private GameObject winnerCanvas;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private int sortingOrder = 5000;

    [Header("Escena")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Transición")]
    [SerializeField] public CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private bool isTransitioning;
    private bool isWinnerShown;
    private bool isReturningToMenu;

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

        isWinnerShown = false;
        isReturningToMenu = false;
        ConfigureFadeCanvas();
        EnsureFadeCanvasOnTop();
    }

    private void Update()
    {
        if (!isWinnerShown || isReturningToMenu)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }

    public void ShowWinner(string playerName)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!isActiveAndEnabled)
        {
            ShowWinnerImmediate(playerName);
            return;
        }

        if (isTransitioning)
            return;

        if (fadeCanvasGroup == null)
        {
            ShowWinnerImmediate(playerName);
            return;
        }

        StartCoroutine(ShowWinnerRoutine(playerName));
    }

    private void ShowWinnerImmediate(string playerName)
    {
        if (winnerText != null)
            winnerText.text = $"Player {playerName} \n wins";
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

        isWinnerShown = true;
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

    private void ReturnToMainMenu()
    {
        if (isReturningToMenu)
            return;

        if (string.IsNullOrWhiteSpace(mainMenuScene))
        {
            Debug.LogError("[WinnerUI] 'mainMenuScene' no está configurado.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            Debug.LogError($"[WinnerUI] La escena '{mainMenuScene}' no está en Build Settings.");
            Debug.LogError("Añádela en: File > Build Settings > Scenes In Build");
            return;
        }

        if (fadeCanvasGroup == null)
        {
            LoadMainMenuImmediate();
            return;
        }

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isReturningToMenu = true;

        yield return FadeCanvas(0f, 1f);

        LoadMainMenuImmediate();
    }

    private void LoadMainMenuImmediate()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    }

    private IEnumerator ShowWinnerRoutine(string playerName)
    {
        isTransitioning = true;

        yield return FadeCanvas(0f, 1f);

        ShowWinnerImmediate(playerName);

        yield return FadeCanvas(1f, 0f);

        isTransitioning = false;
    }

    private void ConfigureFadeCanvas()
    {
        if (fadeCanvasGroup == null) return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private void EnsureFadeCanvasOnTop()
    {
        if (fadeCanvasGroup == null) return;

        Canvas fadeCanvas = fadeCanvasGroup.GetComponentInParent<Canvas>();
        if (fadeCanvas == null) return;

        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.overrideSorting = true;
        fadeCanvas.sortingOrder = sortingOrder + 1;
    }

    private IEnumerator FadeCanvas(float from, float to)
    {
        EnsureFadeCanvasOnTop();

        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;

        fadeCanvasGroup.alpha = from;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = false;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;

        if (Mathf.Approximately(to, 0f))
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}