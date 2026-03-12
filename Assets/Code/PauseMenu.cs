using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Transición")]
    [SerializeField] public CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputAction m_PauseKeyInput;

    private bool m_IsPaused;
    private bool isTransitioning;

    private void Awake()
    {
        m_PauseKeyInput.performed += OnPausePressed;
        ConfigureFadeCanvas();
    }

    private void OnEnable()
    {
        m_PauseKeyInput.Enable();
    }

    private void OnDisable()
    {
        m_PauseKeyInput.Disable();
    }

    private void OnDestroy()
    {
        m_PauseKeyInput.performed -= OnPausePressed;
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (!m_IsPaused)
            Pause();
        else
            Resume();
    }

    public void Pause()
    {
        if (fadeCanvasGroup == null)
        {
            PauseImmediate();
            return;
        }

        StartCoroutine(PauseRoutine());
    }

    public void Resume()
    {
        if (fadeCanvasGroup == null)
        {
            ResumeImmediate();
            return;
        }

        StartCoroutine(ResumeRoutine());
    }

    private void PauseImmediate()
    {
        m_IsPaused = true;
        Time.timeScale = 0f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeImmediate()
    {
        m_IsPaused = false;
        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator PauseRoutine()
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        yield return FadeCanvas(0f, 1f);
        PauseImmediate();
        yield return FadeCanvas(1f, 0f);

        isTransitioning = false;
    }

    private IEnumerator ResumeRoutine()
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        yield return FadeCanvas(0f, 1f);
        ResumeImmediate();
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

    private IEnumerator FadeCanvas(float from, float to)
    {
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

    public void OnResumeButton()
    {
        Resume();
    }

    public void OnExitToDesktopButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}