using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进度条填充控制器 - 不依赖Image Type，直接控制宽度
/// </summary>
public class ProgressBarFill : MonoBehaviour
{
    [Header("进度条设置")]
    public float maxWidth = 100f; // 最大宽度（100%时的宽度）
    
    [Header("动画设置")]
    public bool smoothAnimation = true;
    public float animationSpeed = 2f;
    
    private RectTransform rectTransform;
    private float targetWidth = 0f;
    private float currentWidth = 0f;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("ProgressBarFill: 找不到RectTransform组件！");
            return;
        }
        
        // 获取初始宽度作为最大宽度（如果没设置）
        if (maxWidth <= 0)
        {
            maxWidth = rectTransform.rect.width;
        }
        
        // 初始设置为0（空进度）
        SetProgress(0f);
    }
    
    void Update()
    {
        // 平滑动画
        if (smoothAnimation && rectTransform != null)
        {
            if (Mathf.Abs(currentWidth - targetWidth) > 0.1f)
            {
                currentWidth = Mathf.Lerp(currentWidth, targetWidth, animationSpeed * Time.deltaTime);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            }
            else
            {
                currentWidth = targetWidth;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            }
        }
    }
    
    /// <summary>
    /// 设置进度（0-1之间）
    /// </summary>
    /// <param name="progress">进度值，0到1之间</param>
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress); // 限制在0-1之间
        
        targetWidth = maxWidth * progress;
        
        if (!smoothAnimation && rectTransform != null)
        {
            // 不使用动画，直接设置
            currentWidth = targetWidth;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
        }
        else
        {
            currentWidth = rectTransform != null ? rectTransform.rect.width : 0f;
        }
    }
    
    /// <summary>
    /// 设置进度（通过已收集数和总数）
    /// </summary>
    /// <param name="collected">已收集数量</param>
    /// <param name="total">总数量</param>
    public void SetProgress(int collected, int total)
    {
        if (total > 0)
        {
            float progress = (float)collected / total;
            SetProgress(progress);
        }
        else
        {
            SetProgress(0f);
        }
    }
    
    /// <summary>
    /// 获取当前进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        if (maxWidth > 0)
        {
            return targetWidth / maxWidth;
        }
        return 0f;
    }
}
