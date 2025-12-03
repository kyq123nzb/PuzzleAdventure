using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼图收集进度UI - 管理进度条的显示和动画
/// </summary>
public class PuzzleProgressUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject progressText; // 支持Text和TextMeshPro
    public Image progressFillImage;
    public Image progressBackgroundImage;
    
    [Header("动画设置")]
    public float fillAnimationSpeed = 2f;
    public bool animateProgress = true;
    
    [Header("颜色设置")]
    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color fullColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Gradient progressGradient;
    
    private float targetFillAmount = 0f;
    private int currentCollected = 0;
    private int currentTotal = 0;
    
    void Start()
    {
        // 初始化进度条
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = 0f;
        }
        
        UpdateProgress(0, GameManager.Instance != null ? GameManager.Instance.TotalPuzzles : 9);
    }
    
    void Update()
    {
        // 平滑动画填充进度条
        if (animateProgress && progressFillImage != null)
        {
            float currentFill = progressFillImage.fillAmount;
            if (Mathf.Abs(currentFill - targetFillAmount) > 0.01f)
            {
                progressFillImage.fillAmount = Mathf.Lerp(currentFill, targetFillAmount, 
                    fillAnimationSpeed * Time.deltaTime);
                UpdateProgressColor();
            }
            else
            {
                progressFillImage.fillAmount = targetFillAmount;
            }
        }
    }
    
    public void UpdateProgress(int collected, int total)
    {
        currentCollected = collected;
        currentTotal = total;
        
        if (total > 0)
        {
            targetFillAmount = (float)collected / total;
        }
        else
        {
            targetFillAmount = 0f;
        }
        
        // 如果不使用动画，直接设置
        if (!animateProgress && progressFillImage != null)
        {
            progressFillImage.fillAmount = targetFillAmount;
            UpdateProgressColor();
        }
        
        // 更新文本（支持Text和TextMeshPro）
        if (progressText != null)
        {
            string textContent = $"Puzzles Collected: {collected}/{total}";
            
            // 尝试使用TextMeshPro（新版本）
            TMPro.TextMeshProUGUI tmpText = progressText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = textContent;
            }
            else
            {
                // 尝试使用传统Text组件
                Text textComponent = progressText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = textContent;
                }
            }
        }
    }
    
    void UpdateProgressColor()
    {
        if (progressFillImage == null) return;
        
        // 使用渐变色
        if (progressGradient != null && progressGradient.colorKeys.Length > 0)
        {
            progressFillImage.color = progressGradient.Evaluate(targetFillAmount);
        }
        else
        {
            // 使用简单插值
            progressFillImage.color = Color.Lerp(emptyColor, fullColor, targetFillAmount);
        }
    }
    
    // 从GameManager同步更新
    public void SyncWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            UpdateProgress(GameManager.Instance.GetCollectedPuzzlesCount(), 
                          GameManager.Instance.TotalPuzzles);
        }
    }
}