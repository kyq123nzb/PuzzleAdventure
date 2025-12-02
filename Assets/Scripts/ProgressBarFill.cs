using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进度条填充控制器 - 支持颜色渐变和宽度控制
/// </summary>
public class ProgressBarFill : MonoBehaviour
{
    [Header("进度条设置")]
    public float maxWidth = 500f; // 最大宽度（100%时的宽度）
    
    [Header("颜色渐变设置")]
    public Gradient colorGradient; // 进度条颜色渐变
    
    [Header("动画设置")]
    public bool smoothAnimation = true;
    public float animationSpeed = 2f;
    
    [Header("完成效果")]
    public bool playCompletionEffect = true;
    public Color completionColor = Color.green;
    public float completionEffectDuration = 2f;
    
    // 组件引用
    private RectTransform rectTransform;
    private Image fillImage;
    
    // 状态变量
    private float targetWidth = 0f;
    private float currentWidth = 0f;
    private bool isPlayingCompletionEffect = false;
    private float currentProgress = 0f;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        fillImage = GetComponent<Image>();
        
        if (rectTransform == null)
        {
            Debug.LogError("ProgressBarFill: 找不到RectTransform组件！");
            return;
        }
        
        if (fillImage == null)
        {
            Debug.LogError("ProgressBarFill: 找不到Image组件！");
            return;
        }
        
        // 获取初始宽度作为最大宽度（如果没设置）
        if (maxWidth <= 0)
        {
            maxWidth = rectTransform.rect.width;
            Debug.Log($"自动获取最大宽度: {maxWidth}");
        }
        
        // 初始化颜色渐变（如果未设置）
        if (colorGradient == null)
        {
            InitializeDefaultGradient();
        }
        
        // 初始设置为0（空进度）
        SetProgress(0f);
    }
    
    void Update()
    {
        // 平滑动画
        if (smoothAnimation && rectTransform != null && !isPlayingCompletionEffect)
        {
            if (Mathf.Abs(currentWidth - targetWidth) > 0.1f)
            {
                currentWidth = Mathf.Lerp(currentWidth, targetWidth, animationSpeed * Time.deltaTime);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
                
                // 根据当前宽度计算进度并更新颜色
                UpdateColorBasedOnWidth();
            }
            else
            {
                currentWidth = targetWidth;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
                UpdateColorBasedOnWidth();
            }
        }
    }
    
    /// <summary>
    /// 初始化默认渐变（红色到绿色）
    /// </summary>
    void InitializeDefaultGradient()
    {
        Debug.Log("使用默认渐变颜色（红->黄->绿）");
        
        // 创建一个简单的红黄绿渐变
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(Color.red, 0.0f);      // 0%: 红色
        colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);   // 50%: 黄色
        colorKeys[2] = new GradientColorKey(Color.green, 1.0f);    // 100%: 绿色
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);  // 开始不透明
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);  // 结束不透明
        
        colorGradient = new Gradient();
        colorGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    /// <summary>
    /// 根据当前宽度更新颜色
    /// </summary>
    void UpdateColorBasedOnWidth()
    {
        if (fillImage == null || colorGradient == null) return;
        
        currentProgress = GetProgress();
        fillImage.color = colorGradient.Evaluate(currentProgress);
    }
    
    /// <summary>
    /// 设置进度（0-1之间）
    /// </summary>
    /// <param name="progress">进度值，0到1之间</param>
    public void SetProgress(float progress)
    {
        if (isPlayingCompletionEffect) return;
        
        progress = Mathf.Clamp01(progress); // 限制在0-1之间
        
        targetWidth = maxWidth * progress;
        currentProgress = progress;
        
        if (!smoothAnimation && rectTransform != null)
        {
            // 不使用动画，直接设置
            currentWidth = targetWidth;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            UpdateColorBasedOnWidth();
        }
        else
        {
            currentWidth = rectTransform != null ? rectTransform.rect.width : 0f;
        }
        
        Debug.Log($"设置进度: {progress}, 目标宽度: {targetWidth}");
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
            
            // 如果进度达到100%，播放完成效果
            if (progress >= 1f && playCompletionEffect)
            {
                PlayCompletionEffect();
            }
        }
        else
        {
            SetProgress(0f);
        }
    }
    
    /// <summary>
    /// 播放完成效果（进度达到100%时）
    /// </summary>
    public void PlayCompletionEffect()
    {
        if (isPlayingCompletionEffect || fillImage == null) return;
        
        StartCoroutine(CompletionEffectCoroutine());
    }
    
    /// <summary>
    /// 完成效果协程
    /// </summary>
    System.Collections.IEnumerator CompletionEffectCoroutine()
    {
        isPlayingCompletionEffect = true;
        
        float elapsedTime = 0f;
        Color originalColor = fillImage.color;
        
        while (elapsedTime < completionEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / completionEffectDuration;
            
            // 彩虹色循环
            Color rainbowColor = Color.HSVToRGB(t % 1, 0.8f, 1f);
            fillImage.color = rainbowColor;
            
            // 轻微脉冲效果
            float pulseScale = 1f + Mathf.Sin(t * Mathf.PI * 4) * 0.05f;
            rectTransform.localScale = new Vector3(pulseScale, 1f, 1f);
            
            yield return null;
        }
        
        // 恢复
        fillImage.color = completionColor;
        rectTransform.localScale = Vector3.one;
        
        isPlayingCompletionEffect = false;
    }
    
    /// <summary>
    /// 获取当前进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        if (maxWidth > 0)
        {
            return Mathf.Clamp01(currentWidth / maxWidth);
        }
        return 0f;
    }
    
    /// <summary>
    /// 重置进度条（归零）
    /// </summary>
    public void ResetProgress()
    {
        targetWidth = 0f;
        currentWidth = 0f;
        currentProgress = 0f;
        
        if (rectTransform != null)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        }
        
        if (fillImage != null && colorGradient != null)
        {
            fillImage.color = colorGradient.Evaluate(0f);
        }
    }
    
    /// <summary>
    /// 设置最大宽度
    /// </summary>
    public void SetMaxWidth(float width)
    {
        maxWidth = width;
        SetProgress(currentProgress); // 重新计算目标宽度
    }
}