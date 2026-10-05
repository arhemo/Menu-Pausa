using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject unsavedChangesPanel;

    [Header("Settings")]
    [SerializeField] private SettingsManager settingsManager;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.2f;

    private UIControlsActions.UIControlsActions inputActions;

    private bool isPaused;
    private bool isTransitioning;

    private void Awake()
    {
        inputActions = new UIControlsActions.UIControlsActions();

        inputActions.UI.Cancel.performed += OnPausePressed;

        pauseRoot.SetActive(false);
        unsavedChangesPanel.SetActive(false);

        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions.UI.Cancel.performed -= OnPausePressed;
        inputActions.Dispose();
    }

    private void OnPausePressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isTransitioning)
            return;

        if (isPaused)
            TryClosePauseMenu();
        else
            OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        if (isPaused)
            return;

        isPaused = true;

        settingsManager.OpenSettingsSession();

        pauseRoot.SetActive(true);

        Time.timeScale = 0f;

        StopAllCoroutines();
        StartCoroutine(FadeCanvas(0f, 1f, true));
    }

    public void TryClosePauseMenu()
    {
        if (settingsManager.HasUnappliedChanges())
        {
            unsavedChangesPanel.SetActive(true);
            return;
        }

        ClosePauseMenu();
    }

    public void ConfirmExitWithoutApplying()
    {
        unsavedChangesPanel.SetActive(false);

        settingsManager.DiscardChanges();

        ClosePauseMenu();
    }

    public void CancelExit()
    {
        unsavedChangesPanel.SetActive(false);
    }

    public void ClosePauseMenu()
    {
        if (!isPaused)
            return;

        unsavedChangesPanel.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(FadeCanvas(1f, 0f, false));
    }

    private IEnumerator FadeCanvas(float start, float end, bool opening)
    {
        isTransitioning = true;

        float elapsed = 0f;

        if (opening)
        {
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            pauseCanvasGroup.alpha = Mathf.Lerp(start, end, t);

            yield return null;
        }

        pauseCanvasGroup.alpha = end;

        if (opening)
        {
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;

            pauseRoot.SetActive(false);

            Time.timeScale = 1f;
            isPaused = false;
        }

        isTransitioning = false;
    }
}