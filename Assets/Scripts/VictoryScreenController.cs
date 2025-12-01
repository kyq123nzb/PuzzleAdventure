using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 胜利界面控制器 - 处理胜利界面的显示和动画
/// </summary>
public class VictoryScreenController : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject victoryPanel;
    public Text victoryTitleText;
    public Text victoryMessageText;
    public Text completionStatsText;
    public Button restartButton;
    public Button quitToMenuButton;
    public Button quitGameButton;
    
    [Header("动画设置")]
    public bool useFadeAnimation = true;
    public bool useScaleAnimation = true;
    public float fadeSpeed = 1.5f;
    public float scaleSpeed = 2f;
    
    [Header("文本设置")]
    public string victoryTitle = "恭喜通关！";
    public string victoryMessage = "你成功收集了所有拼图！";
    
    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private bool isVisible = false;
    
    void Start()
    {
        // 获取或添加CanvasGroup
        canvasGroup = victoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null && useFadeAnimation)
        {
            canvasGroup = victoryPanel.AddComponent<CanvasGroup>();
        }
        
        panelRect = victoryPanel.GetComponent<RectTransform>();
        
        // 设置初始状态
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            if (panelRect != null && useScaleAnimation)
            {
                panelRect.localScale = Vector3.zero;
            }
        }
        
        SetupButtons();
        SetupText();
    }
    
    void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);
        
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }
    
    void SetupText()
    {
        if (victoryTitleText != null)
            victoryTitleText.text = victoryTitle;
        
        if (victoryMessageText != null)
            victoryMessageText.text = victoryMessage;
        
        UpdateStatsText();
    }
    
    void UpdateStatsText()
    {
        if (completionStatsText != null && GameManager.Instance != null)
        {
            int collected = GameManager.Instance.GetCollectedPuzzles();
            int total = GameManager.Instance.totalPuzzles;
            completionStatsText.text = $"收集进度: {collected}/{total} 拼图";
        }
    }
    
    public void Show()
    {
        if (isVisible) return;
        
        isVisible = true;
        UpdateStatsText();
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            if (useFadeAnimation || useScaleAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                if (panelRect != null)
                {
                    panelRect.localScale = Vector3.one;
                }
            }
        }
    }
    
    IEnumerator ShowAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // 淡入和缩放动画
        float elapsedTime = 0f;
        float duration = 1f / Mathf.Max(fadeSpeed, scaleSpeed);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            
            // 淡入
            if (useFadeAnimation && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }
            
            // 缩放
            if (useScaleAnimation && panelRect != null)
            {
                float scale = Mathf.SmoothStep(0f, 1f, progress);
                panelRect.localScale = new Vector3(scale, scale, 1f);
            }
            
            yield return null;
        }
        
        // 确保最终状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
        }
    }
    
    public void Hide()
    {
        isVisible = false;
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            if (panelRect != null)
            {
                panelRect.localScale = Vector3.zero;
            }
        }
    }
    
    void OnRestartClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RestartGame();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
