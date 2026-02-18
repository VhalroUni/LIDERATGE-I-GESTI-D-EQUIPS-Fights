using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Input (New Input System)")]
    [SerializeField] private InputAction m_PauseKeyInput;

    private bool m_IsPaused;

    private void Awake()
    {
        m_PauseKeyInput.performed += OnPausePressed;
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
        m_IsPaused = true;

        Time.timeScale = 0f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        m_IsPaused = false;

        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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