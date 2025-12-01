using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单控制器 - 处理暂停菜单的显示和功能
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button quitToMenuButton;
    public Button quitGameButton;
    
    [Header("动画设置")]
    public bool useFadeAnimation = true;
    public float fadeSpeed = 2f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    void Start()
    {
        // 获取或添加CanvasGroup用于淡入淡出
        canvasGroup = pausePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null && useFadeAnimation)
        {
            canvasGroup = pausePanel.AddComponent<CanvasGroup>();
        }
        
        // 设置初始状态
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        
        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);
        
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }
    
    public void Show()
    {
        isVisible = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            if (canvasGroup != null && useFadeAnimation)
            {
                StartCoroutine(FadeIn());
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
    
    public void Hide()
    {
        isVisible = false;
        if (pausePanel != null)
        {
            if (canvasGroup != null && useFadeAnimation)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                pausePanel.SetActive(false);
            }
        }
    }
    
    IEnumerator FadeIn()
    {
        canvasGroup.interactable = false;
        float targetAlpha = 1f;
        
        while (canvasGroup.alpha < targetAlpha)
        {
            canvasGroup.alpha += fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        float targetAlpha = 0f;
        
        while (canvasGroup.alpha > targetAlpha)
        {
            canvasGroup.alpha -= fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        pausePanel.SetActive(false);
    }
    
    void OnResumeClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResumeGame();
        }
    }
    
    void OnQuitToMenuClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.QuitToMainMenu();
        }
    }
    
    void OnQuitGameClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.QuitGame();
        }
    }
}
